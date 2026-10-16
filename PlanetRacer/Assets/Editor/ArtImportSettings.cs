using UnityEditor;
using UnityEngine;

namespace GemRacer.EditorTools
{
    /// <summary>
    /// T-13(2026-09-19): `Assets/Resources/` 아래는 **참조 여부와 상관없이 전부 빌드에 들어간다.**
    /// 이미지 세션이 매일 밤 1.5~1.9MB짜리를 몇 장씩 올리면서 Resources/Art가 169MB까지 불었고,
    /// WebGL 빌드가 "25MiB 넘는 파일" 검사에서 #301부터 연달아 죽었다. 웹사이트가 옛 빌드에 멈췄다.
    ///
    /// 원본 PNG는 그대로 두고 **임포트 설정만** 바꾼다(A안). 그래야
    ///  - 저장소에는 원본이 남아 나중에 큰 해상도가 필요해지면 다시 쓸 수 있고
    ///  - 아트 확인 화면(`?art=1`, ArtViewer)이 Resources를 긁는 방식 그대로 돌아간다.
    ///    Tifania가 "폴더가 아니라 게임에서 보고 판단하겠다"고 한 그 경로다 — 이걸 깨면 안 된다.
    ///
    /// 그리고 이건 AssetPostprocessor라 **앞으로 들어오는 그림에도 자동으로 걸린다**(C안).
    /// 남은 펫이 아직 스무 장 넘게 있어서, 한 번 고치고 끝나는 방식으로는 또 터진다.
    ///
    /// 사람이 인스펙터에서 손으로 바꾼 값은 존중한다 — 아래는 **처음 임포트될 때만** 적용된다
    /// (`assetImporter.importSettingsMissingId`가 아니라 기존 meta 유무로 판단).
    /// 기존 파일에 적용하려면 메뉴 `GemRacer/90. 아트 임포트 설정 다시 적용`을 쓴다.
    /// </summary>
    public sealed class ArtImportSettings : AssetPostprocessor
    {
        const string Root = "Assets/Resources/Art/";

        /// <summary>폴더별 최대 해상도. 화면에서 얼마나 크게 보이는지로 정했다.
        /// 펫·아이콘은 목록에서 작게 보이고, 컷신은 전체 화면이라 더 필요하다.</summary>
        public static int MaxSizeFor(string path)
        {
            if (path.Contains("/Cutscenes/")) return 1024;  // 전체 화면 16:9
            if (path.Contains("/Planets/")) return 1024;    // 행성 구체는 꽤 크게 나온다
            if (path.Contains("/Rigs/")) return 1024;
            // 2026-09-28 이미지 묶음: 타이틀 배경(세로 전체 화면)·레이스 하늘(코스 뒤 전체 폭)은 컷신처럼 크게 깔린다.
            // 512로 두면 뿌옇다. 트로피·아치·속도선도 같은 폴더라 같이 1024가 되지만 장수가 적어(5장) 용량 영향은 작다.
            if (path.Contains("/Title/")) return 1024;
            if (path.Contains("/Race/")) return 1024;
            // A-25(2026-09-29): UI 스킨 6장은 9-slice로 늘려 쓰는 조각이라 원본 해상도가 필요 없다.
            // 상태바(508x64)·버튼(44x80)에 얹으면 256이면 충분하고, 테두리도 이 비율에서 같이 줄어든다.
            if (path.Contains("/UI/")) return 256;
            return 512;                                      // Pets·Icons — 목록에서 작게 쓴다
        }

