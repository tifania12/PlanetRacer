using UnityEngine;
using GemRacer.Core;

#if UNITY_ANDROID && !UNITY_EDITOR
using Unity.Notifications.Android;
#endif
#if UNITY_IOS && !UNITY_EDITOR
using Unity.Notifications.iOS;
#endif

namespace GemRacer.Mining
{
    /// <summary>M-05 / D18-N 화물칸 알림 축. 앱이 백그라운드로 갈 때 "화물칸이 80% 찼다"는
    /// 로컬 알림을 예약하고, 돌아오면 지운다. 재접속을 만드는 장치다.
    ///
    /// "언제 알릴지"는 여기서 계산하지 않는다 — `MiningSimulator.HoursUntilCargoThreshold`가
    /// 코어에서 순수 계산으로 낸다(서버가 같은 값을 내야 하는 값이라). 이 컴포넌트는 그 값을
    /// 받아 플랫폼 알림 API에 넘기는 글루 레이어일 뿐이다. RewardAdTracker·DailyLoginReward와
    /// 같은 역할 분리다.
    ///
    /// 알림 API는 Android·iOS에만 있다. 에디터·WebGL·스탠드얼론에서는 계산까지만 하고
    /// 콘솔에 한 줄 남긴다 — 그래야 Unity 배선 세션이 Play 모드에서 값이 맞는지 볼 수 있다.</summary>
    [RequireComponent(typeof(MiningController))]
    public class CargoNotificationScheduler : MonoBehaviour
    {
        /// <summary>몇 % 찼을 때 알릴지. backlog M-05가 정한 값은 80%다.</summary>
        [Range(0.1f, 1f)]
        public float thresholdFraction = 0.8f;

        /// <summary>이보다 빨리 닿으면 예약하지 않는다. 홈 버튼을 눌렀다 바로 돌아오는 사람에게
        /// 1분 뒤 알림이 가면 그냥 성가시다.</summary>
        public float minimumMinutes = 10f;

        /// <summary>예약·취소할 때마다 콘솔에 한 줄 남긴다. 백그라운드 전환 때만 찍히니
        /// 시끄럽지 않고, 아침에 로그로 확인할 수 있다.</summary>
        public bool logScheduling = true;

        const string AndroidChannelId = "gemracer_cargo";
        const string NotificationTitle = "화물칸이 거의 찼어요";
        const string NotificationBody = "채굴차 화물칸이 80% 찼습니다. 비우지 않으면 곧 채굴이 멈춰요.";

        MiningController _mining;
        bool _channelReady;

        void Awake()
        {
            _mining = GetComponent<MiningController>();
            EnsureAndroidChannel();
        }

        // 모바일에서 홈 버튼을 누르는 순간이 "이제 한동안 안 본다"에 가장 가깝다.
        // OnApplicationQuit은 모바일에서 호출이 보장되지 않아서 여기가 본 진입점이다
        // (MiningController.Save()가 같은 이유로 같은 훅을 쓴다).
        void OnApplicationPause(bool paused)
        {
            if (paused) ScheduleCargoNotification();
            else CancelCargoNotifications();
        }

        void OnApplicationQuit() => ScheduleCargoNotification();

        /// <summary>지금 상태로 80% 도달 시각을 계산해 알림을 예약한다. 이미 넘었거나
        /// (제련소가 유입을 따라잡아) 영원히 안 닿으면 예약하지 않는다.</summary>
        public void ScheduleCargoNotification()
        {
            if (_mining == null) return;

            var planet = _mining.CurrentPlanet;
            if (planet == null) return;

            float? hours = MiningSimulator.HoursUntilCargoThreshold(
                _mining.rig, planet, _mining.RawMinerals, thresholdFraction);

            // null = 정제소가 유입을 따라잡아 원석이 그 이상 안 쌓인다(코어 주석대로 예약 안 함).
            if (hours == null)
            {
                Log("예약 안 함 — 원석이 목표치에 도달하지 않는다(정제가 유입을 따라잡음).");
                return;
            }

            // 0 = 이미 80%를 넘었다. 지금 화면에서 보고 있는 사람에게 다시 알릴 이유가 없다.
            if (hours.Value <= 0f)
            {
                Log("예약 안 함 — 화물칸이 이미 목표치를 넘었다.");
                return;
            }

            float minutes = hours.Value * 60f;
            if (minutes < minimumMinutes)
            {
                Log($"예약 안 함 — {minutes:F1}분 뒤라 최소 간격({minimumMinutes}분)보다 가깝다.");
                return;
            }

            CancelCargoNotifications();
            Schedule(minutes);
            Log($"예약함 — {hours.Value:F2}시간({minutes:F0}분) 뒤.");
        }

        /// <summary>돌아왔을 때 예약을 지운다. 안 지우면 이미 비운 화물칸을 두고 알림이 온다.</summary>
        public void CancelCargoNotifications()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidNotificationCenter.CancelAllScheduledNotifications();
#elif UNITY_IOS && !UNITY_EDITOR
            iOSNotificationCenter.RemoveAllScheduledNotifications();
#endif
        }

        void Schedule(float minutes)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            EnsureAndroidChannel();
            var android = new AndroidNotification
            {
                Title = NotificationTitle,
                Text = NotificationBody,
                FireTime = System.DateTime.Now.AddMinutes(minutes),
                SmallIcon = string.Empty,
                LargeIcon = string.Empty,
            };
            AndroidNotificationCenter.SendNotification(android, AndroidChannelId);
#elif UNITY_IOS && !UNITY_EDITOR
            var ios = new iOSNotification
            {
                Title = NotificationTitle,
                Body = NotificationBody,
                ShowInForeground = false,
                Trigger = new iOSNotificationTimeIntervalTrigger
                {
                    TimeInterval = System.TimeSpan.FromMinutes(minutes),
                    Repeats = false,
                },
            };
            iOSNotificationCenter.ScheduleNotification(ios);
#endif
        }

        void EnsureAndroidChannel()
        {
            if (_channelReady) return;
            _channelReady = true;
#if UNITY_ANDROID && !UNITY_EDITOR
            // Android 8부터 채널이 없으면 알림이 조용히 사라진다.
            var channel = new AndroidNotificationChannel
            {
                Id = AndroidChannelId,
                Name = "화물칸 알림",
                Importance = Importance.Default,
                Description = "화물칸이 거의 찼을 때 알려줍니다.",
            };
            AndroidNotificationCenter.RegisterNotificationChannel(channel);
#endif
        }

        void Log(string message)
        {
            if (logScheduling) Debug.Log($"[화물칸 알림] {message}");
        }
    }
}
