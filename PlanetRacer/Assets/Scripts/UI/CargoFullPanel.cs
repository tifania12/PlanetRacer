using UnityEngine;
using UnityEngine.UIElements;
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

        VisualElement _root;
        Label _message;
        Button _raceButton, _closeButton, _shopButton;

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
    }
}
