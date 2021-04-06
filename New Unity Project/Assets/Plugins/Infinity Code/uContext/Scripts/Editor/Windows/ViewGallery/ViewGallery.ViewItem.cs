/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext.Windows
{
    public partial class ViewGallery
    {
        public abstract class ViewItem
        {
            public Texture2D texture;
            public abstract bool useInPreview { get; set; }
            public abstract bool allowPreview { get; }
            public abstract string name { get; }

            public abstract void PrepareMenu(GenericMenu menu);

            public bool Draw(Rect rect, float maxLabelWidth)
            {
                bool status = false;
                Event e = Event.current;
                Rect toggleRect = new Rect(rect.xMax - 20, rect.yMin + 4, 16, 16);
                if (rect.Contains(e.mousePosition))
                {
                    GUI.Box(new RectOffset(2, 2, 2, 2).Add(rect), string.Empty, selectedStyle);
                    if (!toggleRect.Contains(e.mousePosition)) ProcessEvents(e, ref status);
                }
                GUI.Box(rect, new GUIContent(texture), GUIStyle.none);
                GUI.Label(new Rect(rect.center.x - maxLabelWidth / 2, rect.yMax + 5, maxLabelWidth, 15), name, Styles.centeredLabel);
                if (allowPreview)
                {
                    EditorGUI.BeginChangeCheck();
                    bool v = GUI.Toggle(toggleRect, useInPreview, GUIContent.none);
                    if (EditorGUI.EndChangeCheck()) useInPreview = v;
                }

                return status;
            }

            public void ProcessEvents(Event e, ref bool status)
            {
                if (e.type == EventType.MouseDown)
                {
                    if (e.button == 0) status = true;
                    else if (e.button == 1) ShowContextMenu();
                }
            }

            private void ShowContextMenu()
            {
                GenericMenu menu = new GenericMenu();
                PrepareMenu(menu);
                menu.ShowAsContext();
            }
        }
    }
}