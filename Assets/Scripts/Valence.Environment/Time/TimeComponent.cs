using UnityEngine;

namespace Valence.Environment.Time
{
    [CreateAssetMenu(menuName = "Time/Create Time Component", fileName = "TimeComponent")]
    public class TimeComponent : ScriptableObject
    {
        [Range(0f, 24f)] public float Time;

        [Range(1970, 2100)] public int Year = 1970;

        [Range(1, 12)] public int Month = 1;

        [Range(1, 31)] public int Date = 1;

        public float Latitude;

        public float Longitude;

        public float Utc;

        [Range(0f, 100f)] public float DayLength = 5f;
    }
}