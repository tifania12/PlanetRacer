using TMPro;
using UnityEngine;
using GemRacer.Core;

namespace GemRacer.UI
{
    /// <summary>
    /// P-15: 펫 뽑기 확률 공개 화면. <see cref="PetGachaTable"/>·<see cref="PetFusion"/>을
    /// 그대로 읽어서 표로 그린다 — 사람이 숫자를 옮겨 적지 않는다. 확률표가 나중에 바뀌어도
    /// 이 화면은 코드를 안 고쳐도 그대로 맞는다.
    ///
    /// 캡슐형(뽑기 4종)과 합성형(중복 조각 환산)을 카드로 나눠 보여준다. 각 카드의 줄 수는
    /// 확률표 길이(무료 5 / 일반 6 / 고급 4 / 특수 3)와 승급 단계 수(6, Common→Transcendent)로
    /// 고정돼 있어 부트스트랩이 그 개수만큼 빈 줄을 만들어 두면 여기서 이름으로 채우기만 한다.
    /// </summary>
    public sealed class PetGachaOddsUgui : MonoBehaviour
    {
        void Awake()
        {
            FillCapsule("free", PetGachaTable.Free(), null);
            FillCapsule("normal", PetGachaTable.Normal(), null);
            FillCapsule("advanced", PetGachaTable.Advanced(),
                $"{PetGachaTable.AdvancedPityCount}뽑째 {PetGradeInfo.NameKoFor(PetGachaTable.AdvancedPityGrade)} 확정 · " +
                $"10연차 {PetGradeInfo.NameKoFor(PetGachaTable.AdvancedTenPullMinGrade)} 이상 1마리 확정");
            FillCapsule("special", PetGachaTable.Special(),
                $"{PetGachaTable.SpecialPityCount}뽑째 {PetGradeInfo.NameKoFor(PetGachaTable.SpecialPityGrade)} 확정 · " +
                "초월의 인장으로만 입장");

            var same = UiKit.Find<TMP_Text>(transform, "fusion-same", warnIfMissing: false);
            if (same != null)
                same.text = $"같은 등급 조각 {PetFusion.SameGradeFragmentCost}개 → 그 등급 다른 펫 1마리 (모든 등급 공통, 조각은 안 버려짐)";

            for (var grade = PetGrade.Common; grade < PetGrade.Transcendent; grade++)
            {
                var label = UiKit.Find<TMP_Text>(transform, $"fusion-promo-{(int)grade}", warnIfMissing: false);
                if (label == null) continue;
                var promoted = PetFusion.PromotedGrade(grade);
                label.text = $"{PetGradeInfo.NameKoFor(grade)} → {PetGradeInfo.NameKoFor(promoted)}: 조각 {PetFusion.PromotionCost(grade)}개";
            }
        }

        void FillCapsule(string prefix, System.Collections.Generic.IReadOnlyList<PetGachaWeight> weights, string note)
        {
            for (var i = 0; i < weights.Count; i++)
            {
                var label = UiKit.Find<TMP_Text>(transform, $"{prefix}-row-{i}", warnIfMissing: false);
                if (label == null) continue;
                label.text = $"{PetGradeInfo.NameKoFor(weights[i].Grade)}: {weights[i].Weight * 100f:0.0}%";
            }

            // 무료·일반 뽑기는 천장이 없어 note가 null이다 — 빈 문자열로 두면 카드 아래에
            // 빈 줄만 남으므로(P-15 로그, 2026-09-19) 아예 줄을 꺼 버린다.
            var noteLabel = UiKit.Find<TMP_Text>(transform, $"{prefix}-note", warnIfMissing: false);
            if (noteLabel != null)
            {
                noteLabel.gameObject.SetActive(note != null);
                if (note != null) noteLabel.text = note;
            }
        }
    }
}
