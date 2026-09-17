using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.Core;
using GemRacer.Mining;

namespace GemRacer.UI
{
    /// <summary>
    /// U-11(2026-09-17): CargoFullPanel(UI Toolkit)을 일반 UI(uGUI)로 옮긴 것. 로직은 그대로다 —
    /// MiningController.CargoJustFilled(엣지 트리거)가 참일 때만 뜨고, 닫으면
    /// AcknowledgeCargoFull()로 꺼진다.
    ///
    /// 이 화면만 U-01~U-10에서 빠져 있었다. 옛 UI Toolkit 루트 열 개는 "다 옮기면 지운다"고
    /// 꺼 둔 상태였는데, 화물칸 화면은 옮긴 적이 없어서 **꺼진 채로 잊혔다** — 그 바람에
    /// M-04(정제로 돌리시겠어요?) · M-08(스타터 팩 제안) · M-09 후속(광고 보고 1시간 상한 2배)이
    /// 지금 빌드에서 전부 안 떴다. 이 스크립트가 그 셋을 되살린다.
    ///
    /// 구조는 OfflineRewardUgui와 같다(HUD 버튼으로 여닫는 게 아니라 스스로 뜨는 화면):
    /// 루트는 항상 켜 두고(그래야 Update가 돈다) 자식 backdrop만 SetActive로 여닫는다.
    /// </summary>
    public sealed class CargoFullUgui : MonoBehaviour
    {
        [Tooltip("읽을 대상. 비워두면 씬에서 하나 찾는다.")]
        public MiningController target;

        [Tooltip("'레이스 나가기'로 열 레이스 출전 패널. 비워두면 버튼이 꺼진 채로 남는다.")]
        public UiPanel racePanel;

        [Tooltip("'상점 보기'/'스타터 팩 보기'로 열 상점 패널. 비워두면 두 버튼이 꺼진 채로 남는다.")]
        public UiPanel shopPanel;

        GameObject _backdrop, _starterPackCallout, _adLabelGo, _adButtonGo;
        TMP_Text _message, _starterPackMessage, _adLabel;
        Button _raceButton, _closeButton, _shopButton, _starterPackButton, _starterPackDeclineButton, _adButton;

        void Awake()
        {
            if (target == null) target = FindFirstObjectByType<MiningController>();

            _backdrop = UiKit.FindObject(transform, "cargo-full-backdrop");

            _message = UiKit.Find<TMP_Text>(transform, "cargo-full-message");

            _raceButton  = UiKit.Find<Button>(transform, "race-button");
            _shopButton  = UiKit.Find<Button>(transform, "shop-button");
            _closeButton = UiKit.Find<Button>(transform, "close-button");
            _raceButton?.onClick.AddListener(OpenRace);
            _shopButton?.onClick.AddListener(OpenShop);
            _closeButton?.onClick.AddListener(Close);

            // M-09 후속: "광고 보고 1시간 상한 2배". 오늘 한도가 남아 있을 때만 보인다.
            _adLabel  = UiKit.Find<TMP_Text>(transform, "ad-label");
            _adButton = UiKit.Find<Button>(transform, "ad-button");
            _adButton?.onClick.AddListener(WatchAdForCargoCapDouble);
            _adLabelGo  = _adLabel  != null ? _adLabel.gameObject  : null;
            _adButtonGo = _adButton != null ? _adButton.gameObject : null;

            // M-08: 스타터 팩 칸. 가격·이름은 DefaultData.ShopItems()(=CSV)에서 읽는다 —
            // 옛 UXML과 같은 이유로 부트스트랩은 자리 표시자만 두고 문구는 여기서 채운다.
            _starterPackCallout      = UiKit.FindObject(transform, "starter-pack-callout");
            _starterPackMessage      = UiKit.Find<TMP_Text>(transform, "starter-pack-message");
            _starterPackButton       = UiKit.Find<Button>(transform, "starter-pack-button");
            _starterPackDeclineButton = UiKit.Find<Button>(transform, "starter-pack-decline-button");
            _starterPackButton?.onClick.AddListener(OpenShop);
            _starterPackDeclineButton?.onClick.AddListener(DeclineStarterPack);

            if (_starterPackMessage != null)
            {
                var starterPack = DefaultData.ShopItems()[0]; // ShopSkuId.StarterPack, ShopCatalog.cs 선언 순서상 0번
                _starterPackMessage.text =
                    $"처음 화물칸이 찼어요 — {starterPack.NameKo}으로 화물칸 확장 1단계 + 채굴차 스킨 1종 + " +
                    $"정제 광물 반나절치를 {starterPack.PriceKrw:N0}원에 한 번만 살 수 있어요.";
            }

            if (_backdrop != null) _backdrop.SetActive(false); // 상한에 닿기 전까지 숨겨 둔다
        }

