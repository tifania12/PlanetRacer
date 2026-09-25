using System;

namespace GemRacer.Core
{
    /// <summary>A-20: MiningRig.ToolLevel(1~30) → `Resources/Art/Rigs` 파일 경로 변환.
    /// Models.cs:41의 주석("곡괭이→드릴→레이저, 1~30, 10단계씩 티어")과 DefaultData.cs의
    /// 보물 등급 경계(RequiredToolLevel 1·11·21·26)가 같은 3티어 구획(1~10 / 11~20 / 21~30)을
    /// 가리키고 있어 그대로 가져다 썼다 — 새 경계를 만들지 않는다.
    ///
    /// `rig-tiers-sheet.png`(1536x1024, 세 대를 한 장에)를 셋으로 잘라
    /// `rig-tier-pickaxe`·`rig-tier-drill`·`rig-tier-laser` 세 파일로 나눠 뒀다(순서는 시트의
    /// 왼쪽부터 그대로). PetArt.cs와 같은 이유로 화면보다 먼저 코어에 둔다 — Unity 없이도
    /// Core.Tests로 경계값(1·10·11·20·21·30)을 검증할 수 있다.
    /// 실제 스프라이트 로드(Resources.Load)는 UI 쪽(UiKit.SetSpriteAtPath)이 이 경로 문자열을
    /// 받아서 한다.</summary>
    public static class RigArt
    {
        /// <summary>티어 인덱스(0~2)와 같은 순서.</summary>
        static readonly string[] TierFile = { "rig-tier-pickaxe", "rig-tier-drill", "rig-tier-laser" };

        const int LevelsPerTier = 10;

        /// <summary>ToolLevel(1~30, 범위 밖 값은 양 끝으로 자른다) → 티어 인덱스(0~2).
        /// 1~10=0(곡괭이), 11~20=1(드릴), 21~30=2(레이저).</summary>
        public static int TierIndex(int toolLevel)
        {
            var clamped = Math.Max(1, Math.Min(30, toolLevel));
            var index = (clamped - 1) / LevelsPerTier;
            return Math.Min(TierFile.Length - 1, index);
        }

        /// <summary>`Resources.Load&lt;Sprite&gt;`에 그대로 넘길 수 있는 경로(확장자 없음),
        /// 예: "Art/Rigs/rig-tier-drill". 파일이 실제로 있는지는 확인하지 않는다 — 없으면
        /// 호출부가 null을 돌려주니 조용히 자리 표시자로 넘어간다(2026-09-15 원칙).</summary>
        public static string ResourcePath(int toolLevel) => $"Art/Rigs/{TierFile[TierIndex(toolLevel)]}";
    }
}
