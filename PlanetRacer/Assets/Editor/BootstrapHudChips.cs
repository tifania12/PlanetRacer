using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.UI;

namespace GemRacer.EditorTools
{
    /// <summary>A-28-2 마지막 화면(2026-10-16): HUD 상태바의 재화 셋을 **칩**으로 감싼다.
    ///
    /// 지금 상태바 오른쪽 칸(`mineral-group`)에는 아이콘 셋과 숫자 셋이 그냥 나란히 서 있다.
    /// 키트의 `ui-chip`은 왼쪽에 **아이콘이 들어앉는 동그란 홈**이 파인 알약이라, 쌍마다 칩 하나로
    /// 감싸 주면 그 홈에 재화 아이콘이 앉고 오른쪽에 숫자가 온다. 감싸는 일은 계층을 바꾸는
    /// 일이라 런타임 함수(`UiSkin`)가 아니라 부트스트랩이 할 몫이다 — 제목 리본을 아직 못 붙이는
    /// 이유와 같다.
    ///
    /// **`GemRacer/13`(HUD 세우기)을 다시 누르지 않는다.** 그 메뉴는 캔버스를 통째로 다시 만들어
    /// 배선된 화면 열아홉과 `MainHudUgui`의 패널 연결을 전부 날린다. 그래서 이 메뉴도
    /// `GemRacer/23`·`24`·`36`·`42`와 같은 애처블(additive) 메뉴로, `mineral-group` 밑만 건드린다.
    ///
    /// **순서가 있다.** 아이콘·숫자 칸을 만드는 `GemRacer/24`(원석) → `36`(젬) → `42`(정제)를
    /// 먼저 누르고 이걸 누른다. 거꾸로 누르면(칩이 있는데 24·36·42를 다시 누르면) 그 메뉴들은
    /// 바로 밑 자식만 찾으므로 칩 안에 있는 칸을 못 보고 같은 이름을 하나 더 만든다 —
    /// 그때는 **이 메뉴를 한 번 더 누르면 된다.** 아래 `Unwrap`이 칩을 풀면서 같은 이름이 둘이면
    /// 나중 것만 남기고 정리한다.
    ///
    /// 여러 번 눌러도 결과가 같다(멱등).</summary>
    public static class BootstrapHudChips
    {
        /// <summary>칩 이름 → 그 안에 들어갈 (아이콘, 숫자) 쌍. 순서가 곧 화면 왼쪽부터의 차례다
        /// (`economy-v2.md` 4절: HUD 위 줄은 원석 · 정제 광물 · 젬 셋만).</summary>
        static readonly string[][] Chips =
        {
            new[] { "chip-raw",     "mineral-icon", "mineral-count" },
            new[] { "chip-refined", "refined-icon", "refined-count" },
            new[] { "chip-gem",     "gem-icon",     "gem-count"     },
        };

