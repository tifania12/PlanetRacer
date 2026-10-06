using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GemRacer.EditorTools
{
    /// <summary>E-10: HUD 상태바에 정제 광물 칸(아이콘 + 숫자)을 더한다.
    ///
    /// `economy-v2.md` 4절 마지막 문단이 정한 규칙 — **HUD 위 줄은 원석 · 정제 광물 · 젬 셋만**이고,
    /// 돈은 레이스·연구 화면에서만, 강화석은 강화·던전 화면에서만 낸다. 그 셋 중에서 정제 광물
    /// 칸만 없어서 이 메뉴가 채운다(원석은 GemRacer/24, 젬은 GemRacer/36이 이미 만들어 뒀다).
    ///
    /// BootstrapHudGem(GemRacer/36)과 같은 자리·같은 이유로 **mineral-group 안에** 넣는다.
    /// status-bar 바깥의 HorizontalLayoutGroup이 childForceExpandWidth=true라 자식 수대로 폭을
    /// 똑같이 나누기 때문에, status-bar에 자식을 더하면 A-24에서 실측해 둔 반반 배치가 흔들린다.
    ///
    /// **숫자 글씨를 셋 다 22로 맞춘다.** 오른쪽 칸 하나에 아이콘 셋 + 숫자 셋이 들어가야 해서
    /// 원석 숫자만 26으로 크면 자리가 모자란다. 그리고 MainHudUgui가 원석 숫자에서 "원석 "
    /// 접두사를 뗐다 — 아이콘이 뜻을 지므로(`economy-v2.md` 4절 "각 재화는 아이콘이 반드시 있다")
    /// 젬처럼 숫자만 낸다.
    ///
    /// 실제 그림은 여기서 안 넣는다 — 자리 표시자만 두고 MainHudUgui.Awake()가
    /// UiKit.SetIcon("refined-icon", "icon-refined-mineral")으로 런타임에 입힌다.
    /// 여러 번 눌러도 결과가 같다(멱등) — 이미 있으면 지우고 다시 만든다.
    ///
    /// 끝에서 여섯 칸의 순서를 다시 세운다. GemRacer/36이 젬을 2·3번에 꽂도록 쓰여 있어서,
    /// 그 메뉴를 나중에 다시 누르면 순서가 엉킬 수 있다 — 그래서 이 메뉴는 자기 칸만 만들지 않고
    /// 있는 칸 전부를 원석·정제·젬 순으로 다시 줄 세운다(없는 칸은 건너뛴다).</summary>
    public static class BootstrapHudRefined
    {
        [MenuItem("GemRacer/42. HUD 상태바에 정제 광물 칸 추가 (E-10, 안전 - HUD만 건드림)")]
        public static void AddRefinedToStatusBar()
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
            var oldIcon = group.Find("refined-icon");
            if (oldIcon != null) Object.DestroyImmediate(oldIcon.gameObject);
            var oldCount = group.Find("refined-count");
            if (oldCount != null) Object.DestroyImmediate(oldCount.gameObject);

            // 아이콘 - mineral-icon·gem-icon과 같은 22x22 자리 표시자.
            var icon = NewRect("refined-icon", group);
            var img = icon.gameObject.AddComponent<Image>();
            img.color = Color.white;
            img.preserveAspect = true;
            img.raycastTarget = false;
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            var iconLayout = icon.gameObject.AddComponent<LayoutElement>();
            iconLayout.minWidth = 22f;  iconLayout.preferredWidth = 22f;  iconLayout.flexibleWidth = 0f;
            iconLayout.minHeight = 22f; iconLayout.preferredHeight = 22f; iconLayout.flexibleHeight = 0f;

            // 숫자 - 폰트·색은 원석 숫자에서 그대로 가져온다(셋이 따로 놀면 안 된다).
            var refinedRt = NewRect("refined-count", group);
            var refinedText = refinedRt.gameObject.AddComponent<TextMeshProUGUI>();
            refinedText.text = "0";
            refinedText.fontSize = 22f;
            refinedText.alignment = TextAlignmentOptions.MidlineLeft;
            refinedText.raycastTarget = false;
            refinedText.enableWordWrapping = false;
            if (mineralText != null)
            {
                refinedText.font = mineralText.font;
                refinedText.color = mineralText.color;
            }

            // 원석 숫자도 22로 낮춘다 - 아이콘 셋 + 숫자 셋이 한 칸에 들어가야 한다.
            if (mineralText != null) mineralText.fontSize = 22f;

            // 원석 · 정제 · 젬 순서로 다시 줄 세운다. 없는 칸은 건너뛴다.
            int index = 0;
            foreach (var name in new[] { "mineral-icon", "mineral-count",
                                         "refined-icon", "refined-count",
                                         "gem-icon", "gem-count" })
            {
                var child = group.Find(name);
                if (child != null) child.SetSiblingIndex(index++);
            }

            EditorUtility.SetDirty(groupGo);
            Debug.Log("[GemRacer] HUD 상태바 오른쪽 칸에 'refined-icon' + 'refined-count' 추가함. " +
                      "원석 숫자 글씨를 22로 맞췄고 여섯 칸을 원석·정제·젬 순으로 줄 세웠다. " +
                      "MainHudUgui.Awake()가 UiKit.SetIcon(\"refined-icon\", \"icon-refined-mineral\")으로 그림을 입힌다.");
        }

        static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }
    }
}
