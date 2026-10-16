using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GemRacer.UI
{
    /// <summary>
    /// A-26(2026-10-17): 타이틀 화면. 지금까지는 켜면 바로 채굴 화면이 떴다.
    ///
    /// 구조는 화물칸 가득·오프라인 보상 화면과 같다 — HUD 버튼으로 여닫는 화면이 아니라
    /// 스스로 떴다 사라지는 화면이라, 루트 "Title"은 항상 켜 두고(꺼 두면 Update가 안 돈다)
    /// 자식 "title-veil"만 이 스크립트가 여닫는다. 그래서 UiPanel을 달지 않는다.
    ///
    /// 겸하는 일이 둘이다.
    /// 1. 게임 이름을 한 번 보여 준다. 이름은 **폰트로** 얹는다(그림에 글자가 없다).
    /// 2. 뒤에서 세이브 로드·오프라인 보상 계산이 도는 동안 화면을 가린다 — 가림막이
    ///    먼저 떠 있으면 숫자가 0에서 튀어 오르는 게 안 보인다.
    ///
    /// 아무 곳이나 누르면 넘어간다(가림막 전체가 버튼 하나다). 넘어갈 때는 0.25초 동안
    /// 알파만 내린다 — 프레임 수가 아니라 시간에 비례하게 깎고(CLAUDE.md 5번),
    /// `?fast=20` 배속에 휩쓸리지 않게 unscaled 시간을 쓴다.
    ///
    /// 아트 확인 화면(`?art=1`)일 때는 아예 뜨지 않는다. 그림을 보러 들어온 주소인데
    /// 타이틀이 위에 얹히면 한 번 더 눌러야 하고, 그 화면은 UI를 확인하는 자리가 아니다.
    /// </summary>
    public sealed class TitleUgui : MonoBehaviour
    {
        [Tooltip("넘어갈 때 알파가 0이 되기까지 걸리는 시간(초). 0이면 즉시 사라진다.")]
        public float fadeSeconds = 0.25f;

        [Tooltip("앱을 떠나 있다 돌아왔을 때 타이틀을 다시 띄울 기준(초). " +
                 "0이면 첫 실행에만 뜬다. 잠깐 화면을 바꿨다 돌아온 정도로는 안 뜨게 두려고 기본 60초다.")]
        public float reshowAfterAwaySeconds = 60f;

        GameObject _veil;
        CanvasGroup _group;
        Button _tapButton;
        TMP_Text _hint;

        bool _fading;
        float _awaySince = -1f;   // 떠난 시각(unscaled). -1이면 떠나 있지 않다

        void Awake()
        {
            _veil = UiKit.FindObject(transform, "title-veil");
            if (_veil == null) return;

            _group     = _veil.GetComponent<CanvasGroup>();
            _tapButton = _veil.GetComponent<Button>();
            _hint      = UiKit.Find<TMP_Text>(transform, "title-hint", false);

            _tapButton?.onClick.AddListener(Dismiss);

            // 그림은 씬에 이미 구워져 있지만 한 번 더 물린다 — 씬을 다시 저장하지 못한
            // 빌드에서도 키아트가 빠지지 않게(MainHudUgui가 아이콘을 다시 물리는 것과 같은 이유).
            UiKit.SetSpriteAtPath(transform, "title-keyart", "Art/Title/title-keyart");
            UiKit.SetSpriteAtPath(transform, "title-emblem", "Art/Title/title-emblem");

            // 아트 확인 화면으로 들어온 주소면 타이틀은 건너뛴다.
            if (ArtViewer.WantsArtViewer(Application.absoluteURL))
            {
                _veil.SetActive(false);
                return;
            }

            Show();
        }

        /// <summary>타이틀을 다시 띄운다. 알파를 1로 되돌리고 가림막을 켠다.</summary>
        public void Show()
        {
            if (_veil == null) return;
            _fading = false;
            if (_group != null) _group.alpha = 1f;
            if (_hint != null) _hint.text = "아무 곳이나 누르세요";
            _veil.SetActive(true);
        }

        /// <summary>누르면 넘어간다. 두 번 눌려도 한 번만 먹는다.</summary>
        public void Dismiss()
        {
            if (_veil == null || !_veil.activeSelf || _fading) return;

            if (fadeSeconds <= 0f || _group == null)
            {
                _veil.SetActive(false);
                return;
            }
            _fading = true;
        }

        void Update()
        {
            if (!_fading || _group == null) return;

            // 시간에 비례해 깎는다. 배속(`?fast=`)이 걸려 있어도 체감이 같게 unscaled를 쓴다.
            _group.alpha -= Time.unscaledDeltaTime / Mathf.Max(0.01f, fadeSeconds);
            if (_group.alpha > 0f) return;

            _group.alpha = 0f;
            _fading = false;
            _veil.SetActive(false);
        }

        // --- 오프라인 복귀 ---------------------------------------------------
        // 모바일에서 앱을 내렸다 다시 열면 씬이 그대로 살아 있어 Awake가 다시 돌지 않는다.
        // 그래서 떠나 있던 시간을 재서, 오프라인 보상이 생길 만큼 길었으면 다시 가린다.
        // 웹에서는 pause가 안 오고 focus만 오는 경우가 있어 양쪽을 다 본다.

        void OnApplicationPause(bool paused) => MarkAway(paused);
        void OnApplicationFocus(bool hasFocus) => MarkAway(!hasFocus);

        void MarkAway(bool away)
        {
            if (reshowAfterAwaySeconds <= 0f) return;

            if (away)
            {
                if (_awaySince < 0f) _awaySince = Time.realtimeSinceStartup;
                return;
            }

            if (_awaySince < 0f) return;
            var elapsed = Time.realtimeSinceStartup - _awaySince;
            _awaySince = -1f;
            if (elapsed >= reshowAfterAwaySeconds) Show();
        }
    }
}
