using UnityEngine;

namespace Valence.Environment
{
    public abstract class System : ScriptableObject
    {
        public virtual void OnSystemAwake() { }

        public virtual void OnSystemStart() { }

        public virtual void OnSystemUpdate() { }

        public virtual void OnSystemFixedUpdate() { }

        public virtual void OnSystemLateUpdate() { }

        public virtual void OnSystemEnable() { }

        public virtual void OnSystemDisable() { }

        public virtual void OnSystemDestroy() { }
    }
}
