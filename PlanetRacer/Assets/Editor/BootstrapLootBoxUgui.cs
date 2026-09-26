using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.UI;

namespace GemRacer.EditorTools
{
    /// <summary>
    /// U-05(2026-09-16): 공구 상자 화면(LootBoxPanel, UI Toolkit)을 일반 UI(uGUI)로 세운다.
    /// `UI Canvas/Overlays` 아래에 붙고, HUD의 "상자" 버튼이 여닫는다(BootstrapHudUgui,
    /// `MainHudUgui.boxPanel`에 이 패널을 물려야 실제로 열린다 — 씬 배선은 Unity 세션 몫).
    ///
    /// 세 줄(녹슨/강철/티타늄)을 BootstrapUpgradeUgui와 같은 방식으로 GridLayoutGroup(Flexible)에
    /// 담는다 — 줄마다 이름+보유 개수+열기 버튼뿐이라 업그레이드 화면(레벨+효과 두 줄)보다 내용이
    /// 적어서 셀 높이를 더 낮게 잡았다. 세로 한 칸일 때 목록 높이가 3×140+2×12=444px로
    /// 업그레이드(3×168+2×12=528px, U-02에서 확인 완료)보다 작으니 넘칠 걱정은 없어 보이지만,
    /// 결과 카드까지 더하면 실제로 얼마나 되는지는 **Editor에서 세 기준점 다 봐야 확실하다**
    /// (U-03 부품 제작 화면이 손계산으로는 괜찮아 보였다가 실제로는 넘쳤던 전례가 있다).
    /// </summary>
    public static class BootstrapLootBoxUgui
    {
        static readonly Color Ink      = new Color(0.91f, 0.93f, 1f);
        static readonly Color Dim      = new Color(0.55f, 0.58f, 0.72f);
        static readonly Color Bg       = new Color(0.07f, 0.07f, 0.09f, 0.97f);
        static readonly Color RowFace  = new Color(0.12f, 0.13f, 0.19f, 1f);
        static readonly Color BtnFace  = new Color(0.28f, 0.34f, 0.62f, 1f);
        static readonly Color ResultFace = new Color(0.15f, 0.15f, 0.2f, 1f);
        static readonly Color ResultInk  = new Color(0.85f, 0.87f, 0.98f);

        const float CellWidth = 400f;
        const float CellHeight = 140f;
        const float CellSpacing = 12f;

        static readonly (string prefix, string label, string icon)[] Rows =
        {
            ("rusty", "녹슨 상자", "icon-box-rusty"),
            ("steel", "강철 상자", "icon-box-steel"),
            ("titanium", "티타늄 상자", "icon-box-titanium"),
        };

        [MenuItem("GemRacer/19. 공구 상자 화면 세우기 (uGUI)")]
        public static void Build()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Pretendard-Regular SDF.asset");
            if (font == null) { Debug.LogError("[GemRacer] 한글 폰트 에셋이 없다. 'GemRacer/11' 먼저."); return; }

            var overlays = GameObject.Find("UI Canvas/Overlays");
            if (overlays == null) { Debug.LogError("[GemRacer] 'UI Canvas/Overlays'가 없다. 'GemRacer/13' 먼저."); return; }

            var old = overlays.transform.Find("LootBox");
            if (old != null) Object.DestroyImmediate(old.gameObject);

            var root = NewRect("LootBox", overlays.transform);
            root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero; root.offsetMax = Vector2.zero;

            var bg = root.gameObject.AddComponent<Image>();
            bg.color = Bg;
            bg.raycastTarget = true; // 열려 있는 동안 뒤(3D 뷰·HUD)로 클릭이 새지 않게 막는다

            var panel = root.gameObject.AddComponent<UiPanel>();
            panel.hiddenOnStart = true; // 오버레이 화면이라 기본은 닫힌 채로 시작
            root.gameObject.AddComponent<LootBoxUgui>();

            var col = root.gameObject.AddComponent<VerticalLayoutGroup>();
            col.padding = new RectOffset(20, 20, 20, 20);
            col.spacing = 8f;
            col.childAlignment = TextAnchor.UpperLeft;
            col.childForceExpandWidth = true;
            col.childForceExpandHeight = false;
            col.childControlWidth = true;
            col.childControlHeight = true;