        /// <summary>A-25: UI 스킨의 9-slice 테두리(left, bottom, right, top). **원본 픽셀 기준**이고
        /// maxTextureSize로 줄면 Unity가 비율에 맞춰 같이 줄인다.
        ///
        /// 값은 눈대중이 아니라 `tools/measure_ui_border.py`로 그림에서 쟀다 — 각 열·행이
        /// "가운데와 같아지는 첫 지점"을 찾는 방식이다. 알약형 버튼은 **위아래가 0**인 것이 맞다:
        /// 반원 끝이라 세로로 평평한 구간이 아예 없어서, 좌우만 반높이(모서리 반지름)로 끊고
        /// 가운데를 가로로 늘린다. 위아래에 억지로 값을 넣으면 반원이 얇은 띠로 찌그러진다.
        ///
        /// 그림은 먼저 `tools/crop_ui_skin.py`로 여백을 잘라 둔 상태를 전제한다(1254 정사각
        /// 캔버스 가운데에 모양이 작게 들어 있어서, 안 자르면 모서리 칸이 투명 여백을 덮는다).</summary>
        public static Vector4? SpriteBorderFor(string path)
        {
            var p = path.Replace('\\', '/');
            if (!p.Contains("/Art/UI/")) return null;
            var name = System.IO.Path.GetFileNameWithoutExtension(p);
            switch (name)
            {
                case "ui-panel":          return new Vector4(87f, 78f, 87f, 78f);   // 둥근 사각 패널
                case "ui-header":         return new Vector4(114f, 2f, 114f, 2f);   // 좌우 끝만 둥근 띠
                case "ui-gauge-frame":    return new Vector4(63f, 58f, 63f, 58f);
                case "ui-button":         return new Vector4(107f, 0f, 107f, 0f);   // 알약 — 위아래 0
                case "ui-button-pressed": return new Vector4(109f, 0f, 109f, 0f);
                // ui-tab은 위쪽 모서리만 둥근 모양이라 자동 측정이 좌우로 크게 흔들렸다(224/498).
                // 탭을 쓰는 화면이 아직 없으니 임시값으로 두고, 그 화면을 배선하는 세션이 다시 잰다.
                case "ui-tab":            return new Vector4(120f, 4f, 120f, 4f);

                // --- A-28-2 UI 키트 20장 (2026-10-12 Unity 배선 세션) ---------------
                // 키트도 1254 정사각 캔버스 가운데에 모양이 작게 들어 있었다. 그래서 먼저
                // `python tools/crop_ui_skin.py <Kit 폴더> --write`로 여백을 자르고
                // `tools/measure_ui_border.py`로 쟀다(아래 값은 자른 뒤 픽셀 기준).
                //
                // **잰 값을 그대로 쓰지 않은 것 둘** — 둘 다 알약이라 위아래가 0이 맞다.
                // 측정은 위/아래도 180 안팎을 주지만(높이 391 중 362), 알약은 세로로 평평한
                // 구간이 거의 없어서 그 값을 넣으면 반원이 얇은 띠로 찌그러진다. 기존
                // `ui-button`을 (107,0,107,0)으로 둔 것과 같은 이유다.
                case "ui-btn-primary":         return new Vector4(197f, 0f, 194f, 0f);
                case "ui-btn-primary-pressed": return new Vector4(147f, 0f, 147f, 0f);

                case "ui-popup":          return new Vector4(98f, 86f, 98f, 87f);   // 큰 창 — 사방이 고르게 나왔다
                case "ui-row":            return new Vector4(45f, 47f, 44f, 46f);   // 목록 한 줄
                case "ui-slot":           return new Vector4(100f, 101f, 102f, 101f);
                case "ui-slot-c":         return new Vector4(67f, 69f, 67f, 69f);
                case "ui-bar-frame":      return new Vector4(80f, 63f, 81f, 63f);
                case "ui-toast":          return new Vector4(120f, 51f, 119f, 51f);

                // ui-chip은 **눈으로 보고 정했다**(2026-10-16 Unity 배선 세션, A-28-2 마지막 화면).
                // 1088x289 알약인데 왼쪽에 아이콘이 들어앉는 동그란 홈이 파여 있다. 자동 측정이
                // 못 믿을 값을 준 게 이 홈 때문이고, 홈은 늘어나면 안 되니 왼쪽 테두리를 홈 바깥까지
                // (홈 오른쪽 끝 약 255px) 넉넉히 잡는다. 오른쪽은 둥근 끝(반지름 144.5)만 덮으면 된다.
                // 위아래가 0인 것은 ui-button·ui-btn-primary와 같은 이유다 — 알약은 세로로 평평한
                // 구간이 없어서 값을 넣으면 반원이 띠로 찌그러진다.
                case "ui-chip":           return new Vector4(270f, 0f, 150f, 0f);

                // ui-popup-title도 **눈으로 보고 정했다**(2026-10-16 07시 Unity 배선 세션).
                // 1224x293 띠인데 좌우 끝이 보석 모양으로 뾰족하게 깎여 있고, 위아래 가운데에
                // 장식 턱이 하나씩 튀어나와 있다. 자동 측정은 그 가운데 턱에서 멈춘다.
                // 알파를 열별로 훑으니 띠가 **x=125부터 x=1100까지** 높이가 일정하다(위 28 · 아래 264) —
                // 그 바깥이 곧 좌우 보석 끝이라 테두리를 거기에 맞춘다.
                // 위아래가 0인 것은 ui-button·ui-chip과 같은 이유다. 이 그림은 `UiSkin`이
                // **세로를 칸 높이에 맞추는 배수**(ui-chip과 같은 길)로 얹으므로, 테두리 125px은
                // 46 높이 리본에서 약 19.6단위가 된다 — 보석 끝이 찌그러지지 않는다.
                case "ui-popup-title":    return new Vector4(125f, 0f, 124f, 0f);

                // **일부러 테두리를 안 넣는 것들.** 9-slice로 늘릴 그림이 아니다 —
                // `ui-btn-close`(X 단추)·`ui-badge-new`·`ui-dot`은 비율을 지켜 통째로 쓰고
                // `ui-bar-fill`은 채우기(Filled)로 쓴다. 테두리를 넣으면 모양이 늘어난다.
                //
                // --- 남아 있던 여섯 장 (2026-10-16 19시 Unity 배선 세션) ----------------
                // 「측정이 못 믿을 값을 준 것들」로 미뤄 뒀던 마지막 묶음이다. 07시 세션이
                // `ui-popup-title`에 쓴 방법(알파를 열·행으로 훑어 띠가 일정해지는 구간을 찾는다)을
                // `tools/scan_ui_border.py`로 옮겨서 쟀고, 그 위에 그림을 실제로 눈으로 보고 정했다.
                // measure_ui_border.py가 틀렸던 이유는 가운데 장식이 아니라 **테두리가 소프트 글로우**라
                // 바깥으로 번지기 때문이다 — 번진 쪽에서 판정이 멈춰 좌우 합이 그림 크기를 넘었다.
                //
                // 넷은 둥근 사각이라 사방을 같은 값으로 둔다(기존 `ui-slot-c`가 804px에 67 =
                // 8.3%인 것과 같은 비율이다). 둘은 가운데 장식 때문에 한쪽만 크다.
                case "ui-slot-a":         return new Vector4(95f, 95f, 95f, 95f);    // 파란 테 둥근 사각
                case "ui-slot-b":         return new Vector4(88f, 88f, 88f, 88f);    // 청록 테, 모양은 a와 같다

                // ui-slot-s는 **위쪽 가운데에 보석 장식**이 하나 솟아 있다(y=0~80, x=473~612).
                // 위 테두리를 190으로 잡아 보석과 위 두 귀를 통째로 넣는다 — 그래도 보석이
                // 「위-가운데」 칸에 들어가니 칸이 가로로 길어지면 보석도 같이 늘어난다.
                // 슬롯은 거의 정사각으로 쓰니 지금은 문제가 안 되지만, 가로로 긴 자리에 쓸 거면
                // 보석을 따로 얹는 편이 낫다.
                case "ui-slot-s":         return new Vector4(96f, 108f, 97f, 190f);

                // ui-tooltip은 **아래 가운데에 말풍선 꼬리**가 달려 있다(몸통은 y=817까지, 꼬리가 923까지).
                // 아래 테두리 204는 몸통 아래 모서리 + 꼬리를 함께 덮는 값이다. 보석과 같은 사정이라
                // 말풍선이 가로로 넓어지면 꼬리도 넓어진다 — 꼬리 위치를 고정해야 하는 화면은
                // 꼬리를 떼어 따로 얹는다.
                case "ui-tooltip":        return new Vector4(106f, 204f, 104f, 111f);

                // 탭 둘은 **위 모서리만 둥글고 아래는 직선으로 잘려 있다.** 그래서 아래는 0이다
                // (기존 `ui-tab` 임시값 (120,4,120,4)을 이 둘이 대신한다 — 탭을 쓰는 화면이
                // 생기면 `ui-tab`이 아니라 on/off 두 장을 쓴다).
                case "ui-tab-on":         return new Vector4(160f, 0f, 153f, 151f);
                case "ui-tab-off":        return new Vector4(153f, 0f, 148f, 157f);
                default:                  return null;
            }
        }

