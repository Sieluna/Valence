using Unity.Mathematics;
using UnityEngine;

namespace Valence.Environment.Time
{
    public enum CelestiumSimulationType
    {
        Simple,
        Realistic
    }

    [CreateAssetMenu(menuName = "Time/Create Celestium Component", fileName = "CelestiumComponent")]
    public class CelestiumComponent : ScriptableObject
    {
        public CelestiumSimulationType SimulationType;

        public float3 SunLocalDirection = float3.zero;

        public float3 MoonLocalDirection = float3.zero;
    }
}