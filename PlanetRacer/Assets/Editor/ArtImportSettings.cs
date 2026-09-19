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
            return 512;                                      // Pets·Icons — 목록에서 작게 쓴다
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
            importer.userData = "art-import-v1";                 // 다시 적용했는지 표시
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