            MakeHeaderText("lootbox-title", "공구 상자", root, font, 24, Ink, 32f);

            var rowList = NewRect("row-list", root);
            var rowListLayout = rowList.gameObject.AddComponent<LayoutElement>();
            rowListLayout.flexibleHeight = 1f;
            var grid = rowList.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(CellWidth, CellHeight);
            grid.spacing = new Vector2(CellSpacing, CellSpacing);
            grid.childAlignment = TextAnchor.UpperLeft;
            grid.constraint = GridLayoutGroup.Constraint.Flexible;
            // U-11(2026-09-26): 칸 폭을 고정하지 않고 부모 폭에서 계산한다 — 세로는 한 칸,
            // 가로·태블릿은 두 칸으로 재배치된다(CLAUDE.md 6번). 계산은 ResponsiveGridCells가 한다.
            var gridFit = rowList.gameObject.AddComponent<ResponsiveGridCells>();
            gridFit.minCellWidth = 330f;
            gridFit.maxColumns = 2;
            foreach (var (prefix, label, icon) in Rows)
                MakeRow(rowList, font, prefix, label, icon);

            // 결과 카드. 상자를 열면 LootBoxUgui가 이 라벨의 text를 바꾼다 — 줄바꿈을 켜서
            // 등급·부품 이름이 길어도 카드 밖으로 안 넘치게 한다.
            var resultCard = NewRect("result-card", root);
            var resultLayout = resultCard.gameObject.AddComponent<LayoutElement>();
            resultLayout.minHeight = 72f;
            resultLayout.preferredHeight = 72f;
            // flexibleHeight를 안 정하면 기본값 -1("무시")이라 LayoutUtility가 이 LayoutElement를
            // 건너뛰고, 같은 오브젝트의 VerticalLayoutGroup(childForceExpandHeight = true)이 보고하는
            // flexibleHeight를 대신 쓴다. 그러면 부모 세로 그룹이 남은 높이를 이 카드에도 나눠 줘서
            // 72px 카드가 224px로 부푼다(2026-09-16 U-05 배선에서 실제로 그랬다, ugui-migration.md 3-2).
            resultLayout.flexibleHeight = 0f;
            var resultImg = resultCard.gameObject.AddComponent<Image>();
            resultImg.color = ResultFace;
            resultImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            resultImg.type = Image.Type.Sliced;

            var resultPad = resultCard.gameObject.AddComponent<VerticalLayoutGroup>();
            resultPad.padding = new RectOffset(14, 14, 10, 10);
            resultPad.childAlignment = TextAnchor.MiddleLeft;
            resultPad.childForceExpandWidth = true;
            resultPad.childForceExpandHeight = true;
            resultPad.childControlWidth = true;
            resultPad.childControlHeight = true;

            var resultLabelRt = NewRect("result-label", resultCard);
            var resultText = resultLabelRt.gameObject.AddComponent<TextMeshProUGUI>();
            resultText.font = font;
            resultText.text = "상자를 열면 결과가 여기 뜬다";
            resultText.fontSize = 15;
            resultText.color = ResultInk;
            resultText.alignment = TextAlignmentOptions.MidlineLeft;
            resultText.raycastTarget = false;
            resultText.enableWordWrapping = true;

            // 닫기 버튼. 이 패널도 화면을 꽉 채우고 뒤로 클릭을 막기 때문에, 이게 없으면 한 번
            // 열었을 때 HUD의 "상자" 버튼까지 가려져서 빠져나올 길이 없다(ugui-migration.md 3-1번).
            // onClick은 인스펙터 OnClick 칸에 보이는 영구 리스너로 걸어 둔다.
            var closeBtn = MakeButton("close-button", "닫기", root, font, RowFace);
            var closeLayout = closeBtn.gameObject.AddComponent<LayoutElement>();
            closeLayout.minHeight = 44f;
            closeLayout.preferredHeight = 44f;
            closeLayout.flexibleHeight = 0f;
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
                closeBtn.onClick, new UnityEngine.Events.UnityAction(panel.Hide));

