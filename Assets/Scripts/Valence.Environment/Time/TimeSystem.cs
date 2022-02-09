using UnityEngine;

namespace Valence.Environment
{
    [CreateAssetMenu(fileName = nameof(TimeSystem), menuName = "Time/Create Time System")]
    public class TimeSystem : System
    {
        [SerializeField] private TimeComponent m_TimeData;
        private float m_TimeProgression = 0f;

        public override void OnSystemAwake()
        {
            m_TimeProgression = m_TimeData.DayLength > 0f
                ? 0.4f / m_TimeData.DayLength
                : 0f;
        }

        public override void OnSystemUpdate()
        {
            m_TimeData.Time += m_TimeProgression * UnityEngine.Time.deltaTime;
            if (m_TimeData.Time >= 24f) m_TimeData.Time = 0f;
        }
    }
}