using UnityEngine;
using UnityEngine.UI;

namespace GemRacer.UI
{
    /// <summary>
    /// A-25(2026-09-29): 유니티 기본 회색 박스(`UI/Skin/UISprite.psd`)를 걷어내고
    /// `Resources/Art/UI/ui-*.png` 여섯 장을 9-slice로 얹는다.
    ///
    /// 왜 여기(런타임)에 두나 — 부트스트랩(Editor)이 씬에 굽고, 이 클래스가 런타임에도 한 번 더
    /// 물린다. A-24 아이콘과 같은 방식이다. 씬을 다시 저장하지 못한 상태로 빌드가 나가도
    /// 화면이 회색으로 돌아가지 않는다.
    ///
    /// **그림이 없으면 아무것도 하지 않는다**(art-wiring.md 1절). 회색 박스가 그대로 남을 뿐
    /// 화면이 죽지 않아야 한다.
    ///
    /// 9-slice 주의 — `pixelsPerUnitMultiplier`:
    /// 알약 버튼 스프라이트는 256x71로 들어오고 좌우 테두리(모서리 반지름)가 각 25px다.
    /// HUD 액션 줄의 한 칸은 44.4px뿐이라 그대로 얹으면 모서리 둘(50px)이 칸보다 넓어져
    /// 서로 겹친다. 그래서 작은 칸에는 배수를 올려 테두리를 UI 단위로 줄인다.
    /// </summary>
    public static class UiSkin
    {
        const string Root = "Art/UI/";

        public static Sprite Panel         => UiKit.LoadSpriteAtPath(Root + "ui-panel");
        public static Sprite Header        => UiKit.LoadSpriteAtPath(Root + "ui-header");
        public static Sprite GaugeFrame    => UiKit.LoadSpriteAtPath(Root + "ui-gauge-frame");
        public static Sprite ButtonFace    => UiKit.LoadSpriteAtPath(Root + "ui-button");
        public static Sprite ButtonPressed => UiKit.LoadSpriteAtPath(Root + "ui-button-pressed");
        public static Sprite Tab           => UiKit.LoadSpriteAtPath(Root + "ui-tab");

        /// <summary>Image 하나에 9-slice 스프라이트를 얹는다. 그림이 없으면 건드리지 않는다.
        /// 스프라이트 자체에 색이 들어 있으니 tint는 흰색으로 돌린다 —
        /// 부트스트랩이 넣어 둔 단색(Panel·BtnFace)이 그대로 남아 있으면 그림이 그 색에 물든다.</summary>
        public static bool ApplySliced(Image img, Sprite sprite, float pixelsPerUnitMultiplier = 0f)
        {
            if (img == null || sprite == null) return false;
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            img.fillCenter = true;
            img.pixelsPerUnitMultiplier = pixelsPerUnitMultiplier > 0.01f
                ? pixelsPerUnitMultiplier
                : SliceMultiplier(img, sprite);
            img.color = Color.white;
            return true;
        }

        /// <summary>9-slice 테두리가 화면에서 몇 단위가 될지 정하는 배수를 구한다.
        ///
        /// **2026-09-30에 강화 화면에서 크게 데었다.** uGUI는 테두리를 그림 픽셀 그대로 쓰지 않고
        /// `referencePixelsPerUnit / sprite.pixelsPerUnit` 만큼 키운다. 우리 UI 그림은 임포트에서
        /// 256으로 줄면서 `pixelsPerUnit`이 100이 아니라 **25 안팎**이 된다(1028 → 256이니 약 1/4).
        /// 그래서 배수를 1로 두면 `ui-panel`의 21.67px 테두리가 화면에서 **87단위**가 되고,
        /// 168 높이 카드에서 위아래 모서리가 156을 먹어 가운데가 12밖에 안 남는다 —
        /// 카드 옆선이 안쪽으로 휘고 버튼 알약 끝이 칸 밖으로 삐져나왔다.
        ///
        /// 그래서 두 가지를 한다.
        ///  1. `referencePixelsPerUnit / sprite.pixelsPerUnit`로 되돌려 **"그림의 픽셀 = UI 단위"**로 만든다.
        ///  2. 그러고도 모서리 둘이 칸보다 넓으면(HUD 액션 줄처럼 칸이 44px밖에 안 될 때)
        ///     칸의 90%에 맞춰 한 번 더 줄인다. 전에 HUD가 쓰던 손계산 배수를 이게 대신한다.
        ///
        /// 칸 크기를 아직 모를 때(레이아웃 전, rect가 0)는 1번만 하고 넘어간다.</summary>
        static float SliceMultiplier(Image img, Sprite sprite)
        {
            var canvas = img.canvas;
            var refPpu = canvas != null ? canvas.referencePixelsPerUnit : 100f;
            var spritePpu = Mathf.Max(0.01f, sprite.pixelsPerUnit);
            var k = refPpu / spritePpu;          // 테두리가 부풀려지는 비율
            var mult = k;                        // 1번: 그림 픽셀 그대로

            var rect = img.rectTransform.rect;
            var b = sprite.border;               // x=왼 y=아래 z=오른 w=위
            var hor = b.x + b.z;
            var ver = b.y + b.w;
            if (rect.width  > 1f && hor > 0.01f) mult = Mathf.Max(mult, hor * k / (rect.width  * 0.9f));
            if (rect.height > 1f && ver > 0.01f) mult = Mathf.Max(mult, ver * k / (rect.height * 0.9f));

            return Mathf.Max(0.01f, mult);
        }

