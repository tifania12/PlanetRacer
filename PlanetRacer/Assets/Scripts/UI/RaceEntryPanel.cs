using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using GemRacer.Core;
using GemRacer.Mining;

namespace GemRacer.UI
{
    /// <summary>D09-N: 레이스 출전 화면. 쿼츠 로컬 레이스 3개(DefaultData.QuartzCourses() 선언
    /// 순서 그대로 RaceEntry.uxml의 course1/2/3에 매핑)를 보여주고, 연료가 있으면 "출전" 버튼으로
    /// MiningController.TryEnterRace를 부른다. 결과는 화면 안에서 목록 뷰 ↔ 결과 뷰를 전환해서
    /// 보여준다 — 실제 6대 주행 연출은 D10-N 몫이고, 지금은 결과가 바로 뜬다(CraftingPanel과
    /// 같은 역할 분담: 판단은 코어/MiningController, 여기는 화면 갱신과 클릭 전달만).</summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class RaceEntryPanel : MonoBehaviour
    {
        [Tooltip("출전 대상. 비워두면 씬에서 하나 찾는다.")]
        public MiningController target;

        VisualElement _root, _entryView, _resultView;
        Label _fuelLabel, _rewardLabel;
        Button[] _courseButtons;
        Label[] _resultRows;
        Button _closeButton;
        List<Course> _courses;

        void OnEnable()
        {
            if (target == null) target = FindFirstObjectByType<MiningController>();
            _root = GetComponent<UIDocument>().rootVisualElement;

            _entryView = _root.Q<VisualElement>("entry-view");
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

            _closeButton = _root.Q<Button>("close-button");
            _closeButton.clicked += ShowEntryView;

            ShowEntryView();
        }

        // 연료는 매 프레임 실시간으로 차니(MiningController.RecoverFuel) 계속 다시 그린다.
        void Update() => Refresh();

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

            ShowResultView(course, results, won);
        }

        void ShowResultView(Course course, List<RaceSimulator.Result> results, bool won)
        {
            for (int i = 0; i < _resultRows.Length; i++)
            {
                if (i < results.Count)
                {
                    var r = results[i];
                    // RaceSimulator.MakeOpponents가 "ai_0", "ai_1"... 순서로 id를 붙인다 —
                    // 화면엔 1부터 보이게 +1(사람 눈엔 0번 상대보다 1번 상대가 자연스럽다).
                    var name = r.Id == "player" ? "나" : $"상대 {int.Parse(r.Id.Substring(r.Id.LastIndexOf('_') + 1)) + 1}";
                    _resultRows[i].text = $"{r.Rank}위 {name} — {r.Time:F1}초";
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
                _rewardLabel.text = reward != null
                    ? $"1위! 우승 보상: {reward.NameKo} (레벨 +{reward.LevelBonus})"
                    : "1위! (보상 정의 없음 — 확인 필요)";
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
            _resultView.style.display = DisplayStyle.None;
            _entryView.style.display = DisplayStyle.Flex;
        }
    }
}
