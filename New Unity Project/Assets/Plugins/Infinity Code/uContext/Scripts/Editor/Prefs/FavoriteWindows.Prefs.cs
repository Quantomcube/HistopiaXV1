/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using System.Linq;
using InfinityCode.uContext.Windows;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace InfinityCode.uContext
{
    public static partial class Prefs
    {
        public static bool favoriteWindowsInContextMenu = true;

        public class FavoriteWindowsManager : StandalonePrefManager<FavoriteWindowsManager>
        {
            private ReorderableList reorderableList;
            private bool waitWindowChanged;
            private Vector2 scrollPositon;

            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Favorite Windows In Context Menu",
                    };
                }
            }

            private void AddItem(ReorderableList list)
            {
                waitWindowChanged = true;
                EditorApplication.update -= WaitWindowChanged;
                EditorApplication.update += WaitWindowChanged;
            }

            public override void Draw()
            {
                if (reorderableList == null)
                {
                    reorderableList = new ReorderableList(ReferenceManager.favoriteWindows, typeof(FavoriteWindowItem), true, true, true, true);
                    reorderableList.drawHeaderCallback += DrawHeader;
                    reorderableList.drawElementCallback += DrawElement;
                    reorderableList.onAddCallback += AddItem;
                    reorderableList.onRemoveCallback += RemoveItem;
                    reorderableList.onReorderCallback += Reorder;
                    reorderableList.elementHeight = 48;
                }

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

                favoriteWindowsInContextMenu = EditorGUILayout.ToggleLeft("Favorite Windows In Context Menu", favoriteWindowsInContextMenu);
                reorderableList.DoLayoutList();

                EditorGUILayout.EndScrollView();

                if (waitWindowChanged)
                {
                    EditorGUILayout.HelpBox("Set the focus on the window you want to add to the favorites.", MessageType.Info);

                    if (GUILayout.Button("Stop Pick"))
                    {
                        waitWindowChanged = false;
                        EditorApplication.update -= WaitWindowChanged;
                    }
                }
            }

            private void DrawElement(Rect rect, int index, bool isactive, bool isfocused)
            {
                FavoriteWindowItem item = ReferenceManager.favoriteWindows[index];

                EditorGUI.BeginChangeCheck();
                Rect r = new Rect(rect);
                float lineHeight = rect.height / 2;
                r.height = lineHeight - 2;
                item.title = EditorGUI.TextField(r, "Title", item.title);
                if (EditorGUI.EndChangeCheck()) ReferenceManager.Save();

                EditorGUI.BeginDisabledGroup(true);
                r.y += lineHeight;
                EditorGUI.TextField(r, "Class", item.className);
                EditorGUI.EndDisabledGroup();
            }

            private void DrawHeader(Rect rect)
            {
                GUI.Label(rect, "Windows");
            }

            private void RemoveItem(ReorderableList list)
            {
                ReferenceManager.favoriteWindows.RemoveAt(list.index);
                ReferenceManager.Save();
            }

            private void Reorder(ReorderableList list)
            {
                ReferenceManager.Save();
            }

            private void WaitWindowChanged()
            {
                EditorWindow wnd = EditorWindow.focusedWindow;

                if (wnd == null) return;
                if (wnd.GetType() == EditorTypes.projectSettingsWindow) return;

                EditorApplication.update -= WaitWindowChanged;
                string className = wnd.GetType().AssemblyQualifiedName;
                if (ReferenceManager.favoriteWindows.All(r => r.className != className))
                {
                    ReferenceManager.favoriteWindows.Add(new FavoriteWindowItem(wnd));
                    ReferenceManager.Save();
                }

                waitWindowChanged = false;
                settingsWindow.Repaint();
            }
        }
    }
}