        [MenuItem("GemRacer/44. HUD 상태바 재화 칩 (A-28-2, 안전 - HUD만 건드림)")]
        public static void AddChipsToStatusBar()
        {
            var groupGo = GameObject.Find("UI Canvas/HUD/status-bar/mineral-group");
            if (groupGo == null)
            {
                Debug.LogError("[GemRacer] 'UI Canvas/HUD/status-bar/mineral-group'이 없다. " +
                               "'GemRacer/24. HUD 상태바에 원석 아이콘 추가'를 먼저 눌러라.");
                return;
            }
            var group = (RectTransform)groupGo.transform;

            Unwrap(group);

            // 바깥 칸(mineral-group)의 줄 설정. 칩은 제 높이를 지켜야 하므로 세로로 늘리지 않는다 —
            // `childForceExpandHeight`가 켜져 있으면 LayoutElement의 flexibleHeight=0을 덮어쓴다
            // (ugui-migration.md 3-2에 적힌 그대로다).
            var groupLayout = group.GetComponent<HorizontalLayoutGroup>();
            if (groupLayout != null)
            {
                groupLayout.childForceExpandHeight = false;
                groupLayout.childAlignment = TextAnchor.MiddleRight;
                groupLayout.spacing = 6f;
            }

            var made = 0;
            foreach (var spec in Chips)
            {
                var icon  = group.Find(spec[1]);
                var count = group.Find(spec[2]);
                if (icon == null && count == null) continue;   // 아직 안 만든 칸은 건너뛴다

                var chip = NewRect(spec[0], group);
                chip.SetSiblingIndex(icon != null ? icon.GetSiblingIndex() : count.GetSiblingIndex());

                // **그림은 칩 자신이 아니라 뒤에 깔린 자식이 쓴다.** 칩에 직접 Image를 붙이면
                // Image도 레이아웃 요소라서 `preferredWidth`를 같이 보고하는데, 9-slice일 때
                // 그 값이 "테두리 둘의 합"(ui-chip은 420)이다. 바깥 줄은 같은 순위 중 큰 값을
                // 쓰므로 칩 셋이 420씩을 부르고, 상태바가 그쪽에 폭을 몰아주면서 행성 이름이
                // 25px로 찌그러졌다(2026-10-16에 실제로 그랬다). 배경을 레이아웃에서 빼면
                // 칩 폭은 안쪽 줄(아이콘 + 숫자)이 재는 값 그대로가 된다.
                var bg = NewRect(spec[0] + "-bg", chip);
                bg.anchorMin = Vector2.zero;
                bg.anchorMax = Vector2.one;
                bg.offsetMin = Vector2.zero;
                bg.offsetMax = Vector2.zero;
                var img = bg.gameObject.AddComponent<Image>();
                img.raycastTarget = false;     // 숫자를 보여 주는 칸이라 클릭을 먹을 이유가 없다
                img.color = Color.white;       // 그림에 색이 들어 있다. 흰색이라야 안 물든다
                bg.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;

                var layout = chip.gameObject.AddComponent<HorizontalLayoutGroup>();
                layout.spacing = 4f;
                // 왼쪽을 좁게 준다 — 아이콘이 그림의 동그란 홈 위에 앉아야 한다.
                layout.padding = new RectOffset(6, 10, 3, 3);
                layout.childAlignment = TextAnchor.MiddleLeft;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;
                layout.childControlWidth = true;
                layout.childControlHeight = true;

                // 폭은 안 정한다(-1 그대로) — 안쪽 줄이 재는 값을 그대로 쓴다. 높이만 못 박고
                // flexible을 0으로 눌러 둔다(ugui-migration.md 3-2).
                var le = chip.gameObject.AddComponent<LayoutElement>();
                le.minHeight = UiSkin.ChipHeight; le.preferredHeight = UiSkin.ChipHeight; le.flexibleHeight = 0f;
                le.flexibleWidth = 0f;

                if (icon  != null) icon.SetParent(chip, false);
                if (count != null) count.SetParent(chip, false);
                made++;
            }

            // 원석 · 정제 · 젬 순으로 줄 세운다(GemRacer/42가 칸에 하던 일과 같다).
            var index = 0;
            foreach (var spec in Chips)
            {
                var chip = group.Find(spec[0]);
                if (chip != null) chip.SetSiblingIndex(index++);
            }

            // 그림을 얹는 일은 런타임과 같은 함수가 한다 — 한쪽만 고쳐서 어긋나는 일을 막는다
            // (HUD·강화·상점과 같은 방식). `MainHudUgui.Awake`도 같은 함수를 부른다.
            var hud = GameObject.Find("UI Canvas/HUD");
            var skinned = hud != null ? UiSkin.ApplyToHud(hud.transform) : 0;

            EditorUtility.SetDirty(groupGo);
            Debug.Log($"[GemRacer] HUD 상태바 재화 칩 {made}개 세움(chip-raw·chip-refined·chip-gem). " +
                      $"키트 스킨 {skinned}곳. 그림이 아직 없으면 조용히 넘어간다.");
        }

        /// <summary>이미 있는 칩을 풀어 아이콘·숫자를 바깥 칸으로 도로 꺼낸다. 멱등의 핵심이다.
        /// 같은 이름이 둘이면(칩이 있는 줄 모르고 `GemRacer/24`·`36`·`42`를 다시 누른 경우)
        /// **나중 것만 남긴다** — 그 메뉴들이 방금 만든 쪽이 폰트·순서가 맞춰진 새 칸이다.</summary>
        static void Unwrap(RectTransform group)
        {
            foreach (var spec in Chips)
            {
                var chip = group.Find(spec[0]);
                if (chip == null) continue;
                var at = chip.GetSiblingIndex();
                for (var i = chip.childCount - 1; i >= 0; i--)
                {
                    var child = chip.GetChild(i);
                    if (child.name == spec[0] + "-bg") continue;   // 배경은 칩과 함께 사라진다
                    child.SetParent(group, false);
                    child.SetSiblingIndex(at);
                }
                Object.DestroyImmediate(chip.gameObject);
            }

            foreach (var spec in Chips)
            {
                for (var k = 1; k <= 2; k++)
                {
                    var dupes = new System.Collections.Generic.List<Transform>();
                    foreach (Transform child in group)
                        if (child.name == spec[k]) dupes.Add(child);
                    for (var i = 0; i < dupes.Count - 1; i++)
                    {
                        Debug.LogWarning($"[GemRacer] '{spec[k]}'가 둘 이상이라 먼저 것을 지운다 " +
                                         "(칩이 있는 상태에서 GemRacer/24·36·42를 다시 누르면 생긴다).");
                        Object.DestroyImmediate(dupes[i].gameObject);
                    }
                }
            }
        }

        static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }
    }
}
