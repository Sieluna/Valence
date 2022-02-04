using UnityEngine;

namespace Environment.Data
{
    [CreateAssetMenu(fileName = "New Time", menuName = "Time Prefab")]
    public class TimePrefab : ScriptableObject
    {
        [Range(0f, 24f)] public float time;

        [Range(0f, 100f)] public float dayCycleLength = 5f;
    }
}