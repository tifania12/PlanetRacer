using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace GemRacer.EditorTools
{
    /// <summary>
    /// A-24(2026-09-29): HUD 액션 줄 버튼을 "아이콘 + 아래 작은 글자"로 바꾼다.
    ///
    /// 왜 필요했나 — 액션 줄이 열 칸이 되면서 한 칸이 44.4px까지 줄었다. 20pt 글자로는
    /// "업그레이드"(86.4px)·"레이스"(51.9px)·"시즌패스"(69.1px) 셋이 말줄임으로 잘렸다(B-03).
    /// 글자를 줄이거나 줄을 접는 건 취향 문제라 Tifania 결정 대기였지만, 아이콘을 얹어
    /// 글자를 보조로 내리는 길은 backlog A-24로 이미 정해져 있던 방향이다.
    ///
    /// 왜 파일을 따로 뺐나 — 액션 줄에 버튼을 더하는 메뉴가 넷이다(GemRacer/23 상점,
    /// 28 펫, 32 행성, 34 시즌패스). 모양 잡는 코드를 각자 갖게 두면 넷이 서로 다른 모양이
    /// 된다. 그래서 ShapeButton 하나로 모으고, 이미 세워진 씬은 ApplyToOpenScene 메뉴로
    /// 한 번에 맞춘다.
    ///
    /// 이 메뉴는 씬을 새로 만들지 않는다. 열려 있는 씬의 action-row만 찾아 고친다 —
    /// CLAUDE.md의 "GemRacer/7을 다시 누르지 않는다" 함정과 같은 이유다.
    /// 여러 번 눌러도 같은 결과가 나온다(멱등).
    /// </summary>
    public static class BootstrapHudIcons
    {
        const string FontPath = "Assets/Fonts/Pretendard-Regular SDF.asset";

        static readonly Color Ink = new Color(0.91f, 0.93f, 1f);

        /// <summary>버튼 이름 → Assets/Resources/Art/Icons/Hud/ 밑 그림 이름.
        /// MainHudUgui.Wire가 런타임에 넣는 것과 같은 짝이다. 여기서도 넣어 두는 이유는
        /// 플레이하지 않고 씬만 봐도 아이콘이 보이게 하려는 것.</summary>
        public static readonly Dictionary<string, string> IconByButton = new Dictionary<string, string>
        {
            { "btn-mine",       "hud-upgrade" },
            { "btn-craft",      "hud-craft" },
            { "btn-race",       "hud-race" },
            { "btn-box",        "hud-box" },
            { "btn-settings",   "hud-settings" },
            { "btn-shop",       "hud-shop" },
            { "btn-pet-gacha",  "hud-gacha" },
            { "btn-pet-dex",    "hud-dex" },
            { "btn-planet",     "hud-planet" },
            { "btn-seasonpass", "hud-seasonpass" },
        };

        [MenuItem("GemRacer/35. HUD 액션 줄 버튼에 아이콘 입히기 (A-24)")]
        public static void ApplyToOpenScene()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            var row = FindActionRow();
            if (row == null)
            {
                Debug.LogWarning("[GemRacer] 열린 씬에서 'action-row'를 못 찾았다. MainGame 씬을 먼저 연다.");
                return;
            }

            int shaped = 0;
            for (int i = 0; i < row.childCount; i++)
            {
                var child = row.GetChild(i) as RectTransform;
                if (child == null || child.GetComponent<Button>() == null) continue;

                // 라벨 글자는 런타임에 MainHudUgui.Wire가 다시 쓴다. 여기서는 지금 것을 그대로 둔다.
                var existing = child.Find("label");
                var keep = existing != null ? existing.GetComponent<TextMeshProUGUI>() : null;
                ShapeButton(child, keep != null ? keep.text : child.name, font);
                shaped++;
            }

            EditorSceneManager.MarkSceneDirty(row.gameObject.scene);
            Debug.Log($"[GemRacer] A-24: 액션 줄 버튼 {shaped}개에 아이콘 자리를 잡고 라벨을 아래 띠로 내렸다.");
        }

        /// <summary>버튼 하나를 "위 아이콘 + 아래 작은 글자" 모양으로 만든다.
        /// 이미 그 모양이면 값만 다시 맞춘다 — 몇 번을 불러도 결과가 같다.</summary>
        public static void ShapeButton(RectTransform buttonRt, string label, TMP_FontAsset font)
        {
            if (buttonRt == null) return;

            // --- 아이콘 자리 ---
            var iconName = buttonRt.name + "-icon";
            var iconRt = FindOrCreate(buttonRt, iconName);
            iconRt.anchorMin = new Vector2(0.5f, 1f);
            iconRt.anchorMax = new Vector2(0.5f, 1f);
            iconRt.pivot     = new Vector2(0.5f, 1f);
            iconRt.sizeDelta = new Vector2(36f, 36f);
            iconRt.anchoredPosition = new Vector2(0f, -8f);

            var iconImg = iconRt.GetComponent<Image>();
            if (iconImg == null) iconImg = iconRt.gameObject.AddComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            var sprite = LoadHudSprite(buttonRt.name);
            if (sprite != null)
            {
                iconImg.sprite = sprite;
                iconImg.color = Color.white;
            }
            else
            {
                // 그림이 아직 없으면 회색 네모 대신 아무것도 안 보이게 둔다.
                iconImg.sprite = null;
                iconImg.color = new Color(1f, 1f, 1f, 0f);
            }

            // --- 라벨 띠 ---
            var labelRt = FindOrCreate(buttonRt, "label");
            labelRt.anchorMin = new Vector2(0f, 0f);
            labelRt.anchorMax = new Vector2(1f, 0f);
            labelRt.pivot     = new Vector2(0.5f, 0f);
            labelRt.sizeDelta = new Vector2(0f, 18f);
            labelRt.anchoredPosition = new Vector2(0f, 6f);

            var t = labelRt.GetComponent<TextMeshProUGUI>();
            if (t == null) t = labelRt.gameObject.AddComponent<TextMeshProUGUI>();
            if (font != null) t.font = font;
            t.text = label;
            t.color = Ink;
            t.alignment = TextAlignmentOptions.Center;
            t.raycastTarget = false;
            t.enableWordWrapping = false;
            t.overflowMode = TextOverflowModes.Ellipsis;
            // 아이콘이 뜻을 지므로 글자는 보조다. 9~12pt 창이면 44.4px 칸에
            // 네 글자("시즌패스")까지 들어간다 — 20pt 시절에 세 개가 잘리던 자리다.
            t.enableAutoSizing = true;
            t.fontSize = 12f;
            t.fontSizeMin = 9f;
            t.fontSizeMax = 12f;

            // 아이콘이 먼저, 라벨이 나중에 그려지도록 순서를 고정한다.
            iconRt.SetSiblingIndex(0);
            labelRt.SetSiblingIndex(1);
        }

        static Sprite LoadHudSprite(string buttonName)
        {
            if (!IconByButton.TryGetValue(buttonName, out var iconName)) return null;
            return AssetDatabase.LoadAssetAtPath<Sprite>(
                $"Assets/Resources/Art/Icons/Hud/{iconName}.png");
        }

        static RectTransform FindOrCreate(RectTransform parent, string name)
        {
            var found = parent.Find(name) as RectTransform;
            if (found != null) return found;

            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.localScale = Vector3.one;
            return rt;
        }

        static RectTransform FindActionRow()
        {
            foreach (var rt in Resources.FindObjectsOfTypeAll<RectTransform>())
            {
                if (rt.name != "action-row") continue;
                if (!rt.gameObject.scene.IsValid()) continue;
                return rt;
            }
            return null;
        }
    }
}
