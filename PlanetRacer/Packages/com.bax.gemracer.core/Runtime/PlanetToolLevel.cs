using System;
using System.Collections.Generic;

namespace GemRacer.Core
{
    /// <summary>P-06(행성별 곡괭이, planet-progression.md 2절 (나)안): 행성별로 나뉘어 보관되는
    /// 곡괭이(MiningRig.ToolLevel) 값과, 새 행성으로 넘어갈 때 이전 행성 레벨의 일부를 시작값으로
    /// 물려주는 계산. 지금 실제로 채굴 중인 MiningRig.ToolLevel은 "지금 있는 행성"의 값 하나뿐이고
    /// (SaveData.cs ToolLevelPlanetIds/Values 필드 주석 참고), 이 파일은 그 값을 행성이 바뀔 때마다
    /// 여기 저장했다가 되돌려 놓는 메커니즘만 미리 만들어 둔 것이다(PlanetMineralBank.cs가 먼저
    /// 만들어지고 나중에 붙은 것과 같은 순서) — 실제로 "행성을 옮기면 이 값을 읽고 쓴다"는 배선
    /// (SaveData.Rig.ToolLevel과 이 창고를 잇는 자리)은 P-09(행성 이동 화면)가 붙을 때 Unity
    /// 세션 몫으로 남는다.</summary>
    public static class PlanetToolLevel
    {
        /// <summary>기본 물려주기 비율 — "이전 최고 레벨의 40%"(planet-progression.md 2절).</summary>
        public const double DefaultCarryOverFraction = 0.4;

        /// <summary>그 행성에 저장된 곡괭이 레벨. 한 번도 저장한 적 없으면 1
        /// (MiningRig.ToolLevel의 기본값과 같다 — 아직 안 가 본 행성은 1레벨부터라는 뜻).</summary>
        public static int Level(List<string> planetIds, List<int> levels, string planetId)
        {
            var idx = planetIds.IndexOf(planetId);
            return idx < 0 ? 1 : levels[idx];
        }

        /// <summary>그 행성의 레벨을 정확히 이 값으로 저장한다(누적이 아니라 대입 — 곡괭이 레벨은
        /// 업그레이드로 1씩 오르거나 행성을 떠날 때 마지막 값을 그대로 남기는 것이라 더하는 개념이
        /// 아니다). 처음 저장하는 planetId면 새 칸을 만든다. 1 미만은 1로 올려 잡는다(레벨은
        /// 0이나 음수가 될 수 없다).</summary>
        public static void Set(List<string> planetIds, List<int> levels, string planetId, int level)
        {
            var clamped = Math.Max(1, level);
            var idx = planetIds.IndexOf(planetId);
            if (idx < 0) { planetIds.Add(planetId); levels.Add(clamped); }
            else levels[idx] = clamped;
        }

        /// <summary>새 행성에 처음 도착했을 때 곡괭이 시작 레벨. 이전 행성 레벨의 fraction만큼
        /// 물려받되(내림), 최소 1레벨은 보장한다 — "옮길 때마다 1레벨로 돌아가면 좌절이 된다"는
        /// 설계 취지 그대로(0이거나 음수인 previousLevel이 들어와도 1을 돌려준다).</summary>
        public static int CarryOverStartLevel(int previousLevel, double fraction = DefaultCarryOverFraction)
        {
            if (previousLevel <= 0) return 1;
            return Math.Max(1, (int)Math.Floor(previousLevel * fraction));
        }
    }
}
