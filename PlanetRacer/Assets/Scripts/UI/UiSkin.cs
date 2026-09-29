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
        public static bool ApplySliced(Image img, Sprite sprite, float pixelsPerUnitMultiplier = 1f)
        {
            if (img == null || sprite == null) return false;
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            img.fillCenter = true;
            img.pixelsPerUnitMultiplier = Mathf.Max(0.01f, pixelsPerUnitMultiplier);
            img.color = Color.white;
            return true;
        }

        /// <summary>버튼 한 칸에 알약 스킨을 얹는다.
        ///
        /// **전환(transition)은 ColorTint 그대로 둔다.** SpriteSwap으로 `ui-button-pressed`를
        /// 물려 보려다 되돌렸다 — SpriteSwap은 색을 아예 안 쓰기 때문에, 아직 안 만든 화면 때문에
        /// `interactable = false`로 꺼 둔 버튼(HUD 열 칸 중 여럿)이 **켜진 것과 똑같이 보인다.**
        /// 꺼져 있다는 표시가 MainHudUgui가 일부러 주는 신호라 그걸 잃는 쪽이 손해가 크다.
        /// 눌린 그림(`ui-button-pressed`)은 「눌리는 동안만 스프라이트를 바꾸고 꺼진 칸은
        /// 어둡게 칠하는」 작은 스크립트가 생기면 그때 붙인다 — backlog A-25에 적어 둔다.</summary>
        public static bool ApplyButton(Button btn, float pixelsPerUnitMultiplier = 1f)
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
                // 칸이 좁다(열 칸이면 44.4px). 모서리 둘이 칸 너비를 넘지 않게 배수를 올린다.
                var slot = row.GetComponent<RectTransform>().rect.width / Mathf.Max(1, buttons.Length);
                var mult = Mathf.Max(1f, 50f / Mathf.Max(8f, slot * 0.45f));
                foreach (var b in buttons)
                    if (ApplyButton(b, mult)) done++;
            }

            return done;
        }
    }
}
