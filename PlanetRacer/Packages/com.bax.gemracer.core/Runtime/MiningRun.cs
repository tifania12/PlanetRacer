using System;

namespace GemRacer.Core
{
    /// <summary>실시간 채굴 한 틱의 단계. 이동 중엔 광물이 안 나오고, 광맥 앞에 멈춰 있는 동안만 나온다.</summary>
    public enum MiningPhase { Traveling, MiningVein }

    /// <summary>
    /// D04-N: MiningSimulator의 시간당 평균 공식(MineralsPerHour)을 프레임 단위로 풀어 쓴 것.
    /// "다음 광맥까지 이동(RigSpeed) → 광맥 앞에서 SecondsPerVein만큼 멈춰 YieldPerVein을 캔다"를
    /// 그대로 반복한다. 오래 돌리면 MineralsPerHour와 같은 평균이 나오지만(테스트로 확인),
    /// 여기서는 "지금 이동 중인지 채굴 중인지", "광맥 하나를 막 다 캔 순간"을 프레임 단위로 알 수 있어
    /// MiningController(Assets/Scripts)가 정지·재생 연출에 쓸 수 있다.
    ///
    /// 오프라인 보상은 여전히 MiningSimulator.Offline의 적분식을 쓴다 — 몇 시간을 프레임 단위로
    /// 흉내 낼 필요는 없어서 둘을 따로 둔다(Advance를 오프라인 경과 시간만큼 한 번에 불러도 결과는
    /// 맞지만, 그럴 땐 while 루프가 광맥 수만큼 도니 느리다).
    /// </summary>
    public sealed class MiningRunState
    {
        public MiningPhase Phase { get; private set; } = MiningPhase.Traveling;

        /// <summary>현재 단계가 끝나기까지 남은 시간(초).</summary>
        public float PhaseSecondsRemaining { get; private set; }

        /// <summary>이 상태를 만든 뒤로 누적된 원석 총량(정제 전).</summary>
        public float TotalRawMinerals { get; private set; }

        public MiningRunState(MiningRig rig, Planet planet)
        {
            PhaseSecondsRemaining = TravelSecondsPerVein(rig, planet);
        }

        /// <summary>deltaSeconds만큼 시간을 흘린다. 그 사이 새로 캔 원석량(이번 호출분만)을 돌려준다.
        /// 델타가 길면(느린 프레임, 일시정지 뒤 복귀 등) 광맥을 여러 개 지나칠 수도 있어 while로 처리한다 —
        /// 큰 델타를 한 번에 줘도, 작은 델타로 여러 번 나눠 줘도 누적 결과는 같다(테스트로 확인).</summary>
        public float Advance(MiningRig rig, Planet planet, float deltaSeconds)
        {
            var minedThisCall = 0f;
            var remaining = Math.Max(0f, deltaSeconds);

            while (remaining > 0f)
            {
                if (remaining < PhaseSecondsRemaining)
                {
                    PhaseSecondsRemaining -= remaining;
                    remaining = 0f;
                    continue;
                }

                remaining -= PhaseSecondsRemaining;
                if (Phase == MiningPhase.Traveling)
                {
                    Phase = MiningPhase.MiningVein;
                    PhaseSecondsRemaining = MiningSimulator.SecondsPerVein(rig);
                }
                else
                {
                    var yield = MiningSimulator.YieldPerVein(rig, planet);
                    minedThisCall += yield;
                    TotalRawMinerals += yield;
                    Phase = MiningPhase.Traveling;
                    PhaseSecondsRemaining = TravelSecondsPerVein(rig, planet);
                }
            }
            return minedThisCall;
        }

        static float TravelSecondsPerVein(MiningRig rig, Planet planet)
        {
            var speed = MiningSimulator.RigSpeed(rig, planet);
            var travelPerVein = planet.Circumference / Math.Max(1, planet.VeinCount);
            return travelPerVein / speed;
        }
    }
}