        // OfflineRewardUgui와 같은 이유로 매 프레임 다시 본다 — CargoJustFilled가 꺼지는 순간
        // (닫기 다음 프레임) 화면이 스스로 사라지게 하는 제일 단순한 방법이다.
        void Update() => Refresh();

        void Refresh()
        {
            if (_backdrop == null || target == null) return;

            if (!target.CargoJustFilled)
            {
                if (_backdrop.activeSelf) _backdrop.SetActive(false);
                return;
            }
            if (!_backdrop.activeSelf) _backdrop.SetActive(true);

            if (_message != null)
                _message.text = target.rig.RefineryLevel > 0
                    ? "화물칸이 가득 찼어요. 제련소가 돌고 있어서 정제 광물로는 계속 쌓이고, 잠시 후 원석 자리도 다시 나요."
                    : "화물칸이 가득 찼어요. 제련소를 얻으면 원석이 자동으로 정제 광물로 바뀌어서 화물칸이 다시는 안 차요. " +
                      "레이스에서 우승하면 상자로 제련소를 얻을 수 있어요.";

            if (_raceButton != null) _raceButton.interactable = racePanel != null;
            if (_shopButton != null) _shopButton.interactable = shopPanel != null;

            RefreshAdRow();

            // M-08: "첫 상한 도달 직후 한 번만" — 사거나 거절하기 전까지는 상한에 또 닿을 때마다
            // 이 칸이 같이 뜨고, 나머지(레이스 나가기·상점 보기)는 매번 그대로 뜬다.
            var showStarterPack = target.ShouldShowStarterPackOffer;
            if (_starterPackCallout != null && _starterPackCallout.activeSelf != showStarterPack)
                _starterPackCallout.SetActive(showStarterPack);
            if (showStarterPack && _starterPackButton != null)
                _starterPackButton.interactable = shopPanel != null;
        }

        // M-09 후속: 지금 이미 켜진 중이면(RemainingSeconds > 0) 버튼 대신 남은 시간을 보여준다 —
        // 또 눌러도 틀린 동작은 아니지만, "지금 켜져 있다"를 알려주는 게 더 친절하다.
        void RefreshAdRow()
        {
            var remainingSeconds = target.CargoCapDoubleHourRemainingSeconds;
            if (remainingSeconds > 0)
            {
                if (_adButtonGo != null && _adButtonGo.activeSelf) _adButtonGo.SetActive(false);
                if (_adLabelGo != null && !_adLabelGo.activeSelf) _adLabelGo.SetActive(true);
                if (_adLabel != null)
                    _adLabel.text = $"상한 2배 적용 중 — {remainingSeconds / 60}:{remainingSeconds % 60:D2} 남음";
                return;
            }

            var remaining = target.RemainingRewardAdsToday(RewardAdSlot.CargoCapDoubleHour);
            var canWatch = remaining > 0;
            if (_adButtonGo != null && _adButtonGo.activeSelf != canWatch) _adButtonGo.SetActive(canWatch);
            if (_adLabelGo != null && _adLabelGo.activeSelf != canWatch) _adLabelGo.SetActive(canWatch);
            if (canWatch && _adLabel != null)
                _adLabel.text = $"광고 한 편 보면 1시간 동안 상한이 2배(오늘 {remaining}회 남음)";
        }

        void OpenRace()
        {
            racePanel?.Show();
            Close();
        }

        // M-07: 상점을 열 때는 이 화면을 닫지 않는다 — 구매 후에도 "정제로 돌리시겠어요?" 맥락으로
        // 돌아올 수 있어야 자연스럽다(닫아 버리면 상점만 보다가 무료 해법 안내를 놓친다).
        void OpenShop() => shopPanel?.Show();

        void Close() => target?.AcknowledgeCargoFull();

        void WatchAdForCargoCapDouble() => target?.WatchAdForCargoCapDouble();

        // M-08: "괜찮아요"는 화면 전체를 닫지 않는다 — 아래 레이스 나가기·상점 보기는 그대로
        // 유효하니 스타터 팩 칸만 접는다(다음 프레임 Refresh가 스스로 감춘다).
        void DeclineStarterPack() => target?.DeclineStarterPackOffer();
    }
}
