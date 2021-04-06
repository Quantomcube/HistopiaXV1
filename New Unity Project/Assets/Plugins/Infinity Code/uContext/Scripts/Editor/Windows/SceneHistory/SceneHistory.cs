/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using System.Linq;
using InfinityCode.uContext.Tools;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InfinityCode.uContext.Windows
{
    [InitializeOnLoad]
    public class SceneHistory : EditorWindow
    {
        private static GUIContent closeContent;
        private static GUIContent showContent;
        private static GUIStyle showContentStyle;

        private Vector2 scrollPosition;
        private string filter = "";
        private List<SceneHistoryItem> filteredRecords;
        private SceneHistoryItem selectedItem;
        private int selectedIndex = 0;
        private double lastClickTime;

        private static List<SceneHistoryItem> items
        {
            get { return ReferenceManager.sceneHistory; }
        }

        static SceneHistory()
        {
            EditorSceneManager.sceneClosed += OnSceneClosed;
        }

        private bool DrawRecord(SceneHistoryItem item)
        {
            bool ret = false;
            EditorGUILayout.BeginHorizontal(selectedItem == item ? Styles.selectedRow : Styles.transparentRow);

            if (GUILayout.Button(showContent, showContentStyle, GUILayout.ExpandWidth(false), GUILayout.Height(12)))
            {
                EditorSceneManager.OpenScene(item.path);
                Close();
            }

            GUIContent content = new GUIContent(item.name, item.path);
            ButtonEvent ev = GUILayoutUtils.Button(content, EditorStyles.label, GUILayout.Height(20), GUILayout.MaxWidth(position.width - 30));

            ProcessRecordEvents(ev, item);

            if (GUILayout.Button(closeContent, Styles.transparentButton, GUILayout.ExpandWidth(false), GUILayout.Height(12))) ret = true;

            EditorGUILayout.EndHorizontal();

            return ret;
        }

        private void OnDestroy()
        {
            filteredRecords = null;
            filter = "";
        }

        private void OnEnable()
        {
            if (filteredRecords == null && items.Count > 0) selectedItem = items[selectedIndex];

            showContent = new GUIContent(Styles.isProSkin ? Icons.openNewWhite : Icons.openNewBlack, "Open Scene");
            closeContent = new GUIContent(Styles.isProSkin ? Icons.closeWhite : Icons.closeBlack, "Remove");
        }

        private void OnGUI()
        {
            if (showContentStyle == null || showContentStyle.normal.background == null)
            {
                showContentStyle = new GUIStyle(Styles.transparentButton)
                {
                    margin =
                    {
                        top = 6,
                        left = 6
                    }
                };
            }

            if (ProcessEvents())
            {
                Close();
                return;
            }
            Toolbar();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            int removeIndex = -1;

            if (filteredRecords == null)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    SceneHistoryItem item = items[i];
                    if (DrawRecord(item)) removeIndex = i;
                }
            }
            else
            {
                for (int i = 0; i < filteredRecords.Count; i++)
                {
                    SceneHistoryItem item = filteredRecords[i];
                    if (DrawRecord(item)) removeIndex = items.IndexOf(item);
                }
            }

            if (removeIndex != -1)
            {
                items.RemoveAt(removeIndex);
                ReferenceManager.Save();
                if (filteredRecords != null) UpdateFilteredRecords();
            }

            EditorGUILayout.EndScrollView();
        }

        private static void OnSceneClosed(Scene scene)
        {
            SelectionHistory.Clear();
            string path = scene.path;
            if (string.IsNullOrEmpty(path) || items.Any(r => r.path == path)) return;

            items.Insert(0, new SceneHistoryItem
            {
                path = path,
                name = scene.name
            });

            while (items.Count > 19) items.RemoveAt(items.Count - 1);
            ReferenceManager.Save();
        }

        [MenuItem("File/Recent Scenes %#o", false, 155)]
        public static void OpenWindow()
        {
            GetWindow<SceneHistory>(true, "Recent Scenes");
        }

        private bool ProcessEvents()
        {
            Event e = Event.current;
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.DownArrow) SelectNext();
                else if (e.keyCode == KeyCode.UpArrow) SelectPrev();
                else if (e.keyCode == KeyCode.KeypadEnter || e.keyCode == KeyCode.Return) return SelectCurrent();
            }

            return false;
        }

        private void ProcessRecordClick(SceneHistoryItem item, Event e)
        {
            if (selectedItem == item)
            {
                if (EditorApplication.timeSinceStartup - lastClickTime < 0.3)
                {
                    SelectCurrent();
                    Close();
                    return;
                }

                lastClickTime = EditorApplication.timeSinceStartup;
            }
            else
            {
                lastClickTime = EditorApplication.timeSinceStartup;
                selectedItem = item;
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(item.path);
                selectedIndex = items.IndexOf(item);
            }

            e.Use();
        }

        private void ProcessRecordEvents(ButtonEvent ev, SceneHistoryItem item)
        {
            Event e = Event.current;
            if (ev == ButtonEvent.click)
            {
                if (e.button == 0) ProcessRecordClick(item, e);
            }
        }

        private bool SelectCurrent()
        {
            if (selectedItem != null)
            {
                SelectionHistory.Clear();
                EditorSceneManager.OpenScene(selectedItem.path);
                return true;
            }

            return false;
        }

        private void SelectPrev()
        {
            selectedIndex--;
            if (filteredRecords != null)
            {
                if (filteredRecords.Count == 0)
                {
                    selectedIndex = 0;
                    selectedItem = null;
                }
                else
                {
                    if (selectedIndex < 0) selectedIndex = filteredRecords.Count - 1;
                    selectedItem = filteredRecords[selectedIndex];
                }
            }
            else
            {
                if (selectedIndex < 0) selectedIndex = items.Count - 1;
                selectedItem = items[selectedIndex];
            }
            Event.current.Use();
            Repaint();
        }

        private void SelectNext()
        {
            selectedIndex++;
            if (filteredRecords != null)
            {
                if (filteredRecords.Count == 0)
                {
                    selectedIndex = 0;
                    selectedItem = null;
                }
                else
                {
                    if (selectedIndex >= filteredRecords.Count) selectedIndex = 0;
                    selectedItem = filteredRecords[selectedIndex];
                }
            }
            else
            {
                if (selectedIndex >= items.Count) selectedIndex = 0;
                selectedItem = items[selectedIndex];
            }

            Event.current.Use();
            Repaint();
        }

        private void Toolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUI.BeginChangeCheck();
            filter = EditorReflected.ToolbarSearchField(filter);
            if (EditorGUI.EndChangeCheck()) UpdateFilteredRecords();

            EditorGUILayout.EndHorizontal();
        }

        private void UpdateFilteredRecords()
        {
            if (string.IsNullOrEmpty(filter))
            {
                filteredRecords = null;
                return;
            }

            string assetType;
            string pattern = SearchableItem.GetPattern(filter, out assetType);

            filteredRecords = items.Where(i => i.UpdateAccuracy(pattern) > 0).OrderByDescending(i => i.accuracy).ToList();
            if (!filteredRecords.Contains(selectedItem))
            {
                if (filteredRecords.Count > 0) selectedItem = filteredRecords[0];
                else selectedItem = null;
                selectedIndex = 0;
            }
            else
            {
                selectedIndex = filteredRecords.IndexOf(selectedItem);
            }
        }

        [MenuItem("File/Recent Scenes %#o", true, 155)]
        public static bool ValidateOpenWindow()
        {
            return items.Count > 0;
        }
    }
}