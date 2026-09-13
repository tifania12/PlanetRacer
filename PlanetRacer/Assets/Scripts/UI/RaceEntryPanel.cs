using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using GemRacer.Core;
using GemRacer.Mining;

namespace GemRacer.UI
{
    /// <summary>D09-N: 레이스 출전 화면. 쿼츠 로컬 레이스 3개(DefaultData.QuartzCourses() 선언
    /// 순서 그대로 RaceEntry.uxml의 course1/2/3에 매핑)를 보여주고, 연료가 있으면 "출전" 버튼으로
    /// MiningController.TryEnterRace를 부른다. 결과는 화면 안에서 목록 뷰 → 연출 뷰 → 결과 뷰
    /// 순서로 전환해서 보여준다(CraftingPanel과 같은 역할 분담: 판단은 코어/MiningController,
    /// 여기는 화면 갱신과 클릭 전달만).
    /// D10-N: 출전 버튼을 누르면 바로 결과가 뜨는 대신, 코어 RaceAnimation.BuildSchedule이 만든
    /// 도착 시각표대로 20~30초 동안 anim-view의 막대가 채워지다가(순위대로 도착 — 실제 판정은
    /// 이미 끝난 상태라 결과가 바뀔 일은 없다) 끝나면 결과 뷰로 넘어간다. "건너뛰기"를 누르면
    /// 남은 연출을 안 보고 바로 결과로 넘어간다.</summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class RaceEntryPanel : MonoBehaviour
    {
        [Tooltip("출전 대상. 비워두면 씬에서 하나 찾는다.")]
        public MiningController target;

        VisualElement _root, _entryView, _animView, _resultView;
        Label _fuelLabel, _rewardLabel;
        Button[] _courseButtons;
        Label[] _resultRows;
        Label[] _animNameLabels;
        VisualElement[] _animFills;
        Button _closeButton, _skipButton;
        List<Course> _courses;

        // D10-N 연출 상태. Course/Result/Won은 연출이 끝난 뒤 그대로 ShowResultView에 넘긴다 —
        // 실제 판정은 TryEnterRace 시점에 이미 끝나 있고, 여기서는 보여주는 순서만 늦춘다.
        bool _isAnimating;
        float _animStartTime, _animDuration;
        List<RaceAnimation.Arrival> _animSchedule;
        Course _pendingCourse;
        List<RaceSimulator.Result> _pendingResults;
        bool _pendingWon;

        void OnEnable()
        {
            if (target == null) target = FindFirstObjectByType<MiningController>();
            _root = GetComponent<UIDocument>().rootVisualElement;

            _entryView = _root.Q<VisualElement>("entry-view");
            _animView = _root.Q<VisualElement>("anim-view");
            _resultView = _root.Q<VisualElement>("result-view");
            _fuelLabel = _root.Q<Label>("fuel-label");

            _courses = DefaultData.QuartzCourses(); // 지금은 쿼츠뿐. 다른 행성이 생기면 planetId로 분기(TODO)
            _courseButtons = new[]
            {
                _root.Q<Button>("course1-button"), _root.Q<Button>("course2-button"), _root.Q<Button>("course3-button"),
            };
            for (int i = 0; i < _courseButtons.Length; i++)
            {
                var index = i; // 람다가 반복 변수를 그대로 캡처하지 않게 지역 변수로 고정
                _courseButtons[index].clicked += () => OnCourseButtonClicked(index);
            }

            _resultRows = new Label[6];
            for (int i = 0; i < _resultRows.Length; i++) _resultRows[i] = _root.Q<Label>($"result-row-{i}");
            _rewardLabel = _root.Q<Label>("reward-label");

            _animNameLabels = new Label[6];
            _animFills = new VisualElement[6];
            for (int i = 0; i < _animNameLabels.Length; i++)
            {
                _animNameLabels[i] = _root.Q<Label>($"anim-name-{i}");
                _animFills[i] = _root.Q<VisualElement>($"anim-fill-{i}");
            }
            _skipButton = _root.Q<Button>("skip-button");
            _skipButton.clicked += SkipAnimation;

            _closeButton = _root.Q<Button>("close-button");
            _closeButton.clicked += ShowEntryView;

            ShowEntryView();
        }

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
            _fuelLabel.text = target.Fuel >= RaceFuel.MaxFuel
                ? $"연료 {target.Fuel}/{RaceFuel.MaxFuel} (가득 참)"
                : $"연료 {target.Fuel}/{RaceFuel.MaxFuel} (다음 회복까지 {nextInSeconds / 60}:{nextInSeconds % 60:D2})";

            var canEnter = target.Fuel >= RaceFuel.EntryCost;
            foreach (var button in _courseButtons) button.SetEnabled(canEnter);
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
                    _animNameLabels[i].text = OpponentName(results[i].Id);
                    _animNameLabels[i].style.display = DisplayStyle.Flex;
                    _animFills[i].style.width = Length.Percent(0f);
                }
                else
                {
                    _animNameLabels[i].style.display = DisplayStyle.None;
                }
            }

            _entryView.style.display = DisplayStyle.None;
            _resultView.style.display = DisplayStyle.None;
            _animView.style.display = DisplayStyle.Flex;
        }

        void UpdateAnimation()
        {
            var elapsed = Time.time - _animStartTime;
            for (int i = 0; i < _animFills.Length; i++)
            {
                if (i >= _animSchedule.Count) continue;
                var arrival = _animSchedule[i].ArrivalSeconds;
                var progress = arrival <= 0f ? 1f : Mathf.Clamp01(elapsed / arrival);
                _animFills[i].style.width = Length.Percent(progress * 100f);
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
                    _resultRows[i].text = $"{r.Rank}위 {OpponentName(r.Id)} — {r.Time:F1}초";
                    _resultRows[i].style.display = DisplayStyle.Flex;
                }
                else
                {
                    _resultRows[i].style.display = DisplayStyle.None;
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
                _rewardLabel.text = $"1위! {partText}{boxText}";
            }
            else
            {
                _rewardLabel.text = "이번엔 1위를 놓쳤다. 부품을 더 갖추고 다시 도전해 보자.";
            }

            _entryView.style.display = DisplayStyle.None;
            _resultView.style.display = DisplayStyle.Flex;
        }

        void ShowEntryView()
        {
            _isAnimating = false; // 닫기를 누른 시점엔 연출이 끝나 있을 뿐이지만 방어적으로 같이 끈다.
            _animView.style.display = DisplayStyle.None;
            _resultView.style.display = DisplayStyle.None;
            _entryView.style.display = DisplayStyle.Flex;
        }
    }
}
