using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GemRacer.UI
{
    /// <summary>
    /// A-25(2026-10-08): 눌린 그림(`ui-button-pressed`)을 붙이면서도 **꺼진 칸은 꺼져 보이게** 남긴다.
    ///
    /// 왜 Button의 SpriteSwap을 안 쓰나 — `UiSkin.ApplyButton`의 주석에 적힌 그대로다.
    /// SpriteSwap으로 바꾸면 uGUI가 색을 아예 안 쓰기 때문에, `interactable = false`로 꺼 둔
    /// 버튼(HUD 열 칸 중 여럿이 그렇다)이 켜진 것과 똑같이 보인다. 꺼져 있다는 표시는
    /// `MainHudUgui`가 일부러 주는 신호라 그걸 잃는 쪽이 손해가 크다.
    ///
    /// 그래서 전환은 **ColorTint 그대로 두고**(꺼진 칸은 계속 흐려진다) 눌리는 동안만
    /// 이 스크립트가 스프라이트를 갈아 끼운다. 색은 건드리지 않는다 — 누르는 동안 Button이
    /// `pressedColor`로 크로스페이드하는 중이라, 여기서 색을 흰색으로 돌리면 서로 싸운다.
    ///
    /// 되돌릴 때는 **누르기 직전의 스프라이트를 그대로** 복원한다. `ui-button`을 다시 집어 오지
    /// 않는 이유는, 스킨이 아직 안 입혀진 회색 박스 버튼에 이 스크립트가 붙어도
    /// 엉뚱한 그림으로 바뀌지 않게 하기 위해서다(art-wiring.md 1절 — 그림이 없으면 아무것도 안 한다).
    /// </summary>
    [RequireComponent(typeof(Image))]
    public sealed class UiPressSprite : MonoBehaviour,
        IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        Image _img;
        Button _btn;

        bool _down;
        Sprite _prevSprite;
        float _prevMultiplier;

        void Awake()
        {
            _img = GetComponent<Image>();
            _btn = GetComponent<Button>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_down) return;
            if (_img == null) return;
            if (_btn != null && !_btn.interactable) return;   // 꺼진 칸은 눌린 그림도 안 쓴다

            var pressed = UiSkin.ButtonPressed;
            if (pressed == null) return;                      // 그림이 없으면 아무것도 하지 않는다
            if (_img.sprite == pressed) return;

            _prevSprite = _img.sprite;
            _prevMultiplier = _img.pixelsPerUnitMultiplier;
            _down = true;

            UiSkin.SwapSpriteKeepTint(_img, pressed);
        }

        public void OnPointerUp(PointerEventData eventData) => Restore();

        public void OnPointerExit(PointerEventData eventData) => Restore();

        // 누른 채로 패널이 닫히거나(설정 화면처럼) 버튼이 꺼지면 눌린 그림으로 굳어 버린다.
        void OnDisable() => Restore();

        void Restore()
        {
            if (!_down) return;
            _down = false;
            if (_img == null) return;

            _img.sprite = _prevSprite;
            _img.pixelsPerUnitMultiplier = _prevMultiplier;
        }
    }
}
