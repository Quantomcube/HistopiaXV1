/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext.Windows
{
    public partial class CreateBrowser
    {
        public abstract class Item : SearchableItem
        {
            protected GUIContent _content;
            private string[] _search;
            internal string label;
            internal FolderItem parent;

            internal GUIContent content
            {
                get
                {
                    if (_content == null) InitContent();
                    return _content;
                }
            }

            public virtual void Dispose()
            {
                _content = null;
                parent = null;
            }

            public virtual void Draw()
            {
                Rect r = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.label, GUILayout.Height(18));

                if (selectedItem == this) GUI.DrawTexture(r, Styles.selectedRowTexture);
                GUI.Box(r, content, EditorStyles.label);
                Event e = Event.current;
                if (e.type == EventType.MouseDown && r.Contains(e.mousePosition))
                {
                    if (e.button == 0) OnClick();
                    else
                    {
                        selectedItem = this;
                        instance.UpdateSelectedIndex();
                        instance.Repaint();
                    }
                    e.Use();
                }
            }

            public virtual void Filter(string pattern, List<Item> filteredItems)
            {
                if (UpdateAccuracy(pattern) > 0) filteredItems.Add(this);
            }

            protected override string[] GetSearchStrings()
            {
                if (_search == null) _search = new[] {label};
                return _search;
            }

            protected abstract void InitContent();

            public abstract void OnClick();
        }
    }
}