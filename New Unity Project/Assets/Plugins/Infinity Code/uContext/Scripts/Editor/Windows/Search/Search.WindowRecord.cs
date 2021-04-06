/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext.Windows
{
    public partial class Search
    {
        internal class WindowRecord : Record
        {
            private static Texture2D windowIcon;

            protected override Texture2D assetPreview
            {
                get
                {
                    if (windowIcon == null) windowIcon = EditorIconContents.winBtnWinMax.image as Texture2D;
                    return windowIcon;
                }
            }

            public override Object target
            {
                get { return null; }
            }

            public override string tooltip
            {
                get
                {
                    return _tooltip;
                }
            }

            public override string type
            {
                get { return "window"; }
            }

            public WindowRecord(string submenu, string label)
            {
                _tooltip = submenu;
                _label = submenu;
                search = new[] { label };
            }

            protected override void ShowContextMenu(int index)
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Open Window"), false, () => SelectRecord(index, 1));
                menu.ShowAsContext();
            }

            public override void Select(int state)
            {
                EditorApplication.ExecuteMenuItem(tooltip);
            }

            public override float UpdateAccuracy(string pattern)
            {
                float v = base.UpdateAccuracy(pattern);
                v *= 1.01f;
                _accuracy = v;
                return v;
            }
        }
    }
}