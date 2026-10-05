using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.Core;
using GemRacer.Mining;

namespace GemRacer.UI
{
    /// <summary>
    /// E-08(2026-10-06 Unity 배선 세션): 일일 던전 **입구** 화면.
    ///
    /// 보여 주는 것은 네 가지다 — 오늘의 요일 행성, 남은 입장 횟수(0~3), 등급별 강화석
    /// 보상표(연구 "강화석 감정" 배율이 이미 곱해진 실제 값), 그리고 지금 가진 강화석.
    /// 규칙은 전부 코어(DailyDungeon)에 있고 여기는 MiningController 글루를 불러 TMP에 넣을 뿐이다.
    ///
    /// **한 판(60초 채굴 + 레이스 1판)은 아직 없다.** 그래서 입장 버튼은 눌리지 않게 두고
    /// 라벨로 그 이유를 적는다 — 누를 수 있게 해 두면 하루 3회뿐인 입장 횟수만 조용히
    /// 사라진다(TryEnterDungeon은 호출되는 순간 저장까지 한다). 한 판이 생기면 이 화면에서
    /// 바꿀 것은 `_enterButton.onClick`에 그 진입점을 걸고 `EnterRunPlaceholder`를 지우는 것뿐이다.
    ///
    /// ResearchUgui와 같은 요령 — 루트는 항상 켜 두고 자식 "dungeon-backdrop"만 여닫는다.
    /// 여는 길은 `Open()`이고, 레이스 출전 화면의 "일일 던전" 버튼이 그것을 부른다(GemRacer/43).
    /// </summary>
    public sealed class DungeonUgui : MonoBehaviour
    {
        /// <summary>보상표 줄 수. C/B/A/S 네 등급.</summary>
        public const int GradeCount = 4;

        static readonly string[] WeekdayNamesKo = { "월", "화", "수", "목", "금", "토", "일" };

        [Tooltip("던전 상태를 읽을 대상. 비워두면 씬에서 하나 찾는다.")]
        public MiningController target;

        GameObject _backdrop;
        TMP_Text _planetLabel, _entriesLabel, _stonesLabel, _noteLabel;
        Button _enterButton;
        TMP_Text _enterLabel;
        readonly TMP_Text[] _gradeLabels = new TMP_Text[GradeCount];

        void Awake()
        {
            if (target == null) target = FindFirstObjectByType<MiningController>();

            _backdrop = UiKit.FindObject(transform, "dungeon-backdrop");
            _planetLabel = UiKit.Find<TMP_Text>(transform, "dungeon-planet-label");
            _entriesLabel = UiKit.Find<TMP_Text>(transform, "dungeon-entries-label");
            _stonesLabel = UiKit.Find<TMP_Text>(transform, "dungeon-stones-label");
            _noteLabel = UiKit.Find<TMP_Text>(transform, "dungeon-note-label");

            for (var i = 0; i < GradeCount; i++)
                _gradeLabels[i] = UiKit.Find<TMP_Text>(transform, $"dungeon-grade-{i}");

            _enterButton = UiKit.Find<Button>(transform, "dungeon-enter-button");
            _enterLabel = _enterButton != null ? _enterButton.GetComponentInChildren<TMP_Text>() : null;

            UiKit.Find<Button>(transform, "dungeon-close-button")?.onClick.AddListener(Close);

            if (_backdrop != null) _backdrop.SetActive(false);
        }

        public void Open()
        {
            if (_backdrop != null) _backdrop.SetActive(true);
            Refresh();
        }

        public void Close()
        {
            if (_backdrop != null) _backdrop.SetActive(false);
        }

        /// <summary>오늘의 행성 이름. 일요일은 코어가 null을 주니(전부 중 선택) 그대로 적어 준다.</summary>
        string PlanetNameToday()
        {
            var id = target.DungeonPlanetIdToday;
            if (string.IsNullOrEmpty(id)) return "전부 중 선택";
            foreach (var p in DefaultData.Planets())
                if (p.Id == id) return p.NameKo;
            return id; // 행성 목록이 바뀌어 못 찾으면 id라도 보여 준다 — 빈칸보다 낫다
        }

        void Refresh()
        {
            if (target == null) return;

            var weekday = target.DungeonWeekdayToday;
            var weekdayKo = weekday >= 0 && weekday < WeekdayNamesKo.Length ? WeekdayNamesKo[weekday] : "?";
            var left = target.DungeonEntriesLeft;

            if (_planetLabel != null)
                _planetLabel.text = $"오늘({weekdayKo})의 행성 — {PlanetNameToday()}";

            if (_entriesLabel != null)
                _entriesLabel.text = $"남은 입장 {left}/{DailyDungeon.MaxEntriesPerDay}회  ·  자정에 초기화";

            if (_stonesLabel != null)
                _stonesLabel.text = $"가진 강화석 {target.EnhancementStones:N0}개";

            for (var i = 0; i < GradeCount; i++)
            {
                if (_gradeLabels[i] == null) continue;
                var grade = (DungeonGrade)i;
                _gradeLabels[i].text = $"{grade} 등급 — 강화석 {target.DungeonStonesPreview(grade)}개";
            }

            // 한 판이 아직 없다는 사실을 화면에도 적는다. 남은 횟수가 0일 때는 그쪽을 먼저 말한다.
            if (_noteLabel != null)
                _noteLabel.text = left <= 0
                    ? "오늘은 다 썼다. 내일 자정에 세 번 다시 찬다."
                    : "한 판은 60초 채굴 뒤 레이스 1판이다. 아직 만드는 중이다.";

            if (_enterButton != null) _enterButton.interactable = false;
            if (_enterLabel != null) _enterLabel.text = left <= 0 ? "오늘 끝" : "준비 중";
        }
    }
}
