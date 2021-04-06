/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext.Tools
{
    [InitializeOnLoad]
    public static class MaximizeGameView
    {
        static MaximizeGameView()
        {
            KeyManager.KeyBinding binding = KeyManager.AddBinding();
            binding.OnValidate += () => Prefs.improveMaximizeGameViewBehaviour;
            binding.OnInvoke += OnInvoke;
        }

        private static void OnInvoke()
        {
            Event e = Event.current;
            EditorWindow wnd = EditorWindow.focusedWindow;
            if (wnd != null && 
                ((wnd.GetType() == EditorTypes.gameView && e.keyCode == KeyCode.Space && e.modifiers == EventModifiers.Shift) || 
                 (e.keyCode == KeyCode.F11 && e.modifiers == EventModifiers.FunctionKey)))
            {
                wnd.maximized = !wnd.maximized;
                Event.current.Use();
            }
        }
    }
}