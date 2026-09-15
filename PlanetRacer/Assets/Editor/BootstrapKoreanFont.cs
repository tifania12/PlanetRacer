using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace GemRacer.EditorTools
{
    /// <summary>
    /// D08: 한글 폰트 에셋을 만든다.
    ///
    /// 왜 필요한가 — 유니티 기본 폰트에는 한글 글리프가 없다. 에디터에서는 시스템 폰트로
    /// 대체돼서 잘 보이지만 빌드에는 그 대체 경로가 없어서 한글이 통째로 빈칸으로 나온다.
    /// 2026-09-14에 웹 빌드에서 이게 그대로 드러났다.
    ///
    /// 왜 Dynamic인가 — 한글 음절은 11,172자다. 전부 미리 구워 아틀라스에 넣으면
    /// 용량이 감당이 안 된다. Dynamic으로 두면 실제로 화면에 나온 글자만 그때그때 구워서
    /// 아틀라스에 채운다. 대신 빌드에 원본 폰트 파일이 같이 들어간다(1.5MB).
    ///
    /// 폰트는 Pretendard(SIL OFL 1.1). 라이선스 전문은 Assets/Fonts/Pretendard-OFL.txt.
    /// OFL은 원문을 같이 배포할 것을 요구하므로 저 파일을 지우면 안 된다.
    /// </summary>
    public static class BootstrapKoreanFont
    {
        const string SourceFont = "Assets/Fonts/Pretendard-Regular.otf";
        const string OutputAsset = "Assets/Fonts/Pretendard-Regular SDF.asset";

        [MenuItem("GemRacer/11. 한글 폰트 에셋 만들기")]
        public static void Create()
        {
            var font = AssetDatabase.LoadAssetAtPath<Font>(SourceFont);
            if (font == null)
            {
                Debug.LogError($"[GemRacer] 원본 폰트를 못 찾았다: {SourceFont}");
                return;
            }

            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(OutputAsset);
            if (existing != null)
            {
                Debug.Log($"[GemRacer] 폰트 에셋이 이미 있다: {OutputAsset}. 다시 만들지 않는다.");
                Selection.activeObject = existing;
                return;
            }

            // 90pt 샘플링에 4px 패딩, SDF16. 아틀라스 1024x1024 한 장으로 시작하고
            // 모자라면 멀티 아틀라스로 늘어난다(enableMultiAtlasSupport = true).
            var fontAsset = TMP_FontAsset.CreateFontAsset(
                font,
                samplingPointSize: 90,
                atlasPadding: 4,
                renderMode: GlyphRenderMode.SDFAA,
                atlasWidth: 1024,
                atlasHeight: 1024,
                atlasPopulationMode: AtlasPopulationMode.Dynamic,
                enableMultiAtlasSupport: true);

            if (fontAsset == null)
            {
                Debug.LogError("[GemRacer] TMP 폰트 에셋 생성 실패.");
                return;
            }

            fontAsset.name = Path.GetFileNameWithoutExtension(OutputAsset);
            AssetDatabase.CreateAsset(fontAsset, OutputAsset);

            // 아틀라스 텍스처와 머티리얼을 서브에셋으로 붙인다. 이걸 안 하면
            // 에셋을 다시 열었을 때 참조가 끊어진다.
            if (fontAsset.atlasTextures != null)
            {
                foreach (var tex in fontAsset.atlasTextures)
                {
                    if (tex == null) continue;
                    tex.name = fontAsset.name + " Atlas";
                    AssetDatabase.AddObjectToAsset(tex, fontAsset);
                }
            }
            if (fontAsset.material != null)
            {
                fontAsset.material.name = fontAsset.name + " Material";
                AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
            }

            EditorUtility.SetDirty(fontAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[GemRacer] 한글 폰트 에셋 생성: {OutputAsset} " +
                      $"(Dynamic, 1024x1024, 소스 {font.name})");
            Selection.activeObject = fontAsset;
        }

        /// <summary>TMP의 기본 폰트를 위에서 만든 한글 폰트로 바꾼다.
        /// 새로 만드는 TextMeshPro 컴포넌트가 별도 지정 없이 한글을 찍게 된다.</summary>
        [MenuItem("GemRacer/12. TMP 기본 폰트를 한글 폰트로")]
        public static void SetAsTmpDefault()
        {
            var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(OutputAsset);
            if (fontAsset == null)
            {
                Debug.LogError($"[GemRacer] 먼저 '11. 한글 폰트 에셋 만들기'를 실행할 것. 없음: {OutputAsset}");
                return;
            }

            var settings = TMP_Settings.instance;
            if (settings == null)
            {
                Debug.LogError("[GemRacer] TMP_Settings가 없다. Window > TextMeshPro > Import TMP Essential Resources 먼저.");
                return;
            }

            var so = new SerializedObject(settings);
            var prop = so.FindProperty("m_defaultFontAsset");
            if (prop == null)
            {
                Debug.LogError("[GemRacer] TMP_Settings에서 m_defaultFontAsset을 못 찾았다. TMP 버전이 다를 수 있다.");
                return;
            }
            prop.objectReferenceValue = fontAsset;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();

            Debug.Log($"[GemRacer] TMP 기본 폰트를 {fontAsset.name}으로 바꿨다.");
        }
    }
}
