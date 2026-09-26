using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.Core;
using GemRacer.Mining;

namespace GemRacer.UI
{
    /// <summary>
    /// M-14 ③(2026-09-26): 시즌 패스 화면. core `SeasonPass.cs`(M-10)가 11일째 아무 데도 안 불리던
    /// 마지막 조각이다 — ①XP 배선(레이스 우승마다 지급)과 ②SKU(`ShopSkuId.SeasonPassPaidTrack`)는
    /// 이미 끝났고(MiningController.TryEnterRace/DebugPurchase), 여기서 그 값을 실제로 보여주고
    /// 받는 버튼을 단다.
    ///
    /// `DefaultData.SeasonPassTiers()`가 10티어를 주고, 티어마다 무료·유료 보상이 하나씩 있다
    /// (지금은 전부 채워져 있다 — 빈 레벨이 없다). ShopUgui가 DefaultData.ShopItems()를 그대로
    /// 읽어 이름·가격을 찍는 것과 같은 방식으로, 여기도 티어 열 개의 텍스트는 Awake에서
    /// DefaultData로 채우고 씬(부트스트랩)의 기본 문구는 자리 표시자로만 쓴다.
    ///
    /// 이름 규칙은 tier{level}-*(title/xp/free-text/free-button/paid-text/paid-button) — 레벨이
    /// 그대로 인덱스다(ShopUgui의 Prefixes 배열과 달리 순서 걱정 없이 레벨 번호로 바로 찾는다).
    /// </summary>
    public sealed class SeasonPassUgui : MonoBehaviour
    {
        [Tooltip("읽고 쓸 대상. 비워두면 씬에서 하나 찾는다.")]
        public MiningController target;

        static readonly SeasonPassTier[] Tiers = DefaultData.SeasonPassTiers();

        TMP_Text _levelLabel;
        TMP_Text _paidTrackState;
        Button _paidTrackButton;
        TMP_Text _paidTrackButtonLabel;

        TMP_Text[] _freeText;
        Button[] _freeButton;
        TMP_Text[] _freeButtonLabel;
        TMP_Text[] _paidText;
        Button[] _paidButton;
        TMP_Text[] _paidButtonLabel;

        void Awake()
        {
            if (target == null) target = FindFirstObjectByType<MiningController>();

            _levelLabel = UiKit.Find<TMP_Text>(transform, "level-label");
            _paidTrackState = UiKit.Find<TMP_Text>(transform, "paidtrack-state");
            _paidTrackButton = UiKit.Find<Button>(transform, "paidtrack-button");
            _paidTrackButtonLabel = _paidTrackButton != null ? _paidTrackButton.GetComponentInChildren<TMP_Text>() : null;
            _paidTrackButton?.onClick.AddListener(() => target?.DebugPurchase(ShopSkuId.SeasonPassPaidTrack));

            var n = Tiers.Length;
            _freeText = new TMP_Text[n];
            _freeButton = new Button[n];
            _freeButtonLabel = new TMP_Text[n];
            _paidText = new TMP_Text[n];
            _paidButton = new Button[n];
            _paidButtonLabel = new TMP_Text[n];

            for (int i = 0; i < n; i++)
            {
                var tier = Tiers[i];
                var prefix = $"tier{tier.Level}";

                var title = UiKit.Find<TMP_Text>(transform, $"{prefix}-title", warnIfMissing: false);
                if (title != null) title.text = $"레벨 {tier.Level}";
                var xp = UiKit.Find<TMP_Text>(transform, $"{prefix}-xp", warnIfMissing: false);
                if (xp != null) xp.text = $"{tier.RequiredXp} XP";

                _freeText[i] = UiKit.Find<TMP_Text>(transform, $"{prefix}-free-text", warnIfMissing: false);
                if (_freeText[i] != null) _freeText[i].text = RewardText(tier.FreeReward);
                _freeButton[i] = UiKit.Find<Button>(transform, $"{prefix}-free-button", warnIfMissing: false);
                _freeButtonLabel[i] = _freeButton[i] != null ? _freeButton[i].GetComponentInChildren<TMP_Text>() : null;

                _paidText[i] = UiKit.Find<TMP_Text>(transform, $"{prefix}-paid-text", warnIfMissing: false);
                if (_paidText[i] != null) _paidText[i].text = RewardText(tier.PaidReward);
                _paidButton[i] = UiKit.Find<Button>(transform, $"{prefix}-paid-button", warnIfMissing: false);
                _paidButtonLabel[i] = _paidButton[i] != null ? _paidButton[i].GetComponentInChildren<TMP_Text>() : null;

                var level = tier.Level; // 람다가 반복 변수를 그대로 캡처하지 않게 지역 변수로 고정(ShopUgui와 같은 이유)
                _freeButton[i]?.onClick.AddListener(() => OnClaimClicked(SeasonPassTrack.Free, level));
                _paidButton[i]?.onClick.AddListener(() => OnClaimClicked(SeasonPassTrack.Paid, level));
            }
        }

