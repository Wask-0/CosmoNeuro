using System;
using UnityEngine;

namespace _Project.Develop.Runtime.Utilities.FoundationUnity.Lifecycle
{
    public class MonoDestroyable : MonoBehaviour
    {
        public event Action<MonoDestroyable> Destroyed;

        public bool IsDestroyed { get; private set; }
        private bool _notified;

        public void Destroy()
        {
            if (IsDestroyed)
                return;
            
            IsDestroyed = true;
            
            Destroy(gameObject);
        }

        public void OnDestroy()
        {
            IsDestroyed = true;
            
            if (_notified) 
                return;
            
            _notified = true;
            
            Destroyed?.Invoke(this);
            
            Destroyed = null;
        }
    }
}
