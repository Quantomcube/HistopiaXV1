/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext
{
    [InitializeOnLoad]
    public static class WindowManager
    {
        public static Action<EditorWindow> OnWindowFocused;

        private static EditorWindow focusedWindow;

        static WindowManager()
        {
            EditorApplication.update += Update;
            focusedWindow = EditorWindow.focusedWindow;
        }

        private static void Update()
        {
            if (focusedWindow != EditorWindow.focusedWindow)
            {
                focusedWindow = EditorWindow.focusedWindow;
                if (OnWindowFocused != null) OnWindowFocused(focusedWindow);
            }
        }
    }
}