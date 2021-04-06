/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext
{
    public static class WindowsHelper
    {
        public const string MenuPath = "Window/Infinity Code/uContext/";

        public static void ShowInspector()
        {
            Event e = Event.current;
            Type windowType = EditorTypes.inspectorWindow;

            Vector2 size = Prefs.contextMenuWindowSize;
            Rect rect = new Rect(GUIUtility.GUIToScreenPoint(e.mousePosition) - size / 2, Vector2.zero);

            Rect windowRect = new Rect(rect.position, size);
            EditorWindow window = ScriptableObject.CreateInstance(windowType) as EditorWindow;
            window.Show();
            window.position = windowRect;
        }
    }
}