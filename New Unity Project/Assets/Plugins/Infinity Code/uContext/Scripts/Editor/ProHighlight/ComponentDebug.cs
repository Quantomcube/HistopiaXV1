/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using InfinityCode.uContext.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext.ProHighlight
{
#if !UCONTEXT_PRO
    [InitializeOnLoad]
    public static class ComponentDebug
    {
        private static GUIContent debugContent;
        private static GUIContent debugOnContent;
        private static Dictionary<ComponentWindow, Record> records;

        static ComponentDebug()
        {
            ComponentWindow.OnDestroyWindow += OnDestroyWindow;
            ComponentWindow.OnDrawHeader += OnDrawHeader;
            ComponentWindow.OnDrawContent += OnDrawContent;
            ComponentWindow.OnValidateEditor += OnValidateEditor;

            records = new Dictionary<ComponentWindow, Record>();
        }

        private static void OnDestroyWindow(ComponentWindow window)
        {
            records.Remove(window);
        }

        private static bool OnDrawContent(ComponentWindow window)
        {
            if (!Prefs.proHighlight) return false;

            Record record;
            if (!records.TryGetValue(window, out record)) return false;
            if (!record.isDebug) return false;

            EditorGUILayout.LabelField("Debug mode is available in uContext Pro.", EditorStyles.wordWrappedLabel);
            if (GUILayout.Button("Open uContext Pro"))
            {
                Links.OpenPro();
            }

            return true;
        }

        private static void OnDrawHeader(ComponentWindow window, Rect position, float y)
        {
            if (!Prefs.proHighlight) return;

            if (debugContent == null) debugContent = new GUIContent(Icons.debug, "Debug (Requires uContext Pro)");
            if (debugOnContent == null) debugOnContent = new GUIContent(Icons.debugOn, "Debug (Requires uContext Pro)");

            Record record;
            if (!records.TryGetValue(window, out record))
            {
                records[window] = record = new Record();
            }

            if (GUI.Button(new Rect(position.width - 36, y, 16, 16), record.isDebug ? debugOnContent : debugContent, Styles.transparentButton))
            {
                ToggleDebugMode(window, record);
            }
        }

        private static bool OnValidateEditor(ComponentWindow window)
        {
            return records[window].isDebug;
        }

        private static void ToggleDebugMode(ComponentWindow window, Record record)
        {
            record.isDebug = !record.isDebug;
        }

        public class Record
        {
            public bool isDebug;
        }
    }
#endif
}