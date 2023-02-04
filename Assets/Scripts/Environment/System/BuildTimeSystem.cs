using Environment.Data;
using Environment.Interface;
using UnityEngine;

namespace Environment.System
{
    public class BuildTimeSystem : ISharedSystem
    {
        private TimePrefab m_data;
        private float m_timeProgression = 0f;
        
        public BuildTimeSystem()
        { 
            m_data = Resources.Load<TimePrefab>("Time");
        }

        public void Init()
        {
            m_timeProgression = m_data.dayCycleLength > 0f ? 0.4f / m_data.dayCycleLength : 0f;
            m_data.time = 12f;
        }

        public void Refresh()
        {
            if (Application.isPlaying)
            {
                m_data.time += m_timeProgression * Time.deltaTime;
                if (m_data.time >= 24f)
                {
                    m_data.time = 0f;
                }
            }
        }
    }
}