            Selection.activeObject = root.gameObject;
            EditorUtility.SetDirty(root.gameObject);
            Debug.Log("[GemRacer] 공구 상자 화면(uGUI) 세움. MainHudUgui.boxPanel에 이 'LootBox'를 물려야 " +
                      "HUD의 '상자' 버튼으로 실제로 열린다.");
        }

        static void MakeRow(RectTransform parent, TMP_FontAsset font, string prefix, string nameText, string iconName)
        {
            var row = NewRect($"row-{prefix}", parent);

            var img = row.gameObject.AddComponent<Image>();
            img.color = RowFace;
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            img.type = Image.Type.Sliced;

            var col = row.gameObject.AddComponent<VerticalLayoutGroup>();
            col.padding = new RectOffset(14, 14, 12, 12);
            col.spacing = 6f;
            col.childAlignment = TextAnchor.UpperLeft;
            col.childForceExpandWidth = true;
            col.childForceExpandHeight = false;
            col.childControlWidth = true;
            col.childControlHeight = true;

            // A-16: 아이콘 + 이름을 한 줄에.
            var head = NewRect($"{prefix}-head", row);
            var headLayout = head.gameObject.AddComponent<LayoutElement>();
            headLayout.minHeight = 26f; headLayout.preferredHeight = 26f;
            headLayout.flexibleHeight = 0f;
            var headRow = head.gameObject.AddComponent<HorizontalLayoutGroup>();
            headRow.spacing = 8f;
            headRow.childAlignment = TextAnchor.MiddleLeft;
            headRow.childForceExpandWidth = false;
            headRow.childForceExpandHeight = true;
            headRow.childControlWidth = false;
            headRow.childControlHeight = true;

            MakeIcon($"{prefix}-icon", head, 22f);
            MakeInlineText($"{prefix}-name", nameText, head, font, 18, Ink);

            MakeHeaderText($"{prefix}-count", "보유 0개", row, font, 14, Dim, 20f);
            MakeButton($"{prefix}-button", "열기", row, font, BtnFace);
        }

        // A-16: 상자 아이콘 자리. 그림이 없는 동안은 투명 — LootBoxUgui.cs가 Awake에서
        // UiKit.SetIcon으로 실제 스프라이트를 입힌다.
        static Image MakeIcon(string name, RectTransform parent, float size)
        {
            var rt = NewRect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.raycastTarget = false;
            img.preserveAspect = true;
            img.color = new Color(1f, 1f, 1f, 0f);
            var le = rt.gameObject.AddComponent<LayoutElement>();
            le.minWidth = size; le.preferredWidth = size;
            le.minHeight = size; le.preferredHeight = size;
            return img;
        }

        static TMP_Text MakeInlineText(string name, string text, RectTransform parent, TMP_FontAsset font,
                                       float size, Color color)
        {
            var rt = NewRect(name, parent);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = font;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.raycastTarget = false;
            t.enableWordWrapping = false;
            return t;
        }

        // --- 조각 만들기 ---------------------------------------------------

        static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        static TMP_Text MakeHeaderText(string name, string text, RectTransform parent, TMP_FontAsset font,
                                       float size, Color color, float height)
        {
            var rt = NewRect(name, parent);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = font;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = TextAlignmentOptions.MidlineLeft;
            t.raycastTarget = false;
            t.enableWordWrapping = false;
            var le = rt.gameObject.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;
            le.flexibleHeight = 0f;
            return t;
        }

        static Button MakeButton(string name, string label, RectTransform parent, TMP_FontAsset font, Color face)
        {
            var rt = NewRect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = face;
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            img.type = Image.Type.Sliced;

            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;

            var le = rt.gameObject.AddComponent<LayoutElement>();
            le.minHeight = 44f;
            le.preferredHeight = 44f;
            le.flexibleHeight = 0f;

            var labelRt = NewRect("label", rt);
            labelRt.anchorMin = Vector2.zero; labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero; labelRt.offsetMax = Vector2.zero;
            var t = labelRt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = font;
            t.text = label;
            t.fontSize = 14;
            t.color = Color.white;
            t.alignment = TextAlignmentOptions.Center;
            t.raycastTarget = false;
            t.enableWordWrapping = false;

            return btn;
        }
    }
}
