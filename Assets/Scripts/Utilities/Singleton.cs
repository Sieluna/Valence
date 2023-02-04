using UnityEngine;

namespace Utilities
{
    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = (T) FindObjectOfType(typeof(T));
                    if (instance == null) 
                        instance = new GameObject(typeof(T).Name).AddComponent<T>();
                    DontDestroyOnLoad(instance);
                    return instance;
                }

                return instance;
            }
        }
    }

    public class ScriptableObjectSingleton<T> : ScriptableObject where T : ScriptableObject
    {
        protected static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                    instance = CreateInstance<T>();

                return instance;
            }
        }
    }
}