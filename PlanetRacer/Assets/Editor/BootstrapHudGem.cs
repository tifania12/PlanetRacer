using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GemRacer.EditorTools
{
    /// <summary>E-07: HUD 상태바에 젬 칸(아이콘 + 숫자)을 더한다.
    ///
    /// 왜 파일을 따로 뺐나 - BootstrapHudUgui는 313줄이라 더 키우고 싶지 않았고,
    /// BootstrapHudIcons(A-24)가 이미 "HUD를 덧칠하는 메뉴는 따로 둔다"는 선례를 만들어 뒀다.
    ///
    /// **status-bar에 자식을 그냥 하나 더 붙이면 안 된다.** 바깥 HorizontalLayoutGroup이
    /// childForceExpandWidth=true라 자식 수대로 폭을 똑같이 나눈다 - 지금 둘(planet-name /
    /// mineral-group)이라 반반인데 셋이 되면 1/3씩으로 쪼개져 A-24에서 실측해 둔 배치가 흔들린다.
    /// 그래서 젬 칸은 **mineral-group 안에** 넣는다(GemRacer/24가 만든 오른쪽 칸).
    /// 바깥에서 보면 여전히 자식 둘이라 반반은 그대로고, 오른쪽 칸 안에서 원석과 젬이 나란히 선다.
    ///
    /// 실제 그림은 여기서 안 넣는다 - 자리 표시자만 두고 MainHudUgui.Awake()가
    /// UiKit.SetIcon("gem-icon", "icon-gem")으로 런타임에 입힌다(art-wiring.md 1절과 같은 방식).
    /// 여러 번 눌러도 결과가 같다(멱등) - 이미 있으면 지우고 다시 만든다.</summary>
    public static class BootstrapHudGem
    {
        [MenuItem("GemRacer/36. HUD 상태바에 젬 칸 추가 (E-07, 안전 - HUD만 건드림)")]
        public static void AddGemToStatusBar()
        {
            var groupGo = GameObject.Find("UI Canvas/HUD/status-bar/mineral-group");
            if (groupGo == null)
            {
                Debug.LogError("[GemRacer] 'UI Canvas/HUD/status-bar/mineral-group'이 없다. " +
                               "'GemRacer/24. HUD 상태바에 원석 아이콘 추가'를 먼저 눌러라.");
                return;
            }
            var group = (RectTransform)groupGo.transform;

            var countGo = group.Find("mineral-count");
            if (countGo == null)
            {
                Debug.LogError("[GemRacer] 'mineral-group' 안에 'mineral-count'가 없다. 씬이 예상과 달라 멈춘다.");
                return;
            }
            var mineralText = countGo.GetComponent<TMP_Text>();

            // 멱등 - 이미 있으면 지우고 처음부터 다시 만든다.
            var oldIcon = group.Find("gem-icon");
            if (oldIcon != null) Object.DestroyImmediate(oldIcon.gameObject);
            var oldCount = group.Find("gem-count");
            if (oldCount != null) Object.DestroyImmediate(oldCount.gameObject);

            // 아이콘 - mineral-icon과 같은 22x22 자리 표시자.
            var icon = NewRect("gem-icon", group);
            var img = icon.gameObject.AddComponent<Image>();
            img.color = Color.white;
            img.preserveAspect = true;
            img.raycastTarget = false;
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            var iconLayout = icon.gameObject.AddComponent<LayoutElement>();
            iconLayout.minWidth = 22f;  iconLayout.preferredWidth = 22f;  iconLayout.flexibleWidth = 0f;
            iconLayout.minHeight = 22f; iconLayout.preferredHeight = 22f; iconLayout.flexibleHeight = 0f;

            // 숫자 - 폰트·색은 원석 숫자에서 그대로 가져온다(둘이 따로 놀면 안 된다).
            // 크기만 26 -> 22로 줄였다. 오른쪽 칸(270px)에 아이콘 둘 + 숫자 둘이 들어가야 해서
            // 원석 숫자가 커질 때(1234.5) 자리가 빠듯하다. 실측은 daily에 적어 둔다.
            var gemRt = NewRect("gem-count", group);
            var gemText = gemRt.gameObject.AddComponent<TextMeshProUGUI>();
            gemText.text = "0";
            gemText.fontSize = 22f;
            gemText.alignment = TextAlignmentOptions.MidlineLeft;
            gemText.raycastTarget = false;
            gemText.enableWordWrapping = false;
            if (mineralText != null)
            {
                gemText.font = mineralText.font;
                gemText.color = mineralText.color;
            }

            // 순서: 원석 아이콘, 원석 숫자, 젬 아이콘, 젬 숫자
            icon.SetSiblingIndex(2);
            gemRt.SetSiblingIndex(3);

            EditorUtility.SetDirty(groupGo);
            Debug.Log("[GemRacer] HUD 상태바 오른쪽 칸에 'gem-icon' + 'gem-count' 추가함. " +
                      "MainHudUgui.Awake()가 UiKit.SetIcon(\"gem-icon\", \"icon-gem\")으로 그림을 입힌다.");
        }

        static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }
    }
}