        void OnPreprocessTexture()
        {
            if (!assetPath.Replace('\\', '/').StartsWith(Root)) return;
            var importer = (TextureImporter)assetImporter;
            // 이미 .meta가 있으면(= 사람이 한 번이라도 만졌을 수 있으면) 건드리지 않는다.
            if (!string.IsNullOrEmpty(importer.userData)) return;
            Apply(importer, assetPath);
        }

        /// <summary>실제로 값을 넣는 곳. 메뉴에서도 이걸 부른다.</summary>
        public static void Apply(TextureImporter importer, string path)
        {
            importer.textureType = TextureImporterType.Sprite;   // UI에 얹어 쓰는 그림들이다
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;                      // 2D UI라 밉맵이 필요 없다(용량 +33%)
            importer.maxTextureSize = MaxSizeFor(path);
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.crunchedCompression = true;                 // 다운로드 용량이 크게 준다
            importer.compressionQuality = 50;

            // A-25: UI 스킨만 다르게 간다. 9-slice 테두리를 넣고, crunch는 끈다 —
            // 테두리가 1~2px짜리 선이라 crunch 뭉개짐이 화면에서 바로 보인다(6장이라 용량 영향도 작다).
            var border = SpriteBorderFor(path);
            if (border.HasValue)
            {
                importer.spriteBorder = border.Value;
                importer.crunchedCompression = false;
                importer.compressionQuality = 100;
                importer.filterMode = FilterMode.Bilinear;
            }

            importer.userData = "art-import-v1";                 // 다시 적용했는지 표시
        }

        /// <summary>A-25: UI 스킨만 다시 임포트한다(`Assets/Resources/Art/UI` 아래 전부 —
        /// A-25의 6장 + A-28-1 키트 20장이 `Kit/` 안에 있다). 메뉴 90은 200장 전부를 훑어서
        /// 오래 걸리고 사람이 인스펙터에서 손본 값까지 되돌린다 — 스킨만 고칠 때는 이쪽을 쓴다.
        ///
        /// **그림 파일을 자른 뒤에는 꼭 이걸 눌러야 한다.** `OnPreprocessTexture`는 `userData`가
        /// 채워진 그림(= 한 번이라도 임포트된 것)을 건드리지 않아서, 파일만 바뀌면 테두리가
        /// 새로 안 들어간다.</summary>
        [MenuItem("GemRacer/91. UI 스킨 임포트 설정 (9-slice 테두리)")]
        public static void ReapplyUiSkin()
        {
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Resources/Art/UI" });
            var changed = 0;
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;
                Apply(importer, path);
                importer.SaveAndReimport();
                changed++;
            }
            AssetDatabase.Refresh();
            Debug.Log($"[GemRacer] UI 스킨 임포트 다시 적용: {changed}장 (9-slice 테두리 + crunch 끔).");
        }

        [MenuItem("GemRacer/90. 아트 임포트 설정 다시 적용 (Resources/Art 전부)")]
        public static void ReapplyAll()
        {
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Resources/Art" });
            var changed = 0;
            try
            {
                AssetDatabase.StartAssetEditing();
                for (var i = 0; i < guids.Length; i++)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    EditorUtility.DisplayProgressBar("아트 임포트 설정", path, (float)i / guids.Length);
                    var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer == null) continue;
                    Apply(importer, path);
                    importer.SaveAndReimport();
                    changed++;
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                EditorUtility.ClearProgressBar();
            }
            AssetDatabase.Refresh();
            Debug.Log($"[GemRacer] 아트 임포트 설정 다시 적용: {changed}장. " +
                      "원본 PNG는 안 건드렸다 — .meta만 바뀐다.");
        }
    }
}
