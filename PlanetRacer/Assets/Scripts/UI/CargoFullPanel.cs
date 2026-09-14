using UnityEngine;
using UnityEngine.UIElements;
using GemRacer.Core;
using GemRacer.Mining;

namespace GemRacer.UI
{
    /// <summary>M-04: 화물칸이 처음(또는 다시) 상한에 닿았을 때 뜨는 화면. 설계 원칙
    /// (docs/design/monetization.md)대로 "멈췄습니다"가 아니라 "정제로 돌리시겠어요?"로 시작하고,
    /// 무료 해법을 상점보다 먼저 보여준다 — 이 게임에서 그 무료 해법은 제련소(RigSlot.Refinery)인데,
    /// 돈으로 사는 게 아니라 레이스에서 이겨 상자로 얻는다(UpgradeSlot에 Refinery가 없는 게 그래서다,
    /// RigUpgrade.cs 참고). 그래서 이 화면의 1순위 버튼은 상점이 아니라 "레이스 나가기"다. 상점
    /// 화면(M-07) 자체가 아직 없어서 그 버튼은 이번 버전엔 없다 — M-07이 붙으면 여기 추가한다.
    ///
    /// MiningController.CargoJustFilled(엣지 트리거)가 참일 때만 뜨고, 버튼을 누르면
    /// AcknowledgeCargoFull()로 꺼진다. OfflineRewardPanel.cs와 같은 패턴(항상 켜진 오브젝트,
    /// 조건에 따라 스스로 접혔다 펴진다).</summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class CargoFullPanel : MonoBehaviour
    {
        [Tooltip("읽을 대상. 비워두면 씬에서 하나 찾는다.")]
        public MiningController target;

        [Tooltip("'레이스 나가기' 버튼으로 열 레이스 출전 패널의 UIDocument. 비워두면 버튼이 비활성 상태로 남는다.")]
        public UIDocument raceDocument;

        [Tooltip("M-07: '상점 보기' 버튼으로 열 상점 패널의 UIDocument. 비워두면 버튼이 비활성 상태로 남는다.")]
        public UIDocument shopDocument;

        VisualElement _root, _starterPackCallout;
        Label _message, _starterPackMessage, _adLabel;
        Button _raceButton, _closeButton, _shopButton, _starterPackButton, _starterPackDeclineButton, _adButton;

        void OnEnable()
        {
            if (target == null) target = FindFirstObjectByType<MiningController>();
            _root = GetComponent<UIDocument>().rootVisualElement;

            _message = _root.Q<Label>("cargo-full-message");
            _raceButton = _root.Q<Button>("race-button");
            _closeButton = _root.Q<Button>("close-button");
            _shopButton = _root.Q<Button>("shop-button");
            _raceButton.clicked += OpenRace;
            _closeButton.clicked += Close;
            _shopButton.clicked += OpenShop;

            // M-09 후속: "광고 보고 1시간 상한 2배". 오늘 한도가 남아 있을 때만 보인다.
            _adLabel = _root.Q<Label>("ad-label");
            _adButton = _root.Q<Button>("ad-button");
            _adButton.clicked += WatchAdForCargoCapDouble;

            // M-08: 스타터 팩 칸. 가격·이름은 DefaultData.ShopItems()(=CSV)에서 읽는다 —
            // ShopPanel.cs와 같은 이유로 UXML엔 자리 표시자("—")만 둔다.
            _starterPackCallout = _root.Q<VisualElement>("starter-pack-callout");
            _starterPackMessage = _root.Q<Label>("starter-pack-message");
            _starterPackButton = _root.Q<Button>("starter-pack-button");
            _starterPackDeclineButton = _root.Q<Button>("starter-pack-decline-button");
            var starterPack = DefaultData.ShopItems()[0]; // ShopSkuId.StarterPack, ShopCatalog.cs 선언 순서상 0번
            _starterPackMessage.text = $"처음 화물칸이 찼어요 — {starterPack.NameKo}으로 화물칸 확장 1단계 + 채굴차 스킨 1종 + " +
                                        $"정제 광물 반나절치를 {starterPack.PriceKrw:N0}원에 한 번만 살 수 있어요.";
            _starterPackButton.clicked += OpenShop;
            _starterPackDeclineButton.clicked += DeclineStarterPack;

            Refresh();
        }

        // OfflineRewardPanel.cs와 같은 이유로 Update에서 매 프레임 다시 본다 — CargoJustFilled가
        // 꺼지는 순간(버튼 클릭 다음 프레임) 화면이 스스로 사라지게 하는 제일 단순한 방법이다.
        void Update() => Refresh();

        void Refresh()
        {
            if (_root == null || target == null) return;
            if (!target.CargoJustFilled)
            {
                _root.style.display = DisplayStyle.None;
                return;
            }
            _root.style.display = DisplayStyle.Flex;

            _message.text = target.rig.RefineryLevel > 0
                ? "화물칸이 가득 찼어요. 제련소가 돌고 있어서 정제 광물로는 계속 쌓이고, 잠시 후 원석 자리도 다시 나요."
                : "화물칸이 가득 찼어요. 제련소를 얻으면 원석이 자동으로 정제 광물로 바뀌어서 화물칸이 다시는 안 차요. " +
                  "레이스에서 우승하면 상자로 제련소를 얻을 수 있어요.";

            _raceButton.SetEnabled(raceDocument != null);
            _shopButton.SetEnabled(shopDocument != null);

            // M-09 후속: 지금 이미 켜진 중이면(RemainingSeconds > 0) 버튼 대신 남은 시간을
            // 보여준다 — 또 눌러도 CanWatchRewardAd가 하루 한도만 보고 시간은 원래 만료 시각부터
            // 이어 붙이니(WatchAdForCargoCapDouble) 틀린 동작은 아니지만, 화면에서 "지금 켜져
            // 있다"는 걸 알려주는 게 더 친절하다.
            var remainingSeconds = target.CargoCapDoubleHourRemainingSeconds;
            if (remainingSeconds > 0)
            {
                _adButton.style.display = DisplayStyle.None;
                _adLabel.style.display = DisplayStyle.Flex;
                _adLabel.text = $"상한 2배 적용 중 — {remainingSeconds / 60}:{remainingSeconds % 60:D2} 남음";
            }
            else
            {
                var remaining = target.RemainingRewardAdsToday(RewardAdSlot.CargoCapDoubleHour);
                var canWatch = remaining > 0;
                _adButton.style.display = canWatch ? DisplayStyle.Flex : DisplayStyle.None;
                _adLabel.style.display = canWatch ? DisplayStyle.Flex : DisplayStyle.None;
                if (canWatch) _adLabel.text = $"광고 한 편 보면 1시간 동안 상한이 2배(오늘 {remaining}회 남음)";
            }

            // M-08: "첫 상한 도달 직후 한 번만" — 이후 상한에 또 닿아도(사거나 거절하기 전까지는
            // 계속) 이 칸만 반복해서 보여주고, 나머지(레이스 나가기·상점 보기)는 매번 그대로 뜬다.
            var showStarterPack = target.ShouldShowStarterPackOffer;
            _starterPackCallout.style.display = showStarterPack ? DisplayStyle.Flex : DisplayStyle.None;
            if (showStarterPack) _starterPackButton.SetEnabled(shopDocument != null);
        }

        void OpenRace()
        {
            if (raceDocument != null) raceDocument.rootVisualElement.style.display = DisplayStyle.Flex;
            Close();
        }

        // M-07: 상점을 열 때는 이 화면을 닫지 않는다 — 레이스 나가기와 달리 상점은 구매 후에도
        // "정제로 돌리시겠어요?" 맥락으로 다시 돌아올 수 있어야 자연스럽다(닫아 버리면 상점만
        // 보다가 무료 해법 안내를 놓친다).
        void OpenShop()
        {
            if (shopDocument != null) shopDocument.rootVisualElement.style.display = DisplayStyle.Flex;
        }

        void Close() => target?.AcknowledgeCargoFull();

        void WatchAdForCargoCapDouble() => target?.WatchAdForCargoCapDouble();

        // M-08: "괜찮아요"는 이 화면 전체를 닫지 않는다 — 아래 레이스 나가기·상점 보기는 그대로
        // 유효하니, 스타터 팩 칸만 접는다(Refresh가 다음 프레임에 ShouldShowStarterPackOffer==false로
        // 스스로 감춘다).
        void DeclineStarterPack() => target?.DeclineStarterPackOffer();
    }
}
