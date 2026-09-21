using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.Core;
using GemRacer.Mining;

namespace GemRacer.UI
{
    /// <summary>
    /// A-17(2026-09-21, pet-gacha.md 9-2): 펫 도감 그리드. 부트스트랩(BootstrapPetDexUgui,
    /// GemRacer/27)이 124칸(dex-cell-{speciesId})을 등급별 섹션으로 미리 만들어 두면, 이 스크립트는
    /// Awake에서 이름만으로 그 칸들을 찾아 두고, 패널이 열릴 때(OnEnable)마다 보유 종만
    /// PetArt.ResourcePath로 스프라이트를 입힌다 — 미보유는 부트스트랩이 만든 회색 실루엣
    /// 그대로 둔다(2026-09-15 "이미지가 없다고 멈추지 않는다"와 같은 방향).
    ///
    /// 도감 안에서는 보유 상태가 스스로 안 바뀐다(뽑기는 다른 화면 몫) — 그래서 PetGachaPullUgui와
    /// 달리 Update()가 아니라 OnEnable()에서만 다시 그린다. 장착은 상세 팝업이 바로 반영한다.
    /// </summary>
    public sealed class PetDexUgui : MonoBehaviour
    {
        [Tooltip("도감 대상. 비워두면 씬에서 하나 찾는다.")]
        public MiningController target;

        TMP_Text[] _headerLabels;
        Image[] _cellImages; // 인덱스 = speciesId(PetSpeciesTable.All 순서 그대로).

        GameObject _detailPanel;
        Image _detailPortrait;
        TMP_Text _detailName, _detailGrade;
        Button _detailEquip;

        int _selectedSpeciesId = -1;

        void Awake()
        {
            if (target == null) target = FindFirstObjectByType<MiningController>();

            var gradeCount = PetGradeInfo.NameKo.Length;
            _headerLabels = new TMP_Text[gradeCount];
            for (var g = 0; g < gradeCount; g++)
                _headerLabels[g] = UiKit.Find<TMP_Text>(transform, $"dex-header-{g}", false);

            var total = PetSpeciesTable.All.Count;
            _cellImages = new Image[total];
            for (var id = 0; id < total; id++)
            {
                var cellObj = UiKit.FindObject(transform, $"dex-cell-{id}", false);
                if (cellObj == null) continue;
                _cellImages[id] = cellObj.GetComponent<Image>();
                var btn = cellObj.GetComponent<Button>();
                var capturedId = id; // 클로저가 루프 변수를 그대로 캡처하면 전부 마지막 id가 된다.
                btn?.onClick.AddListener(() => OpenDetail(capturedId));
            }

            _detailPanel = UiKit.FindObject(transform, "dex-detail-panel", false);
            _detailPortrait = UiKit.Find<Image>(transform, "dex-detail-portrait", false);
            _detailName = UiKit.Find<TMP_Text>(transform, "dex-detail-name", false);
            _detailGrade = UiKit.Find<TMP_Text>(transform, "dex-detail-grade", false);
            _detailEquip = UiKit.Find<Button>(transform, "dex-detail-equip", false);
            var detailClose = UiKit.Find<Button>(transform, "dex-detail-close", false);

            _detailEquip?.onClick.AddListener(OnEquipClicked);
            detailClose?.onClick.AddListener(() => _detailPanel?.SetActive(false));

            _detailPanel?.SetActive(false);
        }

        void OnEnable() => Refresh();

        void Refresh()
        {
            if (target == null || _headerLabels == null) return;
            var save = target.PetGacha;

            for (var g = 0; g < _headerLabels.Length; g++)
            {
                if (_headerLabels[g] == null) continue;
                var grade = (PetGrade)g;
                _headerLabels[g].text =
                    $"{PetGradeInfo.NameKoFor(grade)} {save.OwnedSpeciesCount(grade)}/{PetGradeInfo.SpeciesCountFor(grade)}";
            }

            for (var id = 0; id < _cellImages.Length; id++)
            {
                if (_cellImages[id] == null || !save.OwnsSpecies(id)) continue;
                var sprite = UiKit.LoadSpriteAtPath(PetArt.ResourcePath(PetSpeciesTable.Get(id)));
                if (sprite == null) continue;
                _cellImages[id].sprite = sprite;
                _cellImages[id].color = Color.white;
            }
        }

        void OpenDetail(int speciesId)
        {
            _selectedSpeciesId = speciesId;
            if (target == null || _detailPanel == null) return;

            var save = target.PetGacha;
            var owned = save.OwnsSpecies(speciesId);
            var def = PetSpeciesTable.Get(speciesId);

            if (_detailPortrait != null)
            {
                var sprite = owned ? UiKit.LoadSpriteAtPath(PetArt.ResourcePath(def)) : null;
                if (sprite != null) { _detailPortrait.sprite = sprite; _detailPortrait.color = Color.white; }
                else { _detailPortrait.sprite = null; _detailPortrait.color = new Color(1f, 1f, 1f, 0.08f); }
            }

            // DisplayNameKo는 6·7등급도 예외 없이 이름을 준다(MechanicalDisplayNameKo와 다른 점,
            // PetSpeciesTable.DisplayNameKo 주석 그대로) — 팝업은 그림 유무와 무관하게 항상 이름을 보여준다.
            if (_detailName != null) _detailName.text = owned ? PetSpeciesTable.DisplayNameKo(def) : "???";
            if (_detailGrade != null)
                _detailGrade.text = PetGradeInfo.NameKoFor(def.Grade) +
                    (!owned ? " · 미보유" : save.EquippedSpeciesId == speciesId ? " · 장착 중" : "");

            if (_detailEquip != null)
                _detailEquip.interactable = owned && save.EquippedSpeciesId != speciesId;

            _detailPanel.SetActive(true);
        }

        void OnEquipClicked()
        {
            if (target == null || _selectedSpeciesId < 0) return;
            target.EquipPetSpecies(_selectedSpeciesId);
            OpenDetail(_selectedSpeciesId); // "장착 중" 표시·버튼 비활성화를 바로 반영.
        }
    }
}
