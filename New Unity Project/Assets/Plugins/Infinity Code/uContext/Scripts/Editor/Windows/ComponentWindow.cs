/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.uContext.Windows
{
    [Serializable]
    public class ComponentWindow : AutoSizePopupWindow
    {
        #region Actions

        public static Action<ComponentWindow> OnDestroyWindow;
        public static Predicate<ComponentWindow> OnDrawContent;
        public static Action<ComponentWindow, Rect, float> OnDrawHeader;
        public static Predicate<ComponentWindow> OnValidateEditor;

        #endregion

        #region Fields

        private static ComponentWindow autoPopupWindow;
        private static GUIStyle inspectorBigStyle;

        [SerializeField]
        public string componentID;

        [SerializeField]
        public string path;

        [SerializeField]
        public string type;

        [SerializeField]
        public bool displayGameObject = true;
        
        [SerializeField]
        public bool isPopup;

        private Component _component;
        private bool isMissedComponent;
        private Editor editor;
        private GUIContent selectContent;
        private GUIContent bookmarkContent;
        private GUIContent removeBookmarkContent;
        private bool ignoreNextRepaint;

        public bool allowInitEditor = true;
        private GUIContent titleSettingsIcon;
        private MethodInfo drawEditorHeaderItems;
        private bool destroyAnyway;

        #endregion

        public Component component
        {
            get { return _component; }
            set
            {
                _component = value;
                if (_component != null)
                {
                    componentID = GlobalObjectId.GetGlobalObjectIdSlow(_component).ToString();

                    if (component.transform != null)
                    {
                        Transform transform = component.transform;
                        path = GameObjectUtils.GetTransformPath(transform).Insert(0, '/').ToString();
                    }

                    type = component.GetType().AssemblyQualifiedName;

                    if (editor != null) DestroyImmediate(editor);
                    InitEditor();
                }
            }
        }

        private void DrawHeader()
        {
            if (_component == null) return;

            if (inspectorBigStyle == null)
            {
                inspectorBigStyle = new GUIStyle(Reflection.GetStaticPropertyValue<GUIStyle>(typeof(EditorStyles), "inspectorBig"));
                inspectorBigStyle.margin = new RectOffset(1, 1, 0, 0);
            }

            titleSettingsIcon = EditorIconContents.popup;

            GUILayout.BeginHorizontal(inspectorBigStyle);
            GUILayout.Space(38f);
            GUILayout.BeginVertical();
            GUILayout.Space(19f);
            GUILayout.BeginHorizontal();
            EditorGUILayout.GetControlRect();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            Rect lastRect = GUILayoutUtility.GetLastRect();
            Rect r = new Rect(lastRect.x, lastRect.y, lastRect.width, lastRect.height);
            
            DrawHeaderPreview(r);
            DrawHeaderComponent(r);
            DrawHeaderIcons(r);
            DrawHeaderGameObject(r);
            DrawHeaderExtraButtons(r);
        }

        private void DrawHeaderComponent(Rect r)
        {
            Event e = Event.current;

            int verticalOffset = 4;
            if (!displayGameObject) verticalOffset += 8;

            Behaviour behaviour = _component as Behaviour;
            Renderer renderer = _component as Renderer;
            if (behaviour != null || renderer != null)
            {
                Rect tr1 = new Rect(r.x + 44, r.y + verticalOffset + 1, 16, 18);
                EditorGUI.BeginChangeCheck();
                bool v1 = GUI.Toggle(tr1, behaviour != null ? behaviour.enabled : renderer.enabled, GUIContent.none);
                if (EditorGUI.EndChangeCheck())
                {
                    if (behaviour != null) behaviour.enabled = v1;
                    else renderer.enabled = v1;
                }
            }

            Rect r2 = new Rect(r.x + 60, r.y + verticalOffset, r.width - 100, 18);

            GUI.Label(r2, _component.GetType().Name, EditorStyles.largeLabel);
            if (e.type == EventType.MouseDown && r2.Contains(e.mousePosition))
            {
                if (e.button == 1)
                {
                    ComponentUtils.ShowContextMenu(_component);
                    e.Use();
                }
            }
            else if (e.type == EventType.MouseDrag && r2.Contains(e.mousePosition))
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = new[] {_component};

                DragAndDrop.StartDrag("Drag " + _component.name);
                e.Use();
            }
        }

        private void DrawHeaderExtraButtons(Rect r)
        {
            Event e = Event.current;
            r = new Rect(r.x + 60, r.y + 27, r.width - 100, 18);

            bool containBookmark = Bookmarks.Contain(component);
            if (GUI.Button(new Rect(position.width - 18, r.y, 16, 16), containBookmark ? removeBookmarkContent : bookmarkContent, Styles.transparentButton))
            {
                if (e.modifiers == EventModifiers.Control)
                {
                    Bookmarks.ShowWindow();
                }
                else
                {
                    if (containBookmark) Bookmarks.Remove(component);
                    else Bookmarks.Add(component);
                    Bookmarks.Redraw();
                }
            }

            if (OnDrawHeader != null) OnDrawHeader(this, position, r.y);
        }

        private void DrawHeaderGameObject(Rect r)
        {
            if (!displayGameObject) return;

            Event e = Event.current;

            Rect tr2 = new Rect(r.x + 44, r.y + 25, 16, 18);
            EditorGUI.BeginChangeCheck();
            bool v2 = GUI.Toggle(tr2, _component.gameObject.activeSelf, GUIContent.none);
            if (EditorGUI.EndChangeCheck()) _component.gameObject.SetActive(v2);

            r = new Rect(r.x + 60, r.y + 25, r.width - 100, 18);
            GUI.Label(r, _component.gameObject.name, EditorStyles.boldLabel);
            if (e.type == EventType.MouseDown && r.Contains(e.mousePosition))
            {
                if (e.button == 0)
                {
                    Selection.activeGameObject = _component.gameObject;
                    EditorGUIUtility.PingObject(Selection.activeGameObject);
                    e.Use();
                }
                else if (e.button == 1)
                {
                    GameObjectUtils.ShowContextMenu(false, _component.gameObject);
                    e.Use();
                }
            }
            else if (e.type == EventType.MouseDrag && r.Contains(e.mousePosition))
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = new[] {_component.gameObject};

                DragAndDrop.StartDrag("Drag " + _component.gameObject.name);
                e.Use();
            }
        }

        private void DrawHeaderIcons(Rect r)
        {
            Vector2 settingsSize = Styles.iconButton.CalcSize(titleSettingsIcon);
            Rect settingsRect = new Rect(r.xMax - settingsSize.x, r.y + 5, settingsSize.x, settingsSize.y);
            if (EditorGUI.DropdownButton(settingsRect, titleSettingsIcon, FocusType.Passive, Styles.iconButton))
            {
                ComponentUtils.ShowContextMenu(_component);
            }

            float offset = settingsSize.x * 2;

#if UNITY_2019_3_OR_NEWER
            drawEditorHeaderItems.Invoke(null, new object[] {new Rect(r.xMax - offset, r.y + 5f, settingsSize.x, settingsSize.y), new UnityEngine.Object[] {component}, 0});
#else
            drawEditorHeaderItems.Invoke(null, new object[] {new Rect(r.xMax - offset, r.y + 5f, settingsSize.x, settingsSize.y), new UnityEngine.Object[]{component}});
#endif
        }

        private void DrawHeaderPreview(Rect r)
        {
            Rect r1 = new Rect(r.x + 6, r.y + 6, 32, 32);

            bool isLoadingAssetPreview = AssetPreview.IsLoadingAssetPreview(component.GetInstanceID());
            Texture2D texture2D = AssetPreview.GetAssetPreview(component);
            if (texture2D == null)
            {
                if (isLoadingAssetPreview) Repaint();
                texture2D = AssetPreview.GetMiniThumbnail(component);
            }

            GUI.Label(r1, texture2D, Styles.centeredLabel);
        }

        public void FreeEditor()
        {
            if (editor == null) return;

            if (destroyAnyway || editor.GetType().ToString() != "UnityEditor.TerrainInspector") DestroyImmediate(editor);
            editor = null;
        }

        private void FreeReferences()
        {
            _component = null;
            OnClose = null;

            FreeEditor();
        }

        private bool InitComponent(Event e)
        {
            FreeEditor();

            if (isMissedComponent)
            {
                if (e.type == EventType.Repaint && ignoreNextRepaint)
                {
                    ignoreNextRepaint = false;
                    return false;
                }

                EditorGUILayout.LabelField("Component is missed.");
                if (GUILayout.Button("Try to restore"))
                {
                    TryRestoreComponent();
                    if (isMissedComponent) return false;
                }
            }
            else TryRestoreComponent();

            return true;
        }

        public void InitEditor()
        {
            FreeEditor();

            if (component is Terrain)
            {
                editor = Reflection.GetStaticFieldValue<Editor>(EditorTypes.terrainInspector, "s_activeTerrainInspectorInstance");
                if (editor == null)
                {
                    destroyAnyway = true;
                    editor = Editor.CreateEditor(component);
                    Reflection.SetStaticFieldValue(EditorTypes.terrainInspector, "s_activeTerrainInspectorInstance", editor);
                    Reflection.SetStaticFieldValue(EditorTypes.terrainInspector, "s_activeTerrainInspector", editor.GetInstanceID());
                }
            }
            else
            {
                editor = Editor.CreateEditor(component);
            }
        }

        private void OnCompilationStarted(object obj)
        {
            FreeEditor();
        }

        protected override void OnContentGUI()
        {
            DrawHeader();

            if (OnDrawContent != null && OnDrawContent(this))
            {
                Repaint();
                return;
            }

            if (editor != null)
            {
                try
                {
                    editor.OnInspectorGUI();
                }
                catch
                {
                    
                }
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (OnDestroyWindow != null) OnDestroyWindow(this);

            FreeReferences();

            if (autoPopupWindow == this) autoPopupWindow = null;

            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnEnable()
        {
            FreeReferences();
            if (_component == null && !string.IsNullOrEmpty(componentID))
            {
                GlobalObjectId gid;
                if (componentID != null && GlobalObjectId.TryParse(componentID, out gid))
                {
                    _component = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(gid) as Component;
                }
            }
            if (_component != null)
            {
                if (_component.GetType().AssemblyQualifiedName != type) _component = null;
            }

            selectContent = EditorIconContents.rectTransformBlueprint;
            selectContent.tooltip = "Select GameObject";

            bookmarkContent = new GUIContent(Styles.isProSkin ? Icons.starWhite: Icons.starBlack, "Bookmark");
            removeBookmarkContent = new GUIContent(Icons.starYellow, "Remove Bookmark");

#if UNITY_2019_3_OR_NEWER
            drawEditorHeaderItems = Reflection.GetMethod(typeof(EditorGUIUtility), "DrawEditorHeaderItems", new[] { typeof(Rect), typeof(Object[]), typeof(float) });
#else
            drawEditorHeaderItems = Reflection.GetMethod(typeof(EditorGUIUtility), "DrawEditorHeaderItems", new[] { typeof(Rect), typeof(Object[])});
#endif

            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            CompilationPipeline.compilationStarted -= OnCompilationStarted;
            CompilationPipeline.compilationStarted += OnCompilationStarted;
        }

        protected override void OnGUI()
        {
            Event e = Event.current;

            if (EditorApplication.isCompiling)
            {
                FreeEditor();
                return;
            }

            if (_component == null && !InitComponent(e)) return;

            if (isMissedComponent) return;

            if (editor == null && allowInitEditor)
            {
                if (_component is Terrain) TryRestoreTerrainEditor();
                else InitEditor();
            }

            if (editor == null)
            {
                if (OnValidateEditor == null || !OnValidateEditor(this)) return;
            }

            if (isPopup)
            {
                GUIStyle style = GUI.skin.box;
                style.normal.textColor = Color.blue;
                GUI.Box(new Rect(0, 0, position.width, position.height), GUIContent.none, style);
            }

            base.OnGUI();
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.ExitingPlayMode) allowInitEditor = false;
            else
            {
                allowInitEditor = true;
                Repaint();
            }

            FreeEditor();
        }

        public static ComponentWindow Show(Component component, bool autosize = true)
        {
            if (component == null) return null;

            ComponentWindow wnd = CreateInstance<ComponentWindow>();

            Texture2D texture2D = AssetPreview.GetAssetPreview(component);
            if (texture2D == null) texture2D = AssetPreview.GetMiniThumbnail(component);

            wnd.titleContent = new GUIContent(component.GetType().Name + " (" + component.gameObject.name + ")", texture2D);
            wnd.component = component;
            wnd.minSize = Vector2.one;
            if (autosize) wnd.adjustHeight = AutoSize.center;
            wnd.Show();
            wnd.Focus();
            if (Event.current != null)
            {
                Vector2 screenPoint = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                Vector2 size = Prefs.contextMenuWindowSize;
                wnd.position = new Rect(screenPoint - size / 2, size);
            }
            return wnd;
        }

        public static ComponentWindow ShowPopup(Component component, Rect? rect = null, string title = null)
        {
            if (component == null) return null;
            if (autoPopupWindow != null)
            {
                autoPopupWindow.Close();
                autoPopupWindow = null;
            }

            ComponentWindow wnd = CreateInstance<ComponentWindow>();
            wnd.component = component;
            wnd.minSize = Vector2.one;

            if (!rect.HasValue)
            {
                Vector2 position = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                Vector2 size = Prefs.contextMenuWindowSize;
                rect = new Rect(position - size / 2, size);
            }

            Rect r = rect.Value;
            if (r.y < 30) r.y = 30;
            wnd.position = r
;
            wnd.ShowPopup();
            wnd.Focus();
            wnd.adjustHeight = AutoSize.top;
            wnd.isPopup = true;

            if (title == null) title = component.GetType().Name;

            PinAndClose.Show(wnd, r, wnd.Close, () =>
            {
                Rect wRect = wnd.position;
                wRect.yMin -= PinAndClose.HEIGHT;
                Show(component, false).position = wRect;
                wnd.Close();
            }, title);
            
            return wnd;
        }

        public static ComponentWindow ShowUtility(Component component, bool autosize = true)
        {
            if (component == null) return null;

            ComponentWindow wnd = CreateInstance<ComponentWindow>();
            wnd.minSize = Vector2.one;
            wnd.titleContent = new GUIContent(component.GetType().Name + " (" + component.gameObject.name + ")");
            wnd.component = component;
            wnd.ShowUtility();
            wnd.Focus();
            if (autosize) wnd.adjustHeight = AutoSize.center;
            Vector2 size = Prefs.contextMenuWindowSize;
            wnd.position = new Rect(GUIUtility.GUIToScreenPoint(Event.current.mousePosition) - size / 2, size);
            return wnd;
        }

        private void TryRestoreComponent()
        {
            FreeEditor();
            GlobalObjectId gid;
            if (GlobalObjectId.TryParse(componentID, out gid)) _component = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(gid) as Component;

            if (_component != null)
            {
                if (type == _component.GetType().AssemblyQualifiedName)
                {
                    isMissedComponent = false;
                    ignoreNextRepaint = true;
                    return;
                }

                _component = null;
            }

            GameObject go = GameObject.Find(path);
            if (go == null)
            {
                isMissedComponent = true;
                ignoreNextRepaint = true;
                return;
            }

            Type t = Type.GetType(type);
            _component = go.GetComponent(t);
            isMissedComponent = _component == null;
            if (isMissedComponent) ignoreNextRepaint = true;
            else componentID = GlobalObjectId.GetGlobalObjectIdSlow(component).ToString();
        }

        public bool TryRestoreTerrainEditor()
        {
            if (Selection.activeGameObject == _component.gameObject)
            {
                InitEditor();
                if (editor != null) return true;
            }
            
            EditorGUILayout.HelpBox("Select Terrain GameObject", MessageType.Info);
            if (GUILayout.Button("Select"))
            {
                Selection.activeGameObject = _component.gameObject;
            }

            return false;
        }
    }
}