using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.Core;
using GemRacer.Mining;

namespace GemRacer.UI
{
    /// <summary>
    /// U-07(2026-09-16): OfflineRewardPanel(UI Toolkit)을 일반 UI(uGUI)로 옮긴 것. 로직은
    /// 그대로다 — MiningController.PendingOfflineReward를 그대로 읽어 보여주고 "받기"로
    /// 지급한다. 다른 오버레이(U-02~U-06)와 다른 점 하나 — 이 화면은 HUD 버튼으로 여닫는 게
    /// 아니라 보상 유무에 따라 스스로 열리고 닫힌다. 그래서 UiPanel을 안 쓴다: 이 스크립트가
    /// 붙은 루트는 항상 켜진 채로 두고(그래야 Update가 계속 돈다), 실제로 보이고 뒤 클릭을
    /// 막는 배경(backdrop)만 SetActive로 여닫는다 — TutorialUgui의 bubble과 같은 구조.
    /// </summary>
    public sealed class OfflineRewardUgui : MonoBehaviour
    {
        [Tooltip("보상을 읽어올 대상. 비워두면 씬에서 하나 찾는다.")]
        public MiningController target;

        GameObject _backdrop;
        TMP_Text _elapsed, _counted, _wasted, _minerals, _refined, _treasure, _doubleAdLabel;
        Button _claimButton, _doubleClaimButton;

        void Awake()
        {
            if (target == null) target = FindFirstObjectByType<MiningController>();

            _backdrop = UiKit.FindObject(transform, "offline-reward-backdrop");

            _elapsed  = UiKit.Find<TMP_Text>(transform, "elapsed-label");
            _counted  = UiKit.Find<TMP_Text>(transform, "counted-label");
            _wasted   = UiKit.Find<TMP_Text>(transform, "wasted-label");
            _minerals = UiKit.Find<TMP_Text>(transform, "minerals-label");
            _refined  = UiKit.Find<TMP_Text>(transform, "refined-label");
            _treasure = UiKit.Find<TMP_Text>(transform, "treasure-label");

            _claimButton = UiKit.Find<Button>(transform, "claim-button");
            _claimButton?.onClick.AddListener(Claim);

            _doubleAdLabel = UiKit.Find<TMP_Text>(transform, "double-ad-label");
            _doubleClaimButton = UiKit.Find<Button>(transform, "double-claim-button");
            _doubleClaimButton?.onClick.AddListener(ClaimDoubled);

            if (_backdrop != null) _backdrop.SetActive(false); // 보상이 없을 수도 있으니 확정될 때까지 숨겨 둔다
        }

        // 보상은 MiningController.Awake에서 한 번만 계산되므로 Update에서 굳이 매 프레임 다시
        // 계산할 필요는 없지만, "받기"를 누른 다음 프레임에 화면이 사라지는 것 하나는 여기서
        // 매 프레임 보는 게 제일 단순하다 — 라벨 대입 몇 개뿐이라 비용도 싸다.
        void Update() => Refresh();

        void Refresh()
        {
            if (_backdrop == null || target == null) return;
            var reward = target.PendingOfflineReward;
            if (reward == null)
            {
                if (_backdrop.activeSelf) _backdrop.SetActive(false);
                return;
            }
            if (!_backdrop.activeSelf) _backdrop.SetActive(true);

            var r = reward.Value;
            if (_elapsed != null) _elapsed.text = $"자리를 비운 시간: {FormatHours(r.ElapsedHours)}";
            if (_counted != null) _counted.text = $"인정된 시간: {FormatHours(r.CountedHours)}";
            if (_wasted != null)
                _wasted.text = r.WastedHours > 0.01f
                    ? $"화물칸이 넘쳐 버린 시간: {FormatHours(r.WastedHours)}"
                    : "화물칸이 넘치지 않았다";
            // M-02: 원석과 정제 광물을 나눠서 보여준다 — 보물 환산치(TreasureValue)는 정의상
            // "정제 광물 환산치"라 정제 쪽에 합친다(TreasureDef.MineralValue 주석 참고).
            if (_minerals != null) _minerals.text = $"획득 원석: {r.Minerals:F1}";
            if (_refined != null) _refined.text = $"획득 정제 광물: {r.RefinedGained + r.TreasureValue:F1}";
            if (_treasure != null)
                _treasure.text = r.TreasuresFound > 0
                    ? $"발견한 보물 {r.TreasuresFound}개 (그중 지금 캘 수 있는 것 {r.TreasuresMineable}개)"
                    : "발견한 보물 없음";

            // M-09 후속: 오늘 한도가 남아 있을 때만 버튼을 보여준다 — 다 썼으면 칸 자체를 접어서
            // "왜 안 눌리지"보다 "오늘은 끝났다"가 더 분명하게 보이게 한다.
            var remaining = target.RemainingRewardAdsToday(RewardAdSlot.OfflineRewardDouble);
            var canWatch = remaining > 0;
            if (_doubleClaimButton != null) _doubleClaimButton.gameObject.SetActive(canWatch);
            if (_doubleAdLabel != null)
            {
                _doubleAdLabel.gameObject.SetActive(canWatch);
                if (canWatch) _doubleAdLabel.text = $"광고 한 편 보면 이 보상을 2배로(오늘 {remaining}회 남음)";
            }
        }

        void Claim()
        {
            target?.ClaimOfflineReward();
            // 다음 프레임 Refresh가 PendingOfflineReward == null을 보고 backdrop을 스스로 숨긴다.
        }

        void ClaimDoubled()
        {
            target?.ClaimOfflineRewardDoubled();
            // 실패해도(한도를 마침 다 썼거나) 다음 프레임 Refresh가 버튼을 다시 알맞게 그린다.
        }

        static string FormatHours(float hours) => hours < 1f ? $"{hours * 60f:F0}분" : $"{hours:F1}시간";
    }
}
