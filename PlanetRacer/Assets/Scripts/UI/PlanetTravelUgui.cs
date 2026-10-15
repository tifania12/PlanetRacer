using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.Core;
using GemRacer.Mining;

namespace GemRacer.UI
{
    /// <summary>
    /// P-09(2026-09-25 야간 세션, 코드만 — 씬 배선은 Unity 세션 몫): 행성 선택·이동 화면.
    /// DefaultData.Planets() 순서 그대로 한 줄씩 보여주고, 지금 있는 행성은 "현재 위치"로 표시,
    /// 나머지는 "이동" 버튼을 누르면 MiningController.TravelTo가 실제로 바꾼다.
    ///
    /// **해금(P-08)은 아직 Tifania 결정 대기라 이 화면은 전부 이동 가능하게 둔다** — planet-
    /// progression.md 4절 "이동은 되돌아갈 수 있어야 한다"는 이미 정해진 규칙이고, "어디까지
    /// 열려 있는가"만 미정이다. 나중에 해금 조건이 정해지면 각 줄에 잠금 표시만 얹으면 되고,
    /// 이 화면의 나머지 구조(줄 찾기·이동 호출)는 안 바뀐다.
    ///
    /// RaceEntryUgui와 같은 역할 분담 — 판단(TravelTo가 실제로 무엇을 들고 가고 무엇을 버릴지)은
    /// MiningController/코어가 끝내고, 여기는 목록을 그리고 클릭을 전달할 뿐이다.
    /// </summary>
    public sealed class PlanetTravelUgui : MonoBehaviour
    {
        /// <summary>A-28-2: 이 화면의 주 버튼은 줄마다 있는 「이동」 칸이다.
        /// 아래 「닫기」는 빠져나오는 길이라 보조 버튼으로 둔다 — 앞 열두 화면과 같다.
        ///
        /// 다른 화면처럼 배열을 상수로 못 박지 못하는 이유는 **줄 개수가 행성 수를 따라가기**
        /// 때문이다(`BootstrapPlanetTravelUgui`가 `DefaultData.Planets().Count`만큼 세운다).
        /// 그래서 이름도 같은 수만큼 그 자리에서 만든다 — 행성이 늘어도 이 코드와 부트스트랩
        /// 둘 다 안 고치고 메뉴만 다시 누르면 된다(CLAUDE.md 3번, 멱등).
        /// 부트스트랩(`GemRacer/32`)도 이 함수를 그대로 쓴다.</summary>
        public static string[] MakePrimaryButtons()
        {
            var count = DefaultData.Planets().Count;
            var names = new string[count];
            for (int i = 0; i < count; i++) names[i] = $"planet-{i}-button";
            return names;
        }

        [Tooltip("이동 대상. 비워두면 씬에서 하나 찾는다.")]
        public MiningController target;

        TMP_Text[] _nameLabels;
        TMP_Text[] _infoLabels;
        Button[] _travelButtons;
        int _planetCount;

        void Awake()
        {
            if (target == null) target = FindFirstObjectByType<MiningController>();

            _planetCount = DefaultData.Planets().Count;
            _nameLabels = new TMP_Text[_planetCount];
            _infoLabels = new TMP_Text[_planetCount];
            _travelButtons = new Button[_planetCount];

            for (int i = 0; i < _planetCount; i++)
            {
                _nameLabels[i] = UiKit.Find<TMP_Text>(transform, $"planet-{i}-name");
                _infoLabels[i] = UiKit.Find<TMP_Text>(transform, $"planet-{i}-info");
                var button = UiKit.Find<Button>(transform, $"planet-{i}-button");
                _travelButtons[i] = button;
                if (button != null)
                {
                    var planetId = DefaultData.Planets()[i].Id; // 람다가 반복 변수를 그대로 캡처하지 않게 지역 변수로 고정
                    button.onClick.AddListener(() => OnTravelButtonClicked(planetId));
                }
            }

            var closeButton = UiKit.Find<Button>(transform, "close-button");
            closeButton?.onClick.AddListener(() => GetComponent<UiPanel>()?.Hide());

            // A-28-2: 키트 스킨을 여기서도 한 번 입힌다 — 부트스트랩(`GemRacer/32`)과
            // **같은 함수**다. 씬을 다시 저장하지 못한 채로 빌드가 나가도 회색 상자로
            // 돌아가지 않게(앞 열두 화면과 같은 방식).
            //
            // 카드 이름을 비운다 — 루트 Image가 화면을 꽉 채우는 가림막이라 둥근 창을 얹을
            // 자리가 없다(레이스 출전·상자·제작·도감과 같은 이유).
            UiSkin.ApplyToPopup(transform, null, MakePrimaryButtons());
        }

        // 패널이 열릴 때마다 지금 위치가 바뀌어 있을 수 있으니 다시 그린다.
        void OnEnable() => Refresh();

        void Refresh()
        {
            if (target == null) return;
            var planets = DefaultData.Planets();
            var currentId = target.CurrentPlanet?.Id;

            for (int i = 0; i < _planetCount; i++)
            {
                var p = planets[i];
                var isCurrent = p.Id == currentId;

                if (_nameLabels[i] != null) _nameLabels[i].text = p.NameKo;
                if (_infoLabels[i] != null)
                {
                    var mineralName = string.IsNullOrEmpty(p.MineralNameKo) ? "(이름 없음)" : p.MineralNameKo;
                    if (isCurrent)
                    {
                        _infoLabels[i].text = $"{mineralName} · 지금 있는 곳";
                    }
                    else
                    {
                        // P-19b: 이동을 누르면 곡괭이 레벨이 바뀐다는 것을 미리 말해 준다 — 안 그러면
                        // 물려주기(P-06)로 레벨이 40%로 떨어지는 걸 버그로 오해한다(docs/backlog.md 참고).
                        var toolInfo = target.HasVisitedPlanet(p.Id)
                            ? $"곡괭이 Lv.{target.StoredToolLevel(p.Id)} 보관 중"
                            : $"곡괭이 Lv.{PlanetToolLevel.CarryOverStartLevel(target.rig.ToolLevel)}로 시작(지금의 40%)";
                        _infoLabels[i].text = $"{mineralName} · {toolInfo}";
                    }
                }
                if (_travelButtons[i] != null)
                {
                    var label = _travelButtons[i].GetComponentInChildren<TMP_Text>();
                    if (isCurrent)
                    {
                        _travelButtons[i].interactable = false;
                        if (label != null) label.text = "현재 위치";
                    }
                    else
                    {
                        _travelButtons[i].interactable = true;
                        if (label != null) label.text = "이동";
                    }
                }
            }
        }

        void OnTravelButtonClicked(string planetId)
        {
            if (target == null) return;
            if (target.TravelTo(planetId)) Refresh();
        }
    }
}
