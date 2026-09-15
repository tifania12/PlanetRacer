using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.Core;
using GemRacer.Mining;

namespace GemRacer.UI
{
    /// <summary>
    /// U-04(2026-09-15): RaceEntryPanel(UI Toolkit)을 일반 UI(uGUI)로 옮긴 것. 로직은 그대로다 —
    /// entry-view(코스 3개)에서 출전하면 anim-view(6대 도착 연출)를 거쳐 result-view로 넘어간다.
    /// 판단(연료 차감, 순위, 보상)은 전부 코어/MiningController가 이미 끝내 두고, 여기는 그 결과를
    /// 순서대로 보여주는 것과 클릭 전달만 한다(CraftingUgui·UpgradeUgui와 같은 역할 분담).
    ///
    /// UI Toolkit 시절엔 세 뷰를 style.display로 켜고 껐는데, uGUI에서는 세 뷰가 각각 자식
    /// GameObject라 SetActive로 바꾼다. 진행 막대(anim-fill)도 style.width 퍼센트 대신
    /// Image.fillAmount(Filled/Horizontal)로 채운다 — MainHudUgui의 화물칸 게이지와 같은 방식.
    /// </summary>
    public sealed class RaceEntryUgui : MonoBehaviour
    {
        [Tooltip("출전 대상. 비워두면 씬에서 하나 찾는다.")]
        public MiningController target;

        GameObject _entryView, _animView, _resultView;
        TMP_Text _fuelLabel, _rewardLabel, _fuelAdLabel, _boxAdLabel;
        Button[] _courseButtons;
        TMP_Text[] _resultRows;
        TMP_Text[] _animNameLabels;
        Image[] _animFills;
        Button _closeButton, _skipButton, _fuelAdButton, _boxAdButton;
        List<Course> _courses;

        // M-09 후속: 방금 이긴 코스의 상자 등급. "광고 보고 상자 1개 더" 버튼이 어떤 등급을
        // 더 줘야 하는지는 이겼을 때만 정해지므로 ShowResultView에서 채우고, 진 판이거나
        // 그 등급이 없으면(RaceBoxReward.ForTier가 null을 주는 등급이면) null로 둔다.
        LootBoxType? _pendingBoxType;

        // D10-N 연출 상태. Course/Result/Won은 연출이 끝난 뒤 그대로 ShowResultView에 넘긴다 —
        // 실제 판정은 TryEnterRace 시점에 이미 끝나 있고, 여기서는 보여주는 순서만 늦춘다.
        bool _isAnimating;
        float _animStartTime, _animDuration;
        List<RaceAnimation.Arrival> _animSchedule;
        Course _pendingCourse;
        List<RaceSimulator.Result> _pendingResults;
        bool _pendingWon;

        void Awake()
        {
            if (target == null) target = FindFirstObjectByType<MiningController>();

            _entryView = UiKit.FindObject(transform, "entry-view");
            _animView = UiKit.FindObject(transform, "anim-view");
            _resultView = UiKit.FindObject(transform, "result-view");
            _fuelLabel = UiKit.Find<TMP_Text>(transform, "fuel-label");

            _courses = DefaultData.QuartzCourses(); // 지금은 쿼츠뿐. 다른 행성이 생기면 planetId로 분기(TODO)
            _courseButtons = new[]
            {
                UiKit.Find<Button>(transform, "course1-button"),
                UiKit.Find<Button>(transform, "course2-button"),
                UiKit.Find<Button>(transform, "course3-button"),
            };
            for (int i = 0; i < _courseButtons.Length; i++)
            {
                var index = i; // 람다가 반복 변수를 그대로 캡처하지 않게 지역 변수로 고정
                _courseButtons[index]?.onClick.AddListener(() => OnCourseButtonClicked(index));
            }

            _resultRows = new TMP_Text[6];
            for (int i = 0; i < _resultRows.Length; i++) _resultRows[i] = UiKit.Find<TMP_Text>(transform, $"result-row-{i}");
            _rewardLabel = UiKit.Find<TMP_Text>(transform, "reward-label");

            _animNameLabels = new TMP_Text[6];
            _animFills = new Image[6];
            for (int i = 0; i < _animNameLabels.Length; i++)
            {
                _animNameLabels[i] = UiKit.Find<TMP_Text>(transform, $"anim-name-{i}");
                _animFills[i] = UiKit.Find<Image>(transform, $"anim-fill-{i}");
                if (_animFills[i] != null)
                {
                    _animFills[i].type = Image.Type.Filled;
                    _animFills[i].fillMethod = Image.FillMethod.Horizontal;
                    _animFills[i].fillOrigin = (int)Image.OriginHorizontal.Left;
                }
            }
            _skipButton = UiKit.Find<Button>(transform, "skip-button");
            _skipButton?.onClick.AddListener(SkipAnimation);

            _closeButton = UiKit.Find<Button>(transform, "close-button");
            _closeButton?.onClick.AddListener(ShowEntryView);

            // M-09 후속: "광고 보고 연료 +3"(entry-view, 연료 부족할 때만) / "광고 보고 상자 1개 더"
            // (result-view, 상자를 받은 판일 때만).
            _fuelAdLabel = UiKit.Find<TMP_Text>(transform, "fuel-ad-label");
            _fuelAdButton = UiKit.Find<Button>(transform, "fuel-ad-button");
            _fuelAdButton?.onClick.AddListener(WatchAdForFuelRefill);
            _boxAdLabel = UiKit.Find<TMP_Text>(transform, "box-ad-label");
            _boxAdButton = UiKit.Find<Button>(transform, "box-ad-button");
            _boxAdButton?.onClick.AddListener(WatchAdForExtraLootBox);
        }

