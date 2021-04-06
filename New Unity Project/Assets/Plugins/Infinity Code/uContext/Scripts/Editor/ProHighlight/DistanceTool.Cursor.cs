/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.uContext.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext.ProHighlight
{
#if !UCONTEXT_PRO
    [InitializeOnLoad]
    public static class DistanceToolCursor
    {
        private static bool lastPointUnderCursor;

        static DistanceToolCursor()
        {
            DistanceTool.OnUseCursorGUI += OnUseCursor;
        }

        private static void OnUseCursor(Vector3 prev, bool hasPrev, ref float distance)
        {
            if (!Prefs.proHighlight) return;

            EditorGUILayout.LabelField("Distance to cursor calculation is available in uContext Pro.", EditorStyles.wordWrappedLabel);
            if (GUILayout.Button("Open uContext Pro")) Links.OpenPro();
        }
    }
#endif
}