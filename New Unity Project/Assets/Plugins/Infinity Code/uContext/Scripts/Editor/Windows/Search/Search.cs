/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;

namespace InfinityCode.uContext.Windows
{
    [InitializeOnLoad]
    public partial class Search : PopupWindow
    {
        private const int width = 500;
        private const int maxRecords = 50;

        private static Dictionary<int, Record> projectRecords;
        private static Dictionary<int, Record> sceneRecords;
        private static Dictionary<int, Record> windowRecords;

        private static Search wnd;
        private static bool focusOnTextField = false;
        private static int searchMode = 0;
        private static Record[] bestRecords;
        private static int countBestRecords = 0;
        private static int bestRecordIndex = 0;
        private static bool updateScroll;
        private static bool needUpdateBestRecords;
        private static bool isDragStarted = false;
        private static string[] searchModeLabels = { "Everywhere", "By Hierarchy", "By Project" };

        private string searchText;
        
        private Vector2 scrollPosition;
        private bool resetSelection;
        private FieldInfo recycledEditorField;
        private FieldInfo cursorIndexField;
        private FieldInfo selectionIndexField;

        static Search()
        {
            KeyManager.KeyBinding binding = KeyManager.AddBinding();
            binding.OnValidate += OnValidate;
            binding.OnInvoke += OnInvoke;
        }

        private static void CachePrefabWithComponents(Dictionary<int, Record> tempRecords, GameObject go)
        {
            tempRecords.Add(go.GetInstanceID(), new GameObjectRecord(go));
            Component[] components = go.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                Component c = components[i];
                tempRecords.Add(c.GetInstanceID(), new ComponentRecord(c));
            }

            Transform t = go.transform;
            for (int i = 0; i < t.childCount; i++) CachePrefabWithComponents(tempRecords, t.GetChild(i).gameObject);
        }

        private static void CachePrefabWithoutComponents(Dictionary<int, Record> tempRecords, GameObject go)
        {
            tempRecords.Add(go.GetInstanceID(), new GameObjectRecord(go));
            Transform t = go.transform;
            for (int i = 0; i < t.childCount; i++) CachePrefabWithoutComponents(tempRecords, t.GetChild(i).gameObject);
        }

        private static void CacheProject()
        {
            Dictionary<int, Record> tempRecords = new Dictionary<int, Record>();
            string[] assets = AssetDatabase.FindAssets("", new[] { "Assets" });

            if (projectRecords != null)
            {
                foreach (KeyValuePair<int, Record> pair in projectRecords) pair.Value.used = false;
            }

            foreach (string guid in assets)
            {
                try
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);

                    if (string.IsNullOrEmpty(assetPath)) continue;
                    if (assetPath.Length < 8) continue;
                    if (AssetDatabase.IsValidFolder(assetPath)) continue;

                    int hashCode = assetPath.GetHashCode();

                    if (projectRecords != null)
                    {
                        Record r;

                        if (projectRecords.TryGetValue(hashCode, out r))
                        {
                            if (!tempRecords.ContainsKey(hashCode))
                            {
                                tempRecords.Add(hashCode, r);
                                r.used = true;
                            }
                            continue;
                        }
                    }

                    if (!tempRecords.ContainsKey(hashCode))
                    {
                        tempRecords.Add(hashCode, new ProjectRecord(assetPath));
                    }
                }
                catch
                {

                }
                
            }

            if (projectRecords != null)
            {
                foreach (var pair in projectRecords.Where(p => !p.Value.used)) pair.Value.Dispose();
            }

            projectRecords = tempRecords;
        }

        private static void CacheScene()
        {
            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();

            if (sceneRecords != null)
            {
                foreach (var record in sceneRecords) record.Value.used = false;
            }

            if (prefabStage != null)
            {
                Dictionary<int, Record> tempRecords = new Dictionary<int, Record>();
                try
                {
                    if (Prefs.searchByComponents) CachePrefabWithComponents(tempRecords, prefabStage.prefabContentsRoot);
                    else CachePrefabWithoutComponents(tempRecords, prefabStage.prefabContentsRoot);
                }
                catch (Exception e)
                {
                    Log.Add(e);
                }

                if (sceneRecords != null)
                {
                    foreach (var record in sceneRecords)
                    {
                        if (!record.Value.used) record.Value.Dispose();
                    }
                }

                sceneRecords = tempRecords;
            }
            else
            {
                if (Prefs.searchByComponents) CacheSceneItems();
                else CacheSceneGameObjects();
            }
        }

        private static void CacheSceneGameObjects()
        {
            Transform[] transforms = FindObjectsOfType<Transform>();
            Dictionary<int, Record> tempRecords = new Dictionary<int, Record>(transforms.Length);

            for (int i = 0; i < transforms.Length; i++)
            {
                GameObject go = transforms[i].gameObject;
                int key = go.GetInstanceID();

                Record r = null;

                if (sceneRecords == null || !sceneRecords.TryGetValue(key, out r)) r = new GameObjectRecord(go);
                else r.UpdateGameObjectName(go);

                r.used = true;
                tempRecords.Add(key, r);
            }

            if (sceneRecords != null)
            {
                foreach (var record in sceneRecords)
                {
                    if (!record.Value.used) record.Value.Dispose();
                }
            }

            sceneRecords = tempRecords;
        }

        private static void CacheSceneItems()
        {
            Component[] components = FindObjectsOfType<Component>();
            Dictionary<int, Record> tempRecords = new Dictionary<int, Record>(Mathf.NextPowerOfTwo(components.Length));

            for (int i = 0; i < components.Length; i++)
            {
                Component c = components[i];
                int key = c.GetInstanceID();
                Record r = null;
                if (sceneRecords == null || !sceneRecords.TryGetValue(key, out r)) r = new ComponentRecord(c);
                else r.UpdateGameObjectName(c.gameObject);

                r.used = true;
                tempRecords.Add(key, r);

                GameObject go = c.gameObject;
                key = go.GetInstanceID();

                if (!tempRecords.ContainsKey(key))
                {
                    if (sceneRecords == null || !sceneRecords.TryGetValue(key, out r)) r = new GameObjectRecord(go);
                    else r.UpdateGameObjectName(go);

                    r.used = true;
                    tempRecords.Add(key, r);
                }
            }

            if (sceneRecords != null)
            {
                foreach (var record in sceneRecords)
                {
                    if (!record.Value.used) record.Value.Dispose();
                }
            }

            sceneRecords = tempRecords;
        }

        private static void CacheWindows()
        {
            if (windowRecords != null) return;

            windowRecords = new Dictionary<int, Record>();
            string[] parts;

            foreach (string submenu in Unsupported.GetSubmenus("Window"))
            {
                if (submenu.ToLower() == "Window/Next Window".ToLower()) continue;
                if (submenu.ToLower() == "Window/Previous Window".ToLower()) continue;
                if (submenu.ToLower().StartsWith("Window/Layouts".ToLower())) continue;

                string menuItemStr = submenu.Substring(7);
                parts = menuItemStr.Split('/');
                windowRecords.Add(menuItemStr.GetHashCode(), new WindowRecord(submenu, parts[parts.Length - 1]));
            }

            windowRecords.Add("Project Settings...".GetHashCode(), new WindowRecord("Edit/Project Settings...", "Project Settings"));
            windowRecords.Add("Preferences...".GetHashCode(), new WindowRecord("Edit/Preferences...", "Preferences"));
        }

        private void DrawBestRecords()
        {
            if (countBestRecords == 0) return;

            if (bestRecordIndex >= countBestRecords) bestRecordIndex = 0;
            else if (bestRecordIndex < 0) bestRecordIndex = countBestRecords - 1;

            if (updateScroll)
            {
                float bry = 20 * bestRecordIndex - scrollPosition.y;
                if (bry < 0) scrollPosition.y = 20 * bestRecordIndex;
                else if (bry > 80)
                {
                    if (bestRecordIndex != countBestRecords - 1) scrollPosition.y = 20 * bestRecordIndex - 80;
                    else scrollPosition.y = 20 * bestRecordIndex - 80;
                }
            }

            int selectedIndex = -1;
            int selectedState = -1;

            scrollPosition = GUI.BeginScrollView(new Rect(0, 40, position.width, position.height - 40), scrollPosition, new Rect(0, 0, position.width - 40, countBestRecords * 20));

            for (int i = 0; i < countBestRecords; i++)
            {
                int state = bestRecords[i].Draw(i);
                if (state != 0)
                {
                    selectedIndex = i;
                    selectedState = state;
                }
            }

            GUI.EndScrollView();

            if (selectedIndex != -1) SelectRecord(selectedIndex, selectedState);
        }

        protected void OnDestroy()
        {
            bestRecords = null;
        }

        private void OnEnable()
        {
            recycledEditorField = typeof(EditorGUI).GetField("s_RecycledEditor", BindingFlags.Static | BindingFlags.NonPublic);
            cursorIndexField = typeof(TextEditor).GetField("m_CursorIndex", BindingFlags.Instance | BindingFlags.NonPublic);
            selectionIndexField = typeof(TextEditor).GetField("m_SelectIndex", BindingFlags.Instance | BindingFlags.NonPublic);

            bestRecords = new Record[maxRecords];
            countBestRecords = 0;
            bestRecordIndex = 0;

            CacheScene();
            if (Prefs.searchByProject) CacheProject();
            if (Prefs.searchByWindow) CacheWindows();
        }

        protected override void OnGUI()
        {
            if (focusedWindow != wnd)
            {
                if (isDragStarted)
                {
                    if (DragAndDrop.objectReferences.Length == 0) isDragStarted = false;
                    else Repaint();
                }

                if (!isDragStarted)
                {
                    EventManager.BroadcastClosePopup();
                    return;
                }
            }

            if (EditorApplication.isCompiling)
            {
                EventManager.BroadcastClosePopup();
                return;
            }

            if (sceneRecords == null) CacheScene();
            if (projectRecords == null) CacheProject();
            if (windowRecords == null) CacheWindows();

            if (!ProcessEvents()) return;

            GUI.Box(new Rect(0, 0, position.width, position.height), GUIContent.none, EditorStyles.toolbar);

            if (Prefs.searchByProject)
            {
                EditorGUILayout.BeginHorizontal(GUILayout.Height(20));

                GUILayout.Space(100);

                EditorGUI.BeginChangeCheck();
                searchMode = GUILayout.Toolbar(searchMode, searchModeLabels, EditorStyles.toolbarButton);
                if (EditorGUI.EndChangeCheck())
                {
                    needUpdateBestRecords = true;
                    focusOnTextField = true;
                }

                GUILayout.Space(100);

                EditorGUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label("Search", Styles.centeredLabel, GUILayout.Height(20));
            }

            GUI.SetNextControlName("uContextSearchTextField");
            EditorGUI.BeginChangeCheck();
            searchText = EditorReflected.ToolbarSearchField(searchText);
            bool changed = EditorGUI.EndChangeCheck();

            if (resetSelection && Event.current.type == EventType.Repaint)
            {
                GUI.FocusControl("uContextSearchTextField");
                object recycledEditor = recycledEditorField.GetValue(null);
                cursorIndexField.SetValue(recycledEditor, searchText.Length);
                selectionIndexField.SetValue(recycledEditor, searchText.Length);
                resetSelection = false;
                Repaint();
            }

            if (focusOnTextField && Event.current.type == EventType.Repaint)
            {
                GUI.FocusControl("uContextSearchTextField");
                focusOnTextField = false;
                if (!string.IsNullOrEmpty(searchText)) resetSelection = true;
            }

            if (changed || needUpdateBestRecords) UpdateBestRecords();

            DrawBestRecords();
        }

        private static void OnInvoke()
        {
            Event e = Event.current;
            Vector2 position = e.mousePosition;

            if (focusedWindow != null) position += focusedWindow.position.position;

            Rect rect = new Rect(position + new Vector2(width / -2, -30), new Vector2(width, 140));

#if !UNITY_EDITOR_OSX
            if (rect.y < 5) rect.y = 5;
            else if (rect.yMax > Screen.currentResolution.height - 40) rect.y = Screen.currentResolution.height - 40 - rect.height;
#endif  

            Show(rect);
            e.Use();
        }

        private static bool OnValidate()
        {
            if (!Prefs.search) return false;
            Event e = Event.current;

            if (e.keyCode != Prefs.searchKeyCode) return false;
            if (e.modifiers != Prefs.searchModifiers) return false;

            if (Prefs.SearchDoNotShowOnWindows()) return false;
            return true;
        }

        private static bool ProcessEvents()
        {
            Event e = Event.current;
            updateScroll = false;

            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.DownArrow)
                {
                    bestRecordIndex++;
                    updateScroll = true;
                    e.Use();
                }
                else if (e.keyCode == KeyCode.UpArrow)
                {
                    bestRecordIndex--;
                    updateScroll = true;
                    e.Use();
                }
                else if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
                {
                    if (countBestRecords > 0)
                    {
                        Record bestRecord = bestRecords[bestRecordIndex];
                        int state = 1;
                        if (e.modifiers == EventModifiers.Control || e.modifiers == EventModifiers.Command) state = 2;
                        else if (e.modifiers == EventModifiers.Shift) state = 3;
                        bestRecord.Select(state);

                        EventManager.BroadcastClosePopup();

                        return false;
                    }
                }
                else if (e.keyCode == KeyCode.Escape)
                {
                    EventManager.BroadcastClosePopup();
                    return false;
                }
            }
            else if (e.type == EventType.KeyUp)
            {
                if (Prefs.searchByProject && e.keyCode == KeyCode.Tab)
                {
                    focusOnTextField = true;
                    searchMode++;
                    if (searchMode == 3) searchMode = 0;

                    bestRecordIndex = 0;
                    needUpdateBestRecords = true;
                    e.Use();
                }
            }

            return true;
        }

        private static void SelectRecord(int index, int state)
        {
            bestRecords[index].Select(state);
            EventManager.BroadcastClosePopup();
        }

        public static void Show(Rect rect)
        {
            EventManager.BroadcastClosePopup();

            SceneView.RepaintAll();

            if (Prefs.searchPauseInPlayMode && EditorApplication.isPlaying) EditorApplication.isPaused = true;

            wnd = CreateInstance<Search>();
            wnd.position = rect;
            wnd.ShowPopup();
            wnd.Focus();
            focusOnTextField = true;
            searchMode = 0;

            EventManager.AddBinding(EventManager.ClosePopupEvent).OnInvoke += b =>
            {
                wnd.Close();
                b.Remove();
            };
        }

        private int TakeBestRecords(IEnumerable<KeyValuePair<int, Record>> tempBestRecords)
        {
            int count = 0;
            float minAccuracy = float.MaxValue;

            foreach (var pair in tempBestRecords)
            {
                Record v = pair.Value;
                float a = v.accuracy;

                if (count < maxRecords)
                {
                    bestRecords[count] = v;
                    count++;
                    if (minAccuracy > a) minAccuracy = a;
                    continue;
                }

                if (a <= minAccuracy) continue;

                float newMin = float.MaxValue;
                bool needReplace = true;

                for (int i = 0; i < maxRecords; i++)
                {
                    Record v1 = bestRecords[i];
                    float a1 = v1.accuracy;
                    if (needReplace && a1 == minAccuracy)
                    {
                        if (v1.label.Length > v.label.Length)
                        {
                            bestRecords[i] = v;
                            needReplace = false;
                        }
                        newMin = a1;
                    }
                    else if (newMin > a1) newMin = a1;
                }

                minAccuracy = newMin;
            }

            if (count > 1)
            {
                Record[] sortedRecords = bestRecords.Take(count)
                    .OrderByDescending(r => r.accuracy)
                    .ThenBy(r => r.label.Length)
                    .ThenBy(r => r.label).ToArray();

                for (int i = 0; i < sortedRecords.Length; i++) bestRecords[i] = sortedRecords[i];
            }

            return count;
        }

        private void UpdateBestRecords()
        {
            needUpdateBestRecords = false;
            bestRecordIndex = 0;
            countBestRecords = 0;
            scrollPosition = Vector2.zero;

            int minStrLen = 1;
            if (searchText == null || searchText.Length < minStrLen) return;

            string assetType;
            string search = SearchableItem.GetPattern(searchText, out assetType);

            IEnumerable <KeyValuePair<int, Record>> tempBestRecords;

            if (searchMode == 0)
            {
                int currentMode = 0;
                tempBestRecords = new List<KeyValuePair<int, Record>>();
                if (search.Length > 0)
                {
                    if (search[0] == '@') currentMode = 1;
                    else if (search[0] == '#') currentMode = 2;
                }

                if (currentMode != 0) search = search.Substring(1);

                if (Prefs.searchByWindow && currentMode == 0) tempBestRecords = tempBestRecords.Concat(windowRecords.Where(r => r.Value.Update(search, assetType) > 0));
                if (currentMode == 0 || currentMode == 1) tempBestRecords = tempBestRecords.Concat(sceneRecords.Where(r => r.Value.Update(search, assetType) > 0));
                if (Prefs.searchByProject && currentMode == 0 || currentMode == 2) tempBestRecords = tempBestRecords.Concat(projectRecords.Where(r => r.Value.Update(search, assetType) > 0));
            }
            else
            {
                tempBestRecords = searchMode == 1? sceneRecords: projectRecords;
                tempBestRecords = tempBestRecords.Where(r => r.Value.Update(search, assetType) > 0);
            }

            countBestRecords = TakeBestRecords(tempBestRecords);
            updateScroll = true;
        }
    }
}