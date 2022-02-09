using Unity.Mathematics;
using UnityEngine;

namespace Valence.Environment
{
    public enum CelestiumSimulationType
    {
        Simple,
        Realistic
    }

    [CreateAssetMenu(fileName = nameof(CelestiumComponent), menuName = "Time/Create Celestium Component")]
    public class CelestiumComponent : ScriptableObject
    {
        public CelestiumSimulationType SimulationType;

        public float3 SunLocalDirection = float3.zero;

        public float3 MoonLocalDirection = float3.zero;
    }
}