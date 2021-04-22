/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext
{
    [InitializeOnLoad]
    public class KeyManager : BindingManager<KeyManager.KeyBinding>
    {
        

        static KeyManager()
        {
            GlobalEventManager.AddListener(OnGlobalEvent);
        }

        public static KeyBinding AddBinding()
        {
            return Add(new KeyBinding());
        }

        public static KeyBinding AddBinding(KeyCode keyCode, bool shift = false, bool control = false)
        {
            return Add(new KeyBinding(keyCode, shift, control));
        }

        private static void OnGlobalEvent()
        {
            if (Event.current.type == EventType.KeyDown)
            {
                for (int i = bindings.Count - 1; i >= 0; i--) bindings[i].TryInvoke();
            }
        }

        public static void RemoveBinding(KeyBinding keyBinding)
        {
            bindings.Remove(keyBinding);
            keyBinding.Dispose();
        }

        public class KeyBinding
        {
            public Action OnInvoke;
            public Func<bool> OnValidate;
            private KeyCode keyCode;
            private bool shift;
            private bool control;

            internal KeyBinding()
            {

            }

            internal KeyBinding(KeyCode keyCode, bool shift, bool control)
            {
                this.keyCode = keyCode;
                this.shift = shift;
                this.control = control;
            }

            public void Dispose()
            {
                OnInvoke = null;
            }

            public void Remove()
            {
                RemoveBinding(this);
            }

            public void TryInvoke()
            {
                Event e = Event.current;
                
                if (OnValidate != null)
                {
                    try
                    {
                        if (OnValidate())
                        {
                            if (OnInvoke != null) OnInvoke();
                        }

                    }
                    catch
                    {
                    }
                }
                else if (e.keyCode == keyCode && e.shift == shift && e.control == control)
                {
                    try
                    {
                        if (OnInvoke != null) OnInvoke();
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}