        // 패널이 열릴 때마다(HUD의 "레이스" 버튼이 UiPanel.Toggle로 SetActive(true)) 처음
        // 보이는 화면은 항상 entry-view다 — 지난 판 결과 화면이 남아 있으면 안 된다.
        void OnEnable() => ShowEntryView();

        // 연료는 매 프레임 실시간으로 차니(MiningController.RecoverFuel) 계속 다시 그린다.
        // 연출 중에는 entry-view가 숨어 있어 Refresh 결과가 안 보이지만, 그냥 계속 불러도 무해하다.
        void Update()
        {
            Refresh();
            if (_isAnimating) UpdateAnimation();
        }

        void Refresh()
        {
            if (target == null) return;

            var nextInSeconds = Mathf.CeilToInt(target.SecondsUntilNextFuel);
            if (_fuelLabel != null)
                _fuelLabel.text = target.Fuel >= RaceFuel.MaxFuel
                    ? $"연료 {target.Fuel}/{RaceFuel.MaxFuel} (가득 참)"
                    : $"연료 {target.Fuel}/{RaceFuel.MaxFuel} (다음 회복까지 {nextInSeconds / 60}:{nextInSeconds % 60:D2})";

            var canEnter = target.Fuel >= RaceFuel.EntryCost;
            foreach (var button in _courseButtons)
                if (button != null) button.interactable = canEnter;

            // M-09 후속: 연료가 모자라 출전을 못 할 때만("연료 부족") 이 자리를 보여준다 —
            // 연료가 있으면 굳이 볼 필요 없는 광고다.
            var showFuelAd = !canEnter && target.RemainingRewardAdsToday(RewardAdSlot.FuelRefill) > 0;
            SetActiveIfPresent(_fuelAdButton, showFuelAd);
            SetActiveIfPresent(_fuelAdLabel, showFuelAd);
            if (showFuelAd)
            {
                var remaining = target.RemainingRewardAdsToday(RewardAdSlot.FuelRefill);
                _fuelAdLabel.text = $"연료가 없다면 광고 한 편으로 +3(오늘 {remaining}회 남음)";
            }
        }

        void OnCourseButtonClicked(int index)
        {
            if (target == null || index >= _courses.Count) return;
            var course = _courses[index];
            if (!target.TryEnterRace(course, out var results, out var won)) return; // 연료 부족 — 버튼이 잠깐 안 갱신된 경우 방어

            StartAnimation(course, results, won);
        }

        /// <summary>결과는 이미 정해졌다 — 여기서는 RaceAnimation.BuildSchedule로 만든 도착
        /// 시각표대로 anim-view를 재생할 뿐이다. 연출 길이(20~30초)를 뽑는 난수는 코어가 아니라
        /// 여기(글루 레이어)에서 UnityEngine.Random으로 정한다 — CLAUDE.md 1번대로 코어 함수는
        /// 그 길이를 인자로만 받는다.</summary>
        void StartAnimation(Course course, List<RaceSimulator.Result> results, bool won)
        {
            _pendingCourse = course;
            _pendingResults = results;
            _pendingWon = won;

            _animDuration = UnityEngine.Random.Range(RaceAnimation.MinDurationSeconds, RaceAnimation.MaxDurationSeconds);
            _animSchedule = RaceAnimation.BuildSchedule(results, _animDuration);
            _animStartTime = Time.time;
            _isAnimating = true;

            // results는 RaceSimulator.Run이 이미 순위순으로 정렬해 돌려주고, BuildSchedule도
            // 같은 순서(Rank 오름차순)를 지키므로 i번째 줄 = results[i] = _animSchedule[i]로 그대로 맞는다.
            for (int i = 0; i < _animNameLabels.Length; i++)
            {
                if (i < results.Count)
                {
                    if (_animNameLabels[i] != null) _animNameLabels[i].text = OpponentName(results[i].Id);
                    SetActiveIfPresent(_animNameLabels[i], true);
                    if (_animFills[i] != null) _animFills[i].fillAmount = 0f;
                }
                else
                {
                    SetActiveIfPresent(_animNameLabels[i], false);
                }
            }

            SetActive(_entryView, false);
            SetActive(_resultView, false);
            SetActive(_animView, true);
        }

        void UpdateAnimation()
        {
            var elapsed = Time.time - _animStartTime;
            for (int i = 0; i < _animFills.Length; i++)
            {
                if (i >= _animSchedule.Count || _animFills[i] == null) continue;
                var arrival = _animSchedule[i].ArrivalSeconds;
                var progress = arrival <= 0f ? 1f : Mathf.Clamp01(elapsed / arrival);
                _animFills[i].fillAmount = progress;
            }

            if (elapsed >= _animDuration) FinishAnimation();
        }

        void SkipAnimation()
        {
            if (_isAnimating) FinishAnimation();
        }

        void FinishAnimation()
        {
            _isAnimating = false;
            ShowResultView(_pendingCourse, _pendingResults, _pendingWon);
        }

        // RaceSimulator.MakeOpponents가 "ai_0", "ai_1"... 순서로 id를 붙인다 —
        // 화면엔 1부터 보이게 +1(사람 눈엔 0번 상대보다 1번 상대가 자연스럽다).
        static string OpponentName(string id) =>
            id == "player" ? "나" : $"상대 {int.Parse(id.Substring(id.LastIndexOf('_') + 1)) + 1}";

        void ShowResultView(Course course, List<RaceSimulator.Result> results, bool won)
        {
            for (int i = 0; i < _resultRows.Length; i++)
            {
                if (i < results.Count)
                {
                    var r = results[i];
                    if (_resultRows[i] != null) _resultRows[i].text = $"{r.Rank}위 {OpponentName(r.Id)} — {r.Time:F1}초";
                    SetActiveIfPresent(_resultRows[i], true);
                }
                else
                {
                    SetActiveIfPresent(_resultRows[i], false);
                }
            }

            if (won)
            {
                var reward = DefaultData.QuartzLocalRaceRewards().Find(r => r.CourseId == course.Id);
                var partText = reward != null
                    ? $"우승 보상: {reward.NameKo} (레벨 +{reward.LevelBonus})"
                    : "(보상 정의 없음 — 확인 필요)";

                // D11-N 후속: 코스 등급에 맞는 공구 상자도 같이 받는다(RaceBoxReward, GDD "로컬=녹슨").
                var box = RaceBoxReward.ForTier(course.Tier);
                var boxText = box.HasValue ? $" + {LootBoxOpener.NameKo(box.Value)} 1개" : "";
                if (_rewardLabel != null) _rewardLabel.text = $"1위! {partText}{boxText}";
                _pendingBoxType = box; // M-09 후속: "광고 보고 상자 1개 더" 버튼이 이 등급을 그대로 더 준다.
            }
            else
            {
                if (_rewardLabel != null) _rewardLabel.text = "이번엔 1위를 놓쳤다. 부품을 더 갖추고 다시 도전해 보자.";
                _pendingBoxType = null;
            }

            // M-09 후속: 상자를 받은 판(_pendingBoxType != null)에서 오늘 한도가 남아 있을 때만.
            var showBoxAd = _pendingBoxType.HasValue && target != null
                && target.RemainingRewardAdsToday(RewardAdSlot.ExtraLootBox) > 0;
            SetActiveIfPresent(_boxAdButton, showBoxAd);
            SetActiveIfPresent(_boxAdLabel, showBoxAd);
            if (showBoxAd)
            {
                var remaining = target.RemainingRewardAdsToday(RewardAdSlot.ExtraLootBox);
                _boxAdLabel.text = $"광고 한 편 보면 상자 1개 더(오늘 {remaining}회 남음)";
            }

            // anim-view도 반드시 끈다 — 세 뷰가 같은 자리를 겹쳐 차지하므로
            // 하나라도 남겨 두면 결과 글자 위에 연출 막대가 그대로 겹쳐 보인다
            // (2026-09-15 배선 세션에서 실제로 겹쳐 있었다).
            SetActive(_animView, false);
            SetActive(_entryView, false);
            SetActive(_resultView, true);
        }

        void WatchAdForFuelRefill() => target?.WatchAdForFuelRefill();

        void WatchAdForExtraLootBox()
        {
            if (target == null || !_pendingBoxType.HasValue) return;
            if (target.WatchAdForExtraLootBox(_pendingBoxType.Value))
            {
                // 한 판에 한 번만 — 다시 눌러 또 받는 것을 막는다(하루 한도와는 별개의 판 단위 제한).
                _pendingBoxType = null;
                SetActiveIfPresent(_boxAdButton, false);
                SetActiveIfPresent(_boxAdLabel, false);
            }
        }

        void ShowEntryView()
        {
            _isAnimating = false; // 닫기를 누른 시점엔 연출이 끝나 있을 뿐이지만 방어적으로 같이 끈다.
            SetActive(_animView, false);
            SetActive(_resultView, false);
            SetActive(_entryView, true);
        }

        static void SetActive(GameObject go, bool active)
        {
            if (go != null) go.SetActive(active);
        }

        static void SetActiveIfPresent(Component c, bool active)
        {
            if (c != null) c.gameObject.SetActive(active);
        }
    }
}
