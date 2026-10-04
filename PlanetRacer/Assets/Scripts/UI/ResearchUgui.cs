using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.Core;
using GemRacer.Mining;

namespace GemRacer.UI
{
    /// <summary>
    /// E-06 ②(2026-10-04): 연구소 화면. 규칙은 전부 코어(Research·ResearchState)에 있고,
    /// 문구는 ResearchLabels가 만든다 — 여기는 MiningController 글루를 불러 TMP에 넣을 뿐이다.
    /// 루트는 항상 켜 두고(Update가 끝난 연구를 거둬야 한다) backdrop만 여닫는다.
    /// 여는 길은 `Open()` — HUD 버튼 배선은 Unity 세션 몫이다(버튼 줄이 이미 꽉 차서
    /// backlog는 "강화 화면 안 탭"을 권했다. 그때 그 탭 버튼의 onClick에 이 Open을 걸면 된다).
    /// 다섯 줄은 `research-row-0..4`, 안의 글자는 이름으로 찾는다(ResearchKind 순서와 같다).
    /// </summary>
    public sealed class ResearchUgui : MonoBehaviour
    {
        public const int RowCount = 5;

        [Tooltip("연구를 걸 대상. 비워두면 씬에서 하나 찾는다.")]
        public MiningController target;

        sealed class Row
        {
            public TMP_Text Name, Level, Effect, Status;
            public Button Button;
            public TMP_Text ButtonLabel;
        }

        GameObject _backdrop;
        TMP_Text _moneyLabel;
        readonly Row[] _rows = new Row[RowCount];

        void Awake()
        {
            if (target == null) target = FindFirstObjectByType<MiningController>();

            _backdrop = UiKit.FindObject(transform, "research-backdrop");
            _moneyLabel = UiKit.Find<TMP_Text>(transform, "research-money-label");
            UiKit.Find<Button>(transform, "research-close-button")?.onClick.AddListener(Close);

            for (var i = 0; i < RowCount; i++)
            {
                var kind = (ResearchKind)i;
                var rowRoot = UiKit.FindObject(transform, $"research-row-{i}");
                if (rowRoot == null) continue;
                var row = new Row
                {
                    Name = UiKit.Find<TMP_Text>(rowRoot.transform, "name"),
                    Level = UiKit.Find<TMP_Text>(rowRoot.transform, "level"),
                    Effect = UiKit.Find<TMP_Text>(rowRoot.transform, "effect"),
                    Status = UiKit.Find<TMP_Text>(rowRoot.transform, "status"),
                    Button = UiKit.Find<Button>(rowRoot.transform, "start-button"),
                };
                row.ButtonLabel = row.Button != null ? row.Button.GetComponentInChildren<TMP_Text>() : null;
                if (row.Name != null) row.Name.text = ResearchLabels.Name(kind);
                row.Button?.onClick.AddListener(() => StartResearch(kind));
                _rows[i] = row;
            }

            if (_backdrop != null) _backdrop.SetActive(false);
        }

        public void Open()
        {
            if (_backdrop != null) _backdrop.SetActive(true);
            Refresh();
        }

        public void Close()
        {
            if (_backdrop != null) _backdrop.SetActive(false);
        }

        void Update()
        {
            if (target == null) return;
            target.CollectResearch(); // 열려 있지 않아도 끝난 연구는 거둔다 — 효과가 바로 켜져야 한다
            if (_backdrop != null && _backdrop.activeSelf) Refresh();
        }

        void StartResearch(ResearchKind kind)
        {
            if (target != null) target.TryStartResearch(kind);
            Refresh();
        }

        void Refresh()
        {
            if (target == null) return;
            var state = target.ResearchStateView;
            var money = target.ResearchMoney;
            var now = (double)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var slots = Math.Max(1, target.Entitlements.ResearchSlots);
            var slotsFull = state.Active.Count >= slots;

            if (_moneyLabel != null) _moneyLabel.text = $"돈 {money:N0}  ·  연구 슬롯 {state.Active.Count}/{slots}";

            for (var i = 0; i < RowCount; i++)
            {
                var row = _rows[i];
                if (row == null) continue;
                var kind = (ResearchKind)i;
                var level = state.GetLevel(kind);
                var max = level >= Research.MaxLevel(kind);
                var running = state.IsRunning(kind);

                if (row.Level != null) row.Level.text = ResearchLabels.Level(kind, level);
                if (row.Effect != null) row.Effect.text = ResearchLabels.Effect(kind, level);

                string status;
                if (max) status = "더 올릴 수 없다";
                else if (running) status = $"연구 중 — 남은 {ResearchLabels.Duration(state.RemainingSeconds(kind, now))}";
                else status = $"{Research.CostToNext(kind, level):N0} · {ResearchLabels.Duration(Research.SecondsToNext(kind, level))}";
                if (row.Status != null) row.Status.text = status;

                var canStart = !max && !running && !slotsFull && money >= Research.CostToNext(kind, level);
                if (row.Button != null) row.Button.interactable = canStart;
                if (row.ButtonLabel != null)
                    row.ButtonLabel.text = max ? "최대" : running ? "연구 중" : slotsFull ? "슬롯 가득" : money < Research.CostToNext(kind, level) ? "돈 부족" : "연구";
            }
        }
    }
}
