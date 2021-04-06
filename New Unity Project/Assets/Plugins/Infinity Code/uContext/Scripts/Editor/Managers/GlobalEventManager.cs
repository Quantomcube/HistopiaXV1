/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEditor;

namespace InfinityCode.uContext
{
    [InitializeOnLoad]
    public class GlobalEventManager: BindingManager<GlobalEventManager.GlobalEvent>
    {
        static GlobalEventManager()
        {
            FieldInfo globalEventsField = typeof(EditorApplication).GetField("globalEventHandler", BindingFlags.Static | BindingFlags.NonPublic);
            EditorApplication.CallbackFunction value = (EditorApplication.CallbackFunction)globalEventsField.GetValue(null);
            value = EditorGlobalEvent + value;
            globalEventsField.SetValue(null, value);
        }

        private static void EditorGlobalEvent()
        {
            for (int i = bindings.Count - 1; i >= 0; i--) bindings[i].TryInvoke();
        }

        public static GlobalEvent AddListener(Action action)
        {
            return Add(new GlobalEvent(action));
        }

        public class GlobalEvent
        {
            private Action action;

            public GlobalEvent(Action action)
            {
                this.action = action;
            }

            public void TryInvoke()
            {
                try
                {
                    if (action != null) action();
                }
                catch (Exception e)
                {
                    Log.Add(e);
                }
            }
        }
    }
}