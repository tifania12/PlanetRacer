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

        /// <summary>A-25: UI 스킨 6장만 다시 임포트한다. 메뉴 90은 181장 전부를 훑어서 오래 걸리고,
        /// 사람이 인스펙터에서 손본 값까지 되돌린다 — 스킨만 고칠 때는 이쪽을 쓴다.</summary>
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
