using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GemRacer.EditorTools
{
    /// <summary>
    /// A-17(2026-09-21): 펫 뽑기 실행 화면(`GemRacer/26`)·펫 도감(`GemRacer/27`)을 여는 버튼
    /// 둘을 HUD의 action-row에 추가한다. `BootstrapShopUgui.AddShopButtonToActionRow`
    /// (GemRacer/23)와 같은 이유로 additive 메뉴다 — `BootstrapHudUgui.Build()`(GemRacer/13)를
    /// 다시 누르면 이미 배선된 일곱 화면이 통째로 날아가니(CLAUDE.md
    /// "GemRacer/7을 다시 누르면..." 항목과 같은 함정), action-row만 찾아서 그 밑에
    /// "btn-pet-gacha"·"btn-pet-dex" 두 개만 추가한다. 같은 메뉴를 다시 눌러도 기존 것을
    /// 지우고 새로 만들 뿐이라(CLAUDE.md 3번, 멱등) 안전하다.
    ///
    /// 이 버튼 둘이 붙으면 action-row가 여덟 칸이 된다 — `BootstrapHudUgui.MakeButton`의
    /// 자동 축소(14~20pt)가 이미 있어 라벨이 잘리진 않겠지만, "뽑기"/"도감"처럼 짧은
    /// 라벨이라도 실제로 8칸이 눈에 읽히는 크기인지는 Unity 세션이 스크린샷으로 확인할 것.
    /// </summary>
    public static class BootstrapPetHudButtons
    {
        static readonly Color HudBtnFace = new Color(0.16f, 0.18f, 0.26f, 1f);
        static readonly Color Ink = new Color(0.91f, 0.93f, 1f);

        [MenuItem("GemRacer/28. HUD에 펫 뽑기·도감 버튼 추가 (uGUI, 안전 — HUD만 건드림)")]
        public static void AddPetButtonsToActionRow()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Pretendard-Regular SDF.asset");
            if (font == null) { Debug.LogError("[GemRacer] 한글 폰트 에셋이 없다. 'GemRacer/11' 먼저."); return; }

            var actionRowGo = GameObject.Find("UI Canvas/HUD/action-row");
            if (actionRowGo == null) { Debug.LogError("[GemRacer] 'UI Canvas/HUD/action-row'가 없다. 'GemRacer/13' 먼저."); return; }
            var actionRow = (RectTransform)actionRowGo.transform;

            MakeOrReplaceButton(actionRow, font, "btn-pet-gacha", "뽑기");
            MakeOrReplaceButton(actionRow, font, "btn-pet-dex", "도감");

            EditorUtility.SetDirty(actionRowGo);
            Debug.Log("[GemRacer] HUD action-row에 'btn-pet-gacha'·'btn-pet-dex' 추가함. " +
                      "MainHudUgui.petGachaPullPanel/petDexPanel에 각각 'PetGachaPull'·'PetDex'를 " +
                      "물려야 버튼이 켜진다(MainHudUgui가 Awake에서 패널 없으면 자동으로 끈다).");
        }

        static void MakeOrReplaceButton(RectTransform actionRow, TMP_FontAsset font, string name, string label)
        {
            var old = actionRow.Find(name);
            if (old != null) Object.DestroyImmediate(old.gameObject); // 멱등 — 다시 눌러도 결과가 같다

            var rt = NewRect(name, actionRow);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = HudBtnFace;
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            img.type = Image.Type.Sliced;

            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;

            var labelRt = NewRect("label", rt);
            labelRt.anchorMin = Vector2.zero; labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero; labelRt.offsetMax = Vector2.zero;
            var t = labelRt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = font;
            t.text = label;
            t.fontSize = 20;
            t.color = Ink;
            t.alignment = TextAlignmentOptions.Center;
            t.raycastTarget = false;
            t.enableWordWrapping = false;
            t.overflowMode = TextOverflowModes.Ellipsis;
            t.enableAutoSizing = true;
            t.fontSizeMin = 14f;
            t.fontSizeMax = 20f;
        }

        static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }
    }
}