        /// <summary>버튼 한 칸에 알약 스킨을 얹는다.
        ///
        /// **전환(transition)은 ColorTint 그대로 둔다.** SpriteSwap으로 `ui-button-pressed`를
        /// 물려 보려다 되돌렸다 — SpriteSwap은 색을 아예 안 쓰기 때문에, 아직 안 만든 화면 때문에
        /// `interactable = false`로 꺼 둔 버튼(HUD 열 칸 중 여럿)이 **켜진 것과 똑같이 보인다.**
        /// 꺼져 있다는 표시가 MainHudUgui가 일부러 주는 신호라 그걸 잃는 쪽이 손해가 크다.
        /// 눌린 그림(`ui-button-pressed`)은 「눌리는 동안만 스프라이트를 바꾸고 꺼진 칸은
        /// 어둡게 칠하는」 작은 스크립트가 생기면 그때 붙인다 — backlog A-25에 적어 둔다.</summary>
        public static bool ApplyButton(Button btn, float pixelsPerUnitMultiplier = 0f)
        {
            if (btn == null) return false;
            var img = btn.targetGraphic as Image ?? btn.GetComponent<Image>();
            if (!ApplySliced(img, ButtonFace, pixelsPerUnitMultiplier)) return false;
            btn.targetGraphic = img;
            return true;
        }

        /// <summary>HUD에 스킨을 입힌다. 부트스트랩(`GemRacer/37`)과 `MainHudUgui.Awake`가
        /// **같은 함수**를 부른다 — 한쪽만 고쳐서 어긋나는 일을 막는다.
        ///
        /// 얹는 자리는 셋뿐이다.
        ///  - `status-bar` 배경 → `ui-panel` (508x64, 테두리가 넉넉히 들어간다)
        ///  - 액션 줄 버튼 열 칸 → `ui-button`
        ///  - `cargo-gauge-track`은 **일부러 건드리지 않는다.** 높이가 10px뿐이라
        ///    `ui-gauge-frame`(테두리 위아래 각 58px 기준)을 얹으면 틀이 칸을 넘는다.
        ///    그 그림은 강화 화면의 큰 게이지가 쓸 자리다.</summary>
        public static int ApplyToHud(Transform hud)
        {
            if (hud == null) return 0;
            var done = 0;

            var bar = UiKit.Find<Image>(hud, "status-bar", false);
            if (ApplySliced(bar, Panel))
            {
                done++;
                // 스킨의 좌우 끝이 비스듬히 깎여 있어서, 안쪽 여백 16px로는 글자가 그 빗변에
                // 닿는다(젬 숫자가 오른쪽 모서리에 걸쳤다 — 09-29 21시 스크린샷). 여백을 24로 넓힌다.
                // 회색 박스일 때 잰 A-24의 칸 너비는 271.2px였고, 여백을 8px 더 줘도
                // 원석 999999.9 + 젬 99999가 그대로 들어간다.
                var layout = bar.GetComponent<HorizontalLayoutGroup>();
                if (layout != null && layout.padding.left < 24)
                {
                    layout.padding.left = 24;
                    layout.padding.right = 24;
                }
            }

            var row = UiKit.FindObject(hud, "action-row", false);
            if (row != null)
            {
                var buttons = row.GetComponentsInChildren<Button>(true);
                // 칸이 좁다(열 칸이면 44.4px). 모서리 둘이 칸 너비를 넘지 않게 줄이는 일은
                // 이제 `SliceMultiplier`가 칸 크기를 직접 보고 한다 — 09-29에 여기서 손으로 계산하던
                // 배수(`50 / (slot*0.45)`)는 그림의 pixelsPerUnit을 몰라서 어림잡은 값이었다.
                foreach (var b in buttons)
                    if (ApplyButton(b)) done++;
            }

            return done;
        }

        // 강화 화면의 카드 넷과 버튼 다섯. 이름은 BootstrapUpgradeUgui가 짓는 그대로다.
        static readonly string[] UpgradeRows = { "refinery", "tool", "cargo", "engine" };

