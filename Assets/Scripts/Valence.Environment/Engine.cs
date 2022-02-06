using UnityEngine;

namespace Valence.Environment
{
    public class Engine : MonoBehaviour
    {
        [SerializeField] private System[] systems;

        private void Awake()
        {
            foreach (var system in systems)
                system.OnSystemAwake();
        }

        private void Start()
        {
            foreach (var system in systems)
                system.OnSystemStart();
        }

        private void Update()
        {
            foreach (var system in systems)
                system.OnSystemUpdate();
        }

        private void LateUpdate()
        {
            foreach (var system in systems)
                system.OnSystemLateUpdate();
        }

        private void FixedUpdate()
        {
            foreach (var system in systems)
                system.OnSystemFixedUpdate();
        }

        private void OnEnable()
        {
            foreach (var system in systems)
                system.OnSystemEnable();
        }

        private void OnDisable()
        {
            foreach (var system in systems)
                system.OnSystemDisable();
        }

        private void OnDestroy()
        {
            foreach (var system in systems)
                system.OnSystemDestroy();
        }
    }
}