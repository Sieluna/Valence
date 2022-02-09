using UnityEngine;

namespace Valence.Environment
{
    public class Engine : MonoBehaviour
    {
        [SerializeField] private System[] m_Systems;

        private void Awake()
        {
            foreach (var system in m_Systems)
                system.OnSystemAwake();
        }

        private void Start()
        {
            foreach (var system in m_Systems)
                system.OnSystemStart();
        }

        private void Update()
        {
            foreach (var system in m_Systems)
                system.OnSystemUpdate();
        }

        private void LateUpdate()
        {
            foreach (var system in m_Systems)
                system.OnSystemLateUpdate();
        }

        private void FixedUpdate()
        {
            foreach (var system in m_Systems)
                system.OnSystemFixedUpdate();
        }

        private void OnEnable()
        {
            foreach (var system in m_Systems)
                system.OnSystemEnable();
        }

        private void OnDisable()
        {
            foreach (var system in m_Systems)
                system.OnSystemDisable();
        }

        private void OnDestroy()
        {
            foreach (var system in m_Systems)
                system.OnSystemDestroy();
        }
    }
}