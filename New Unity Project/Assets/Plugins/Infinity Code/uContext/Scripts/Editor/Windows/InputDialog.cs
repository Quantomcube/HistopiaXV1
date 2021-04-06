/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext.Windows
{
    public class InputDialog : EditorWindow
    {
        public Action<InputDialog> OnClose;
        public Action<InputDialog> OnDrawExtra;

        private bool focusControl = true;
        private Action<string> okCallback;
        private string text;

        private void InvokeOK()
        {
            try
            {
                okCallback(text);
            }
            catch (Exception e)
            {
                Log.Add(e);
            }

            Close();
        }

        private void OnDestroy()
        {
            if (OnClose != null)
            {
                OnClose(this);
                OnClose = null;
            }

            OnDrawExtra = null;
            okCallback = null;
        }

        private void OnGUI()
        {
            Event e = Event.current;
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
                {
                    InvokeOK();
                    e.Use();
                    return;
                }
            }

            GUI.SetNextControlName("inputTextField");
            text = EditorGUILayout.TextField(text);

            if (focusControl)
            {
                focusControl = false;
                GUI.FocusControl("inputTextField");
            }

            if (OnDrawExtra != null) OnDrawExtra(this);

            EditorGUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("OK", GUILayout.Width(70))) InvokeOK();


            if (GUILayout.Button("Cancel", GUILayout.Width(70))) Close();

            EditorGUILayout.EndHorizontal();
        }

        public static InputDialog Show(string title, string text, Action<string> okCallback)
        {
            Resolution r = Screen.currentResolution;
            Vector2 size = new Vector2(300, 30);

            InputDialog window = CreateInstance<InputDialog>();
            window.titleContent = new GUIContent(title);
            window.text = text;
            window.okCallback = okCallback;
            window.ShowUtility();
            window.position = new Rect(new Vector2((r.width - size.x) / 2, (r.height - size.y) / 2), size);
            return window;
        }
    }
}