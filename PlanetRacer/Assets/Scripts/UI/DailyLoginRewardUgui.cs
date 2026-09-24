using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.Mining;

namespace GemRacer.UI
{
    /// <summary>
    /// D18-N 남은 절반(2026-09-25): 하루 첫 접속 보상 + 구독 매일 지급을 보여주는 확인 팝업.
    /// MiningController.GrantDailyRewards()(2026-09-24 23시 배선)는 이미 자원을 다 줬다 —
    /// 이 화면은 그 결과를 보여주고 "확인"으로 닫히기만 한다. OfflineRewardUgui와 같은 요령으로
    /// 루트는 항상 켜 두고(Update가 계속 돌아야 함) backdrop만 SetActive로 여닫는다.
    /// 화물칸이 꽉 찬 채로 접속했을 때 넘치는 원석을 어떻게 할지는 아직 정해지지 않았다
    /// (docs/backlog.md D18-N 항목) — 이 화면은 그 결정과 무관하게 "실제로 준 값"만 보여준다.
    /// </summary>
    public sealed class DailyLoginRewardUgui : MonoBehaviour
    {
        [Tooltip("보상을 읽어올 대상. 비워두면 씬에서 하나 찾는다.")]
        public MiningController target;

        GameObject _backdrop;
        TMP_Text _streakLabel, _loginLabel, _subscriptionLabel;
        Button _confirmButton;

        void Awake()
        {
            if (target == null) target = FindFirstObjectByType<MiningController>();

            _backdrop = UiKit.FindObject(transform, "daily-reward-backdrop");

            _streakLabel       = UiKit.Find<TMP_Text>(transform, "streak-label");
            _loginLabel        = UiKit.Find<TMP_Text>(transform, "login-reward-label");
            _subscriptionLabel = UiKit.Find<TMP_Text>(transform, "subscription-reward-label");

            _confirmButton = UiKit.Find<Button>(transform, "confirm-button");
            _confirmButton?.onClick.AddListener(Confirm);

            if (_backdrop != null) _backdrop.SetActive(false); // 지급 여부가 확정될 때까지 숨겨 둔다
        }

        void Update() => Refresh();

        void Refresh()
        {
            if (_backdrop == null || target == null) return;
            if (!target.HasPendingDailyRewardNotice)
            {
                if (_backdrop.activeSelf) _backdrop.SetActive(false);
                return;
            }
            if (!_backdrop.activeSelf) _backdrop.SetActive(true);

            var loginMinerals = target.GrantedDailyLoginRawMinerals;
            var subscriptionMinerals = target.GrantedSubscriptionRefinedMinerals;

            if (_streakLabel != null)
            {
                _streakLabel.gameObject.SetActive(loginMinerals > 0f);
                if (loginMinerals > 0f) _streakLabel.text = $"연속 접속 {target.DailyLoginStreakDays}일째";
            }
            if (_loginLabel != null)
            {
                _loginLabel.gameObject.SetActive(loginMinerals > 0f);
                if (loginMinerals > 0f) _loginLabel.text = $"오늘의 접속 보상: 원석 {loginMinerals:F1}";
            }
            if (_subscriptionLabel != null)
            {
                _subscriptionLabel.gameObject.SetActive(subscriptionMinerals > 0f);
                if (subscriptionMinerals > 0f) _subscriptionLabel.text = $"구독 지급: 정제 광물 {subscriptionMinerals:F1}";
            }
        }

        void Confirm()
        {
            target?.AcknowledgeDailyRewards();
            // 다음 프레임 Refresh가 HasPendingDailyRewardNotice == false를 보고 backdrop을 스스로 숨긴다.
        }
    }
}
