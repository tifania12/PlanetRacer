using System;
using System.Collections.Generic;

namespace GemRacer.Core
{
    /// <summary>발견된 보물 하나. 발견은 자동이지만 캘지 말지는 플레이어가 고른다 — 여기서는
    /// "목록에 무엇이 있는지, 지금 캘 수 있는지"만 계산한다. 실제로 캐서 인벤토리에 넣는 것은
    /// 이 결과를 받는 쪽(Assets/Scripts, 나중에 서버)의 몫이다.</summary>
    public struct TreasureDiscovery
    {
        public string DefId;
        public TreasureGrade Grade;
        public int RequiredToolLevel;
        /// <summary>지금 채굴차 도구 레벨로 캘 수 있는지.</summary>
        public bool CanMineNow;
    }

    /// <summary>오프라인 동안의 결과 한 벌 — 광물(자동 산출)과 보물(발견 목록)을 같이 담는다.
    /// D07-N 오프라인 보상 화면이 이 구조체 하나만 받으면 되게 하려는 목적.</summary>
    public struct OfflineDiscoveries
    {
        public MiningSimulator.OfflineResult Mining;
        public List<TreasureDiscovery> Treasures;
    }

    /// <summary>
    /// 탐험 — 채굴차가 표면을 도는 동안 보물을 발견한다. 순수 함수, seed 하나로 재현 가능.
    /// 광맥은 흔해서 선택이 없으니(MiningSimulator가 시간당 산출을 그대로 계산) 여기서 다루지 않는다.
    /// 여기서는 드문 보물만 — "발견은 자동, 선택은 수동"(docs/design/core-loop.md)의 자동 발견 부분.
    /// </summary>
    public static class ExplorationSimulator
    {
        /// <summary>주어진 시간(초) 동안 채굴차가 지나간 광맥 사이클 수(소수 포함).
        /// MiningSimulator와 같은 주기를 쓴다 — 채굴차가 표면을 도는 리듬이 곧 탐험의 리듬이다.</summary>
        public static float CyclesIn(MiningRig rig, Planet planet, double elapsedSeconds)
        {
            var speed = MiningSimulator.RigSpeed(rig, planet);
            var travelPerVein = planet.Circumference / Math.Max(1, planet.VeinCount);
            var secondsPerCycle = travelPerVein / speed + MiningSimulator.SecondsPerVein(rig);
            return (float)Math.Max(0, elapsedSeconds) / secondsPerCycle;
        }

        /// <summary>
        /// 경과 시간(온라인 틱이든 오프라인 누적이든) 동안의 보물 발견 목록을 만든다.
        /// 사이클마다 chancePerCycle 확률로 하나를 굴리고, 등급이 낮을수록(목록 앞쪽일수록) 잘 나오게
        /// 가중치를 준다. seed가 같으면 완전히 같은 목록 — 서버 재검증, 리플레이에 쓴다.
        /// </summary>
        public static List<TreasureDiscovery> Discover(MiningRig rig, Planet planet, double elapsedSeconds,
            IList<TreasureDef> possibleTreasures, int seed, float chancePerCycle = 0.05f)
        {
            var result = new List<TreasureDiscovery>();
            if (possibleTreasures.Count == 0) return result;

            var wholeCycles = (int)CyclesIn(rig, planet, elapsedSeconds);
            var rng = new DeterministicRandom(seed);

            for (var i = 0; i < wholeCycles; i++)
            {
                if (rng.NextFloat() >= chancePerCycle) continue;
                var def = PickWeighted(possibleTreasures, rng);
                result.Add(new TreasureDiscovery
                {
                    DefId = def.Id,
                    Grade = def.Grade,
                    RequiredToolLevel = def.RequiredToolLevel,
                    CanMineNow = CanMine(def, rig),
                });
            }
            return result;
        }

        /// <summary>이 보물을 지금 도구로 캘 수 있는지. 코어 어디서나(발견 시점, 나중에 인벤토리 화면에서) 같은 판정.</summary>
        public static bool CanMine(TreasureDef def, MiningRig rig) => rig.ToolLevel >= def.RequiredToolLevel;

        /// <summary>
        /// L-04: 오프라인 발견 목록. 자리를 비운 동안 광물(MiningSimulator.Offline)과
        /// 보물(Discover)을 같이 계산해서 "돌아왔을 때 보여줄 것" 한 벌로 묶는다.
        /// 탐험도 화물칸 상한(MiningSimulator.CargoHours)만큼만 인정한다 — 화물칸이 다 찬 뒤에는
        /// 채굴차가 멈춰 있는 셈이니 그 이후에 발견이 계속 쌓이면 앞뒤가 안 맞는다. 그래서 여기서는
        /// 원래 elapsedSeconds가 아니라 Offline이 이미 잘라 둔 HoursCounted를 그대로 쓴다.
        /// </summary>
        public static OfflineDiscoveries DiscoverOffline(MiningRig rig, Planet planet, double elapsedSeconds,
            IList<TreasureDef> possibleTreasures, int seed, float chancePerCycle = 0.05f)
        {
            var mining = MiningSimulator.Offline(rig, planet, elapsedSeconds);
            var cappedSeconds = (double)mining.HoursCounted * 3600.0;
            var treasures = Discover(rig, planet, cappedSeconds, possibleTreasures, seed, chancePerCycle);
            return new OfflineDiscoveries { Mining = mining, Treasures = treasures };
        }

        static TreasureDef PickWeighted(IList<TreasureDef> defs, DeterministicRandom rng)
        {
            // defs는 등급이 흔한 순(C→S)으로 온다는 전제. 앞쪽일수록 가중치를 크게 줘서 낮은 등급이 잘 나오게 한다.
            var totalWeight = 0f;
            for (var i = 0; i < defs.Count; i++) totalWeight += defs.Count - i;

            var roll = rng.NextFloat() * totalWeight;
            var acc = 0f;
            for (var i = 0; i < defs.Count; i++)
            {
                acc += defs.Count - i;
                if (roll < acc) return defs[i];
            }
            return defs[defs.Count - 1];
        }
    }
}
