using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.Core;
using GemRacer.Mining;

namespace GemRacer.UI
{
    /// <summary>
    /// A-17(2026-09-21, pet-gacha.md 9-2): 펫 도감 그리드. 부트스트랩(BootstrapPetDexUgui,
    /// GemRacer/27)이 124칸(slot-dex-{speciesId})을 등급별 섹션으로 미리 만들어 두면, 이 스크립트는
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

        /// <summary>A-28-2: 이 화면의 주 버튼은 상세 팝업의 「장착」 한 칸뿐이다.
        /// 도감은 **보는** 화면이라 주 동작이 하나다 — 칸 124개도 버튼이지만 주인공은 그림
        /// (보유는 펫 초상화, 미보유는 빈 칸)이라 주 버튼으로 두면 밝은 알약 124개가 격자를
        /// 덮는다. 닫기 둘(화면 닫기·팝업 닫기)도 보조 버튼으로 둔다.
        /// 부트스트랩(`GemRacer/27`)도 이 배열을 그대로 쓴다 — 양쪽이 어긋나지 않게.</summary>
        public static readonly string[] PrimaryButtons = { "dex-detail-equip" };

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
                // A-28-2(2026-10-14): 이름이 `dex-cell-{id}`에서 `slot-dex-{id}`로 바뀌었다 —
                // `UiSkin.ApplyToPopup`이 칸 그림(`ui-slot`)을 얹는 이름 규칙이 `slot-`이다.
                var cellObj = UiKit.FindObject(transform, $"slot-dex-{id}", false);
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

            // A-28-2: 키트 스킨. **카드 이름을 비운다** — 루트 Image가 화면을 꽉 채우는
            // 가림막이라 둥근 창을 얹을 자리가 없다(제작·상자·레이스 출전·뽑기와 같은 이유).
            // 비우면 함수가 화면 전체를 훑어서 버튼 셋과 칸 124개(`slot-dex-*`)가 한 번에 걸린다.
            UiSkin.ApplyToPopup(transform, null, PrimaryButtons);

            // **가림막 안에 진짜 창이 하나 더 있는 첫 화면이다.** 상세 팝업(320x260)은 이름이
            // `row-`도 `slot-`도 아니라 위 함수가 못 잡는다 — 이름을 바꿔 잡히게 하면 「목록 한 줄」
            // 그림이 얹히니(창이 아니다) `ui-popup`을 여기서 직접 얹는다. 앞 다섯 화면은 가림막
            // 하나뿐이어서 이 자리가 없었다.
            if (_detailPanel != null)
                UiSkin.ApplySliced(_detailPanel.GetComponent<Image>(), UiSkin.Popup);
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
