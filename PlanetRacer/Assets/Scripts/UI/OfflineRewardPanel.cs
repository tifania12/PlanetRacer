using UnityEngine;
using UnityEngine.UIElements;
using GemRacer.Core;
using GemRacer.Mining;

namespace GemRacer.UI
{
    /// <summary>D07-N: 오프라인 보상 화면. MiningController가 Awake에서 이미 계산해 둔
    /// PendingOfflineReward를 그대로 읽어 보여주고, "받기"를 누르면 ClaimOfflineReward()로
    /// 실제 지급한다. 보상이 없으면(첫 실행이거나 자리를 비운 지 얼마 안 됐으면) 화면 자체를
    /// 숨긴다 — Upgrade 오버레이처럼 항상 켜져 있다가 조건에 따라 접혔다 펴진다.
    /// M-09 후속: "광고 보고 2배 받기" 버튼 하나 추가 — 오늘 한도가 남아 있을 때만 보인다.</summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class OfflineRewardPanel : MonoBehaviour
    {
        [Tooltip("보상을 읽어올 대상. 비워두면 씬에서 하나 찾는다.")]
        public MiningController target;

        VisualElement _root;
        Label _elapsed, _counted, _wasted, _minerals, _refined, _treasure, _doubleAdLabel;
        Button _claimButton, _doubleClaimButton;

        void OnEnable()
        {
            if (target == null) target = FindFirstObjectByType<MiningController>();
            _root = GetComponent<UIDocument>().rootVisualElement;

            _elapsed = _root.Q<Label>("elapsed-label");
            _counted = _root.Q<Label>("counted-label");
            _wasted = _root.Q<Label>("wasted-label");
            _minerals = _root.Q<Label>("minerals-label");
            _refined = _root.Q<Label>("refined-label");
            _treasure = _root.Q<Label>("treasure-label");
            _claimButton = _root.Q<Button>("claim-button");
            _claimButton.clicked += Claim;

            _doubleAdLabel = _root.Q<Label>("double-ad-label");
            _doubleClaimButton = _root.Q<Button>("double-claim-button");
            _doubleClaimButton.clicked += ClaimDoubled;

            Refresh();
        }

        // 보상은 MiningController.Awake에서 한 번만 계산되므로 Update에서 굳이 매 프레임 다시
        // 계산할 필요는 없지만, "받기"를 누른 다음 프레임에 화면이 사라지는 것 하나는 여기서
        // 매 프레임 보는 게 제일 단순하다 — 라벨 대입 몇 개뿐이라 비용도 싸다.
        void Update() => Refresh();

        void Refresh()
        {
            if (_root == null || target == null) return;
            var reward = target.PendingOfflineReward;
            if (reward == null)
            {
                _root.style.display = DisplayStyle.None;
                return;
            }
            _root.style.display = DisplayStyle.Flex;

            var r = reward.Value;
            _elapsed.text = $"자리를 비운 시간: {FormatHours(r.ElapsedHours)}";
            _counted.text = $"인정된 시간: {FormatHours(r.CountedHours)}";
            _wasted.text = r.WastedHours > 0.01f
                ? $"화물칸이 넘쳐 버린 시간: {FormatHours(r.WastedHours)}"
                : "화물칸이 넘치지 않았다";
            // M-02: 원석과 정제 광물을 나눠서 보여준다 — 보물 환산치(TreasureValue)는 정의상
            // "정제 광물 환산치"라 정제 쪽에 합친다(TreasureDef.MineralValue 주석 참고).
            _minerals.text = $"획득 원석: {r.Minerals:F1}";
            _refined.text = $"획득 정제 광물: {r.RefinedGained + r.TreasureValue:F1}";
            _treasure.text = r.TreasuresFound > 0
                ? $"발견한 보물 {r.TreasuresFound}개 (그중 지금 캘 수 있는 것 {r.TreasuresMineable}개)"
                : "발견한 보물 없음";

            // M-09 후속: 오늘 한도가 남아 있을 때만 버튼을 보여준다 — 다 썼으면 칸 자체를 접어서
            // "왜 안 눌리지"보다 "오늘은 끝났다"가 더 분명하게 보이게 한다.
            var remaining = target.RemainingRewardAdsToday(RewardAdSlot.OfflineRewardDouble);
            var canWatch = remaining > 0;
            _doubleClaimButton.style.display = canWatch ? DisplayStyle.Flex : DisplayStyle.None;
            _doubleAdLabel.style.display = canWatch ? DisplayStyle.Flex : DisplayStyle.None;
            if (canWatch) _doubleAdLabel.text = $"광고 한 편 보면 이 보상을 2배로(오늘 {remaining}회 남음)";
        }

        void Claim()
        {
            target?.ClaimOfflineReward();
            // 다음 프레임 Refresh가 PendingOfflineReward == null을 보고 화면을 스스로 숨긴다.
        }

        void ClaimDoubled()
        {
            target?.ClaimOfflineRewardDoubled();
            // 실패해도(한도를 마침 다 썼거나) 다음 프레임 Refresh가 버튼을 다시 알맞게 그린다.
        }

        static string FormatHours(float hours) => hours < 1f ? $"{hours * 60f:F0}분" : $"{hours:F1}시간";
    }
}