        // 레벨은 XP를 얻을 때만 바뀌지만(레이스 우승), 유료 트랙 구매·수령은 이 화면 자체에서
        // 바로 일어나니 ShopUgui와 같은 이유로 매 프레임 다시 그린다.
        void Update() => Refresh();

        void Refresh()
        {
            if (target == null) return;
            var state = target.SeasonPass;
            var level = SeasonPassProgress.LevelForXp(Tiers, state.CurrentXp);
            var lastTier = Tiers[Tiers.Length - 1];

            if (_levelLabel != null)
            {
                _levelLabel.text = level >= lastTier.Level
                    ? $"레벨 {level} (최고 레벨) · {state.CurrentXp} XP"
                    : $"레벨 {level} · {state.CurrentXp}/{Tiers[level].RequiredXp} XP";
            }

            if (_paidTrackState != null) _paidTrackState.text = state.OwnsPaidTrack ? "보유 중" : "미보유";
            if (_paidTrackButton != null) _paidTrackButton.interactable = !state.OwnsPaidTrack;
            if (_paidTrackButtonLabel != null)
                _paidTrackButtonLabel.text = state.OwnsPaidTrack ? "구매함" : "유료 트랙 구매";

            for (int i = 0; i < Tiers.Length; i++)
            {
                var tierLevel = Tiers[i].Level;
                SetButtonState(_freeButton[i], _freeButtonLabel[i], state, SeasonPassTrack.Free, tierLevel);
                SetButtonState(_paidButton[i], _paidButtonLabel[i], state, SeasonPassTrack.Paid, tierLevel);
            }
        }

        void SetButtonState(Button button, TMP_Text label, SeasonPassState state, SeasonPassTrack track, int level)
        {
            if (button == null) return;
            button.interactable = SeasonPassProgress.CanClaim(Tiers, state, track, level);
            if (label != null) label.text = StateLabel(state, track, level);
        }

        static string StateLabel(SeasonPassState state, SeasonPassTrack track, int level)
        {
            if (SeasonPassProgress.IsClaimed(state, track, level)) return "받음";
            if (track == SeasonPassTrack.Paid && !state.OwnsPaidTrack) return "잠김";
            if (SeasonPassProgress.LevelForXp(Tiers, state.CurrentXp) < level) return "레벨 부족";
            return "받기";
        }

        void OnClaimClicked(SeasonPassTrack track, int level)
        {
            if (target != null && target.TryClaimSeasonPassReward(track, level, out _)) Refresh();
        }

        static string RewardText(SeasonPassReward? reward)
        {
            if (!reward.HasValue) return "-";
            var r = reward.Value;
            switch (r.Kind)
            {
                case SeasonPassRewardKind.RawMinerals: return $"원석 {r.Amount:0}";
                case SeasonPassRewardKind.RefinedMinerals: return $"정제 광물 {r.Amount:0}";
                case SeasonPassRewardKind.RigPart: return $"{SlotNameKo(r.RigSlot)} 부품";
                case SeasonPassRewardKind.LootBox: return $"{BoxNameKo(r.LootBox)} 상자";
                case SeasonPassRewardKind.CargoCapBoostHours: return $"화물칸 2배 {r.Amount:0}시간";
                case SeasonPassRewardKind.Cosmetic: return "스킨";
                default: return "-";
            }
        }

        static string SlotNameKo(RigSlot slot) => slot switch
        {
            RigSlot.Tool => "곡괭이",
            RigSlot.Cargo => "화물칸",
            RigSlot.Engine => "엔진",
            RigSlot.Detector => "탐지기",
            RigSlot.Refinery => "제련소",
            _ => slot.ToString(),
        };

        static string BoxNameKo(LootBoxType box) => box switch
        {
            LootBoxType.Rusty => "녹슨",
            LootBoxType.Steel => "강철",
            LootBoxType.Titanium => "티타늄",
            _ => box.ToString(),
        };
    }
}