        /// <summary>강화 화면에 스킨을 입힌다. 부트스트랩(`GemRacer/38`)과 `UpgradeUgui.Awake`가
        /// **같은 함수**를 부른다 — HUD와 같은 방식이다.
        ///
        /// 얹는 자리는 셋이다.
        ///  - 화폐 줄(`currency-line`) → `ui-header`. 좌우 끝만 둥근 띠라 양옆 여백을 14로 준다.
        ///    안 그러면 원석 아이콘이 왼쪽 둥근 부분에 올라탄다.
        ///  - 강화 줄 카드 넷(`row-*`) → `ui-panel`. 칸이 330x168쯤이라 테두리(22/19)가 넉넉히 들어간다.
        ///  - 버튼 다섯(강화 넷 + 닫기) → `ui-button`. HUD와 달리 칸이 넓어서(300px 안팎)
        ///    `pixelsPerUnitMultiplier`를 올릴 필요가 없다 — 좌우 모서리 합쳐 50px면 충분히 남는다.
        ///
        /// **바깥 배경(root의 Image)은 일부러 안 건드린다.** 화면을 꽉 채우는 가림막이라
        /// 둥근 패널을 얹으면 모서리가 화면 밖으로 잘려 나가고, 뒤로 클릭이 새지 않게 막는
        /// 역할만 하면 된다.
        ///
        /// `ui-gauge-frame`은 이 화면에 쓸 자리가 없다 — 강화 화면에 게이지가 없다.
        /// 그 그림은 게이지가 있는 화면을 배선하는 세션 몫이다(backlog A-25에 적어 둔다).</summary>
        public static int ApplyToUpgrade(Transform root)
        {
            if (root == null) return 0;
            var done = 0;

            var line = UiKit.FindObject(root, "currency-line", false);
            if (line != null)
            {
                var img = line.GetComponent<Image>();
                if (img == null)
                {
                    img = line.AddComponent<Image>();
                    img.raycastTarget = false; // 글자 줄이라 클릭을 먹을 이유가 없다
                }
                if (ApplySliced(img, Header))
                {
                    done++;
                    var lay = line.GetComponent<HorizontalLayoutGroup>();
                    if (lay != null && lay.padding.left < 14)
                    {
                        lay.padding.left = 14;
                        lay.padding.right = 14;
                    }
                }
            }

            foreach (var prefix in UpgradeRows)
            {
                if (ApplySliced(UiKit.Find<Image>(root, "row-" + prefix, false), Panel)) done++;
                if (ApplyButton(UiKit.Find<Button>(root, prefix + "-button", false))) done++;
            }

            if (ApplyButton(UiKit.Find<Button>(root, "close-button", false))) done++;

            return done;
        }

        // 상점 화면의 아홉 줄. 이름은 BootstrapShopUgui가 짓는 그대로이고 ShopUgui.Prefixes와 순서도 같다.
        static readonly string[] ShopRows =
        {
            "starter", "cargo1", "cargo2", "cargo3", "offlinecap", "accel", "season", "steam", "adremoval",
        };

        /// <summary>상점 화면에 스킨을 입힌다. 부트스트랩(`GemRacer/39`)과 `ShopUgui.Awake`가
        /// **같은 함수**를 부른다 — HUD·강화 화면과 같은 방식이다.
        ///
        /// 얹는 자리는 셋이다.
        ///  - 줄 카드 아홉(`row-*`) → `ui-panel`. 칸이 400x140이고 가로에서는 두 칸으로 재배치되며
        ///    330 안팎이 된다(ResponsiveGridCells) — 어느 쪽이든 테두리(22/19)가 넉넉히 들어간다.
        ///  - 줄마다 구매 버튼 아홉(`*-button`) → `ui-button`. 줄 안쪽 여백 14를 빼고 372x44쯤이라
        ///    강화 화면 버튼과 같은 크기다.
        ///  - 닫기 버튼(`close-button`) → `ui-button`. 스크롤 바깥이라 항상 보인다.
        ///
        /// **일부러 안 건드리는 것 둘.**
        ///  - 바깥 배경(root의 Image)은 화면을 꽉 채우는 가림막이다. 강화 화면과 같은 이유로 그대로 둔다 —
        ///    둥근 패널을 얹으면 모서리가 화면 밖으로 잘려 나가고, 뒤로 클릭이 새지 않게 막는 일만 하면 된다.
        ///  - `scroll-view`의 Image는 `Mask`가 잘라내는 모양으로 쓰는 그림이다. 여기에 9-slice 패널을
        ///    얹으면 목록이 둥근 모서리 모양대로 잘려 나간다. 목록을 감싸는 틀이 필요해지면
        ///    `scroll-view` 바깥에 배경 노드를 하나 더 두는 쪽이 맞다.
        ///
        /// `ui-gauge-frame`은 이 화면에도 자리가 없다 — 상점에 게이지가 없다. 그 그림은 여전히
        /// 게이지가 있는 화면(레이스 출전·연구소 등)을 배선하는 세션 몫이다.</summary>
        public static int ApplyToShop(Transform root)
        {
            if (root == null) return 0;
            var done = 0;

            foreach (var prefix in ShopRows)
            {
                if (ApplySliced(UiKit.Find<Image>(root, "row-" + prefix, false), Panel)) done++;
                if (ApplyButton(UiKit.Find<Button>(root, prefix + "-button", false))) done++;
            }

            if (ApplyButton(UiKit.Find<Button>(root, "close-button", false))) done++;

            return done;
        }
    }
}
