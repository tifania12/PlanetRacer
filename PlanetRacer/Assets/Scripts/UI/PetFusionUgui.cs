using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.Core;
using GemRacer.Mining;

namespace GemRacer.UI
{
    /// <summary>
    /// A-17 조각 합성(2026-09-22, pet-gacha.md 9절): PetGachaPullUgui의 btn-fusion이 여는 화면.
    /// PetFusionController.FuseSameGrade/FusePromotion를 등급별 버튼으로 부르기만 한다 —
    /// PetGachaPullUgui와 같은 모양(target 찾기, Update()로 매 프레임 다시 그리기, seed는
    /// 여기서 UnityEngine.Random으로 뽑아 코어에 넘김).
    /// </summary>
    public sealed class PetFusionUgui : MonoBehaviour
    {
        [Tooltip("합성 대상. 비워두면 씬에서 하나 찾는다.")]
        public MiningController target;

        static readonly PetGrade[] Grades =
        {
            PetGrade.Common, PetGrade.Advanced, PetGrade.Rare, PetGrade.Epic,
            PetGrade.Legendary, PetGrade.Mythic, PetGrade.Transcendent,
        };

        TMP_Text[] _shardLabels;
        Button[] _fuseButtons;
        Button[] _promoteButtons;
        TMP_Text _resultTitle, _resultNote;

        void Awake()
        {
            if (target == null) target = FindFirstObjectByType<MiningController>();

            _shardLabels = new TMP_Text[Grades.Length];
            _fuseButtons = new Button[Grades.Length];
            _promoteButtons = new Button[Grades.Length];

            for (var i = 0; i < Grades.Length; i++)
            {
                _shardLabels[i] = UiKit.Find<TMP_Text>(transform, $"fusion-shards-{i}", false);
                _fuseButtons[i] = UiKit.Find<Button>(transform, $"btn-fuse-{i}", false);
                _promoteButtons[i] = UiKit.Find<Button>(transform, $"btn-promote-{i}", false);

                var grade = Grades[i];
                _fuseButtons[i]?.onClick.AddListener(() => OnFuseClicked(grade));
                _promoteButtons[i]?.onClick.AddListener(() => OnPromoteClicked(grade));
            }

            _resultTitle = UiKit.Find<TMP_Text>(transform, "fusion-result-title", false);
            _resultNote = UiKit.Find<TMP_Text>(transform, "fusion-result-note", false);
        }

        // 조각 수는 뽑기 화면에서도 바뀔 수 있으니(같은 SaveData) PetGachaPullUgui.Update와
        // 같은 이유로 매 프레임 다시 그린다.
        void Update() => Refresh();

        void Refresh()
        {
            if (target == null) return;
            var gacha = target.PetGacha;

            for (var i = 0; i < Grades.Length; i++)
            {
                var grade = Grades[i];
                var shards = gacha.Shards(grade);

                if (_shardLabels[i] != null)
                    _shardLabels[i].text = $"{PetGradeInfo.NameKoFor(grade)} 조각 {shards}개";

                if (_fuseButtons[i] != null)
                    _fuseButtons[i].interactable = shards >= PetFusion.SameGradeFragmentCost;

                if (_promoteButtons[i] != null)
                {
                    // Transcendent(7등급)는 더 위 등급이 없다 — PetFusion.PromotionCost가
                    // 예외를 던지니 버튼 자체를 여기서 꺼 둔다(코어 주석의 "화면이 숨긴다" 그대로).
                    if (grade == PetGrade.Transcendent)
                        _promoteButtons[i].gameObject.SetActive(false);
                    else
                        _promoteButtons[i].interactable = shards >= PetFusion.PromotionCost(grade);
                }
            }
        }

        static int NextSeed() => Random.Range(int.MinValue, int.MaxValue);

        void OnFuseClicked(PetGrade grade)
        {
            if (target == null) return;
            var outcome = target.FuseSameGradePet(grade, NextSeed());

            if (outcome.Pets.Length == 0)
            {
                ShowResult($"{PetGradeInfo.NameKoFor(grade)} 조각이 부족하다",
                    $"합성엔 {PetFusion.SameGradeFragmentCost}개가 필요하다 — 남은 조각 {outcome.RemainingFragments}개");
                return;
            }

            var names = new System.Text.StringBuilder();
            for (var i = 0; i < outcome.Pets.Length; i++)
            {
                if (i > 0) names.Append(", ");
                var def = PetSpeciesTable.Get(outcome.Pets[i].SpeciesId);
                names.Append(PetSpeciesTable.DisplayNameKo(def));
            }
            ShowResult($"{outcome.Pets.Length}마리 획득", $"{names} · 남은 조각 {outcome.RemainingFragments}개");
        }

        void OnPromoteClicked(PetGrade grade)
        {
            if (target == null) return;
            var outcome = target.FusePromotionPet(grade);

            if (outcome.Promotions == 0)
            {
                ShowResult($"{PetGradeInfo.NameKoFor(grade)} 조각이 부족하다",
                    $"승급엔 {PetFusion.PromotionCost(grade)}개가 필요하다 — 남은 조각 {outcome.RemainingFragments}개");
                return;
            }

            var promoted = PetFusion.PromotedGrade(grade);
            ShowResult($"{PetGradeInfo.NameKoFor(promoted)} 조각 +{outcome.Promotions}개",
                $"{PetGradeInfo.NameKoFor(grade)} 조각 남은 것 {outcome.RemainingFragments}개");
        }

        void ShowResult(string title, string note)
        {
            if (_resultTitle != null) _resultTitle.text = title;
            if (_resultNote != null) _resultNote.text = note;
        }
    }
}
