/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.Linq;
using InfinityCode.uContext.Actions;
using InfinityCode.uContext.Windows;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.uContext.FloatToolbar
{
    [InitializeOnLoad]
    public class ObjectToolbar : FloatToolbar
    {
        public static Action<List<Item>> OnGenerateLayoutComplete;
        public static Action<Component[], List<Component>> OnGetRelatedComponents;

        private static ObjectToolbar _instance;
        private static Item showItemLate;

        private static Type[] ignoreTypes = new[]
        {
            typeof(DefaultAsset),
            typeof(SceneAsset)
        };

        private AutoSizePopupWindow _activeWindow;
        private GUIContent _addComponentContent;
        private Rect _areaRect;
        private GUIStyle _headerStyle;
        private GUIStyle _headerHoverStyle;
        private GUIStyle _relatedContentStyle;
        private GUIStyle _selectedContentStyle;
        private GUIStyle _selectedRelatedContentStyle;
        private Vector2 _size = new Vector2(200, 20);
        private int activeIndex = -1;
        private Texture2D background;
        private bool invertVertical = false;
        private bool isMultiple;
        private bool isSelectionChanged;
        private List<Item> items;
        private GUIContent labelContent;
        private bool needMoveWindow;
        private List<Item> relatedItems;
        private Object[] targets;
        private bool visible;
        private float componentsWidth;
        private string lastTargetName;

        private GUIContent addComponentContent
        {
            get
            {
                if (_addComponentContent == null) _addComponentContent = new GUIContent("+", "Add Component");
                return _addComponentContent;
            }
        }

        private GUIStyle headerStyle
        {
            get
            {
                if (_headerStyle == null)
                {
                    _headerStyle = new GUIStyle();
                    _headerStyle.normal.textColor = Styles.ProLabelColor;
                    _headerStyle.normal.background = Resources.CreateSinglePixelTexture(Styles.ProButton);
                    _headerStyle.hover = _headerStyle.onHover = new GUIStyleState
                    {
                        background = Resources.CreateSinglePixelTexture(Styles.ProButtonHover),
                    };
                    _headerStyle.alignment = TextAnchor.MiddleCenter;
                    _headerStyle.fixedHeight = 20;
                    _headerStyle.padding = new RectOffset(2, 0, 0, 2);
                }

                return _headerStyle;
            }
        }

        private GUIStyle headerHoverStyle
        {
            get
            {
                if (_headerHoverStyle == null)
                {
                    _headerHoverStyle = new GUIStyle(headerStyle);
                    _headerHoverStyle.normal.background = Resources.CreateSinglePixelTexture(Styles.ProButtonHover);
                }

                return _headerHoverStyle;
            }
        }

        private GUIStyle relatedContentStyle
        {
            get
            {
                if (_relatedContentStyle == null || _relatedContentStyle.normal.background == null)
                {
                    GUIStyle s = EditorStyles.toolbarButton;
                    _relatedContentStyle = new GUIStyle
                    {
                        normal = { background = Resources.CreateSinglePixelTexture(1, 0.5f, 0, 0.3f) },
                        margin = s.margin,
                        padding = s.padding,
                        fixedHeight = s.fixedHeight
                    };
                }

                return _relatedContentStyle;
            }
        }

        private GUIStyle selectedContentStyle
        {
            get
            {
                if (_selectedContentStyle == null || _selectedContentStyle.normal.background == null)
                {
                    GUIStyle s = EditorStyles.toolbarButton;
                    _selectedContentStyle = new GUIStyle
                    {
                        normal = { background = Resources.CreateSinglePixelTexture(1, 0.1f) },
                        margin = s.margin,
                        padding = s.padding,
                        fixedHeight = s.fixedHeight
                    };
                }

                return _selectedContentStyle;
            }
        }

        private GUIStyle selectedRelatedContentStyle
        {
            get
            {
                if (_selectedRelatedContentStyle == null || _selectedRelatedContentStyle.normal.background == null)
                {
                    GUIStyle s = EditorStyles.toolbarButton;
                    _selectedRelatedContentStyle = new GUIStyle
                    {
                        normal = { background = Resources.CreateSinglePixelTexture(1, 0.5f, 0, 0.6f) },
                        margin = s.margin,
                        padding = s.padding,
                        fixedHeight = s.fixedHeight
                    };
                }

                return _selectedRelatedContentStyle;
            }
        }

        public static ObjectToolbar instance
        {
            get { return _instance; }
        }

        protected override string prefKey
        {
            get { return "ObjectToolbar"; }
        }

        protected override Vector2 size
        {
            get { return _size; }
        }

        protected override Rect areaRect
        {
            get { return _areaRect; }
        }

        public static EditorWindow activeWindow
        {
            get
            {
                if (_instance == null) return null;
                return _instance._activeWindow;
            }
        }

        static ObjectToolbar()
        {
            FloatToolbarManager.Add(new ObjectToolbar());
        }

        public ObjectToolbar()
        {
            _instance = this;
            items = new List<Item>();
            relatedItems = new List<Item>();

            WindowManager.OnWindowFocused += OnWindowFocused;
            QuickAccess.OnVisibleChanged += OnQuickAccessVisibleChanged;
            Selection.selectionChanged += OnSelectionChanged;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            OnSelectionChanged();
            KeyManager.KeyBinding binding = KeyManager.AddBinding();
            binding.OnValidate += OnValidateKey;
            binding.OnInvoke += OnInvokeKey;
        }

        ~ObjectToolbar()
        {
            _instance = null;
            targets = null;

            if (items != null)
            {
                FreeItems();
            }
        }

        public static void CloseActiveWindow()
        {
            if (_instance == null) return;
            AutoSizePopupWindow wnd = _instance._activeWindow;
            if (wnd == null) return;

            _instance.activeIndex = -1;
            if (uContextMenu.allowCloseWindow) wnd.Close();
            _instance._activeWindow = null;
        }

        private void DrawAddComponentButton()
        {
            if (!GUILayout.Button(addComponentContent, headerStyle, GUILayout.Width(20))) return;

            CloseActiveWindow();
            uContextMenu.Close();
            AddComponent.ShowAddComponent(GetWindowRect(Prefs.contextMenuWindowSize));
        }

        protected override void DrawBackground()
        {
            if (background == null)
            {
                background = new Texture2D(1, 1);
                float v = 0.2f;
                background.SetPixel(0, 0, new Color(v, v, v, 1));
                background.Apply();
            }

            GUI.DrawTexture(new Rect(0, 0, _areaRect.width, _areaRect.height), background, ScaleMode.StretchToFill);
        }

        protected override void DrawContent()
        {
            if (isMultiple || items.Count == 0) return;

            if (showItemLate != null)
            {
                ShowItem(showItemLate);
                showItemLate = null;
            }

            if (needMoveWindow && Event.current.type == EventType.Layout)
            {
                needMoveWindow = false;

                if (_activeWindow != null && !_activeWindow.wasMoved)
                {
                    Rect r = GetWindowRect(_activeWindow.position.size);
                    r.x += 1;
                    if (invertVertical) r.y += 14;
                    _activeWindow.SetRect(r);
                }
            }

            EditorGUILayout.BeginHorizontal();

            for (int i = 0; i < items.Count; i++)
            {
                DrawItem(items[i], i, EditorStyles.toolbarButton, selectedContentStyle);
            }

            for (int i = 0; i < relatedItems.Count; i++)
            {
                DrawItem(relatedItems[i], i + items.Count, relatedContentStyle, selectedRelatedContentStyle);
            }

            if (targets.Length == 1 && targets[0] is GameObject) DrawAddComponentButton();

            EditorGUILayout.Space();

            EditorGUILayout.EndHorizontal();
        }

        protected override Rect DrawHeader()
        {
            Rect headerRect = GUILayoutUtility.GetRect(labelContent, headerStyle, GUILayout.Width(rect.width + 2), GUILayout.Height(20));
            GUIStyle style = headerRect.Contains(Event.current.mousePosition) ? headerHoverStyle : headerStyle;
            GUI.Label(headerRect, labelContent, style);
            return headerRect;
        }

        private void DrawItem(Item item, int index, GUIStyle normalStyle, GUIStyle selectedStyle)
        {
            Event e = Event.current;

            GUIStyle style = normalStyle;
            if (index == activeIndex) style = selectedStyle;
            ButtonEvent buttonEvent = GUILayoutUtils.Button(item.content, style, GUILayout.Width(item.width));
            if (buttonEvent == ButtonEvent.click)
            {
                if (e.button == 0)
                {
                    ShowItem(item);
                    e.Use();
                }
                else if (e.button == 1)
                {
                    ComponentUtils.ShowContextMenu(item.target);
                    e.Use();
                }
            }
            else if (buttonEvent == ButtonEvent.drag)
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = new[] { item.target };
                DragAndDrop.StartDrag("Drag " + item.target.name);
                e.Use();
            }
        }

        protected override void DrawToolbar(SceneView sceneView)
        {
            DrawBackground();

            if (!invertVertical)
            {
                Rect headerRect = DrawHeader();
                ProcessHeaderEvents(sceneView.position, headerRect);
                DrawContent();
            }
            else
            {
                DrawContent();

                Rect headerRect = DrawHeader();
                ProcessHeaderEvents(sceneView.position, headerRect);
            }
        }

        private void FreeItems()
        {
            CloseActiveWindow();
            activeIndex = -1;
            for (int i = 0; i < items.Count; i++) items[i].Dispose();
            items.Clear();

            for (int i = 0; i < relatedItems.Count; i++) relatedItems[i].Dispose();
            relatedItems.Clear();
        }

        private void GenerateLayout()
        {
            isSelectionChanged = false;

            string title = "";
            componentsWidth = 0;

            FreeItems();

            if (targets.Length == 1)
            {
                Object target = targets[0];
                if (target != null)
                {
                    title = target.name;

                    GameObject go = target as GameObject;
                    if (go != null)
                    {
                        if (go.scene.name != null) InitComponents(target as GameObject);
                        else
                        {
                            string assetPath = AssetDatabase.GetAssetPath(go);
                            AssetImporter importer = AssetImporter.GetAtPath(assetPath);

                            if (importer == null || importer.GetType() == EditorTypes.prefabImporter) InitComponents(target as GameObject);
                            else InitObject(target);
                        }
                    }
                    else InitObject(target);
                }
            }
            else
            {
                title = "Multiple (" + targets.Length + ")";
            }

            lastTargetName = title;
            labelContent = new GUIContent(title);
            float labelWidth = EditorStyles.label.CalcSize(labelContent).x + 10;
            _size.x = Mathf.Max(labelWidth, componentsWidth);

            if (OnGenerateLayoutComplete != null)
            {
                try
                {
                    OnGenerateLayoutComplete(items);
                }
                catch (Exception e)
                {
                    Log.Add(e);
                }
            }
        }

        private Item GetComponentItem(Component component, int index, GUIStyle style)
        {
            Texture2D thumbnail = AssetPreview.GetMiniThumbnail(component);
            string tooltip = ObjectNames.NicifyVariableName(component.GetType().Name);
            GUIContent content = new GUIContent(thumbnail, tooltip);
            bool useIcon = true;
            if (thumbnail.name == "cs Script Icon" || thumbnail.name == "d_cs Script Icon")
            {
                GameObjectUtils.GetPsIconContent(content);
                useIcon = false;
            }

            if (index < 9) content.tooltip += " (Alt+" + (index + 1) + ")";

            Vector2 s = new Vector2();

            if (!useIcon)
            {
                s = style.CalcSize(content);
                if (s.x < 25) s.x = 25;
            }
            else s.x = 25;

            componentsWidth += s.x;

            return new Item(content, component, index, s.x);
        }

        private Rect GetWindowRect(Vector2 windowSize)
        {
            Rect r = new Rect(GUIUtility.GUIToScreenPoint(new Vector2(-2, 0)), windowSize);
            if (invertVertical) r.y -= windowSize.y + 15;
            else r.y += areaRect.height + 21;

            if (hasVerticalCenterAlign) r.x -= (windowSize.x - areaRect.width) / 2;
            else if (hasRightAlign) r.x -= windowSize.x - areaRect.width - 2;
            return r;
        }

        private void InitComponents(GameObject target)
        {
            Component[] components = target.GetComponents<Component>();

            GUIStyle style = EditorStyles.toolbarButton;

            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if (component == null) continue;

                Item item = GetComponentItem(component, items.Count, style);
                items.Add(item);
            }

            relatedItems.Clear();

            if (OnGetRelatedComponents != null)
            {
                List<Component> relatedComponents = new List<Component>();
                Delegate[] invocationList = OnGetRelatedComponents.GetInvocationList();

                foreach (Action<Component[], List<Component>> d in invocationList)
                {
                    try
                    {
                        d.Invoke(components, relatedComponents);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }

                for (int i = 0; i < relatedComponents.Count; i++)
                {
                    Component component = relatedComponents[i];
                    if (component == null) continue;

                    Item item = GetComponentItem(component, relatedItems.Count + items.Count, style);
                    item.related = true;
                    item.content.tooltip = "[Related] " + component.gameObject.name + " / " + item.content.tooltip;
                    relatedItems.Add(item);
                }
            }

            componentsWidth += 20;
        }

        private void InitObject(Object target)
        {
            Texture2D thumbnail = AssetPreview.GetMiniThumbnail(target);
            GUIContent content;
            if (thumbnail != null) content = new GUIContent(thumbnail, target.name);
            else content = EditorGUIUtility.IconContent("UnityEditor.InspectorWindow", target.name);

            string assetPath = AssetDatabase.GetAssetPath(target);
            if (!string.IsNullOrEmpty(assetPath))
            {
                AssetImporter importer = AssetImporter.GetAtPath(assetPath);
                if (importer != null)
                {
                    Type type = importer.GetType();
                    if (type != typeof(AssetImporter) && type != typeof(MonoImporter))
                    {
                        target = importer;
                        thumbnail = AssetPreview.GetMiniThumbnail(target);
                        if (thumbnail != null) content.image = thumbnail;
                        content.tooltip = content.tooltip + " (Import Settings)";
                    }
                }
            }

            Item item = new Item(content, target, 0, 25);
            items.Add(item);
            componentsWidth += 25;
        }

        protected override void OnDisable()
        {
            if (_activeWindow != null)
            {
                _activeWindow.Close();
                _activeWindow = null;
            }
        }

        protected override void OnHeaderClick()
        {
            ObjectWindow w = activeWindow as ObjectWindow;
            bool isCloseEvent = w != null && w.targets[0] == targets[0];
            CloseActiveWindow();
            if (isCloseEvent) return;

            activeIndex = -1;

            Rect r = GetWindowRect(Prefs.contextMenuWindowSize);
            r.x += 1;
            AutoSize autoSize = AutoSize.top;
            if (invertVertical)
            {
                r.y += 14;
                autoSize = AutoSize.bottom;
            }

            EditorWindow focusedWindow = EditorWindow.focusedWindow;
            ObjectWindow wnd = ObjectWindow.ShowPopup(targets, r);
            wnd.adjustHeight = autoSize;
            wnd.closeOnLossFocus = false;
            wnd.drawInspector = false;
            _activeWindow = wnd;
            if (focusedWindow != null) focusedWindow.Focus();
            _activeWindow.OnClose += aw =>
            {
                _activeWindow = null;
                activeIndex = -1;
            };
        }

        private void OnInvokeKey()
        {
            Event e = Event.current;
            int keyCode = (int)e.keyCode;
            int key = keyCode - (int)KeyCode.Alpha1;
            if (key < 0) key = keyCode - (int)KeyCode.Keypad1;
            if (ShowItem(key)) e.Use();
        }

        private void OnQuickAccessVisibleChanged()
        {
            isDirty = true;
        }

        protected override void OnQuitting()
        {
            CloseActiveWindow();
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            try
            {
                if (state == PlayModeStateChange.EnteredEditMode) OnSelectionChanged();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

        }

        protected override void OnPositionChanged(Rect sceneRect)
        {
            invertVertical = rect.center.y > sceneRect.size.y / 2;

            _areaRect = new Rect(rect);
            if (!isMultiple)
            {
                if (!invertVertical) _areaRect.height += 20;
                else _areaRect.yMin -= 20;
            }

            needMoveWindow = true;
        }

        private void OnSelectionChanged()
        {
            if (_activeWindow != null)
            {
                _activeWindow.Close();
                _activeWindow = null;
            }

            targets = Selection.objects.Where(o => o != null && !ignoreTypes.Contains(o.GetType())).ToArray();
            visible = targets.Length > 0;
            isMultiple = targets.Length > 1;

            isDirty = true;
            isSelectionChanged = true;

            SceneView.RepaintAll();
        }

        private void OnWindowFocused(EditorWindow window)
        {
            if (window == null) return;

            if (window.GetType() == EditorTypes.gameView) CloseActiveWindow();
            else if (window.maximized && !(window is SceneView)) CloseActiveWindow();
        }

        private bool OnValidateKey()
        {
            Event e = Event.current;
            if (e.modifiers != EventModifiers.Alt) return false;
            int keyCode = (int)e.keyCode;
            return (keyCode >= (int)KeyCode.Alpha1 && keyCode <= (int)KeyCode.Alpha9) || (keyCode >= (int)KeyCode.Keypad1 && keyCode <= (int)KeyCode.Keypad9);
        }

        protected override void PrepareHeaderContextMenu(GenericMenuEx menu)
        {
            if (targets.All(t => t is GameObject)) GameObjectUtils.ShowContextMenu(false, targets.Select(t => t as GameObject).ToArray());
        }

        protected override void PrepareToolbar(Rect sceneRect)
        {
            if (Event.current.type != EventType.Layout || targets.Length != 1) return;

            Object firstTarget = targets[0];
            GameObject go = firstTarget as GameObject;

            if (go != null)
            {
                if (go.scene.name != null)
                {
                    Component[] components = go.GetComponents<Component>();
                    if (components.Length != items.Count)
                    {
                        GenerateLayout();
                        UpdateRect(sceneRect);
                        foreach (PinAndClose pinAndClose in UnityEngine.Resources.FindObjectsOfTypeAll<PinAndClose>()) pinAndClose.Repaint();
                        return;
                    }

                    for (int i = 0; i < components.Length; i++)
                    {
                        if (items[i].target != components[i])
                        {
                            GenerateLayout();
                            UpdateRect(sceneRect);
                            foreach (PinAndClose pinAndClose in UnityEngine.Resources.FindObjectsOfTypeAll<PinAndClose>()) pinAndClose.Repaint();
                            return;
                        }
                    }
                }

                if (targets.Length == 1 && firstTarget.name != lastTargetName)
                {
                    lastTargetName = firstTarget.name;
                    labelContent = new GUIContent(lastTargetName);
                    float labelWidth = EditorStyles.label.CalcSize(labelContent).x + 10;
                    _size.x = Mathf.Max(labelWidth, componentsWidth);
                    UpdateRect(sceneRect);
                    foreach (PinAndClose pinAndClose in UnityEngine.Resources.FindObjectsOfTypeAll<PinAndClose>()) pinAndClose.Repaint();
                }
            }
            else targets = new Object[0];
        }

        protected override void Reset()
        {
            align = ToolbarAlign.bottomLeft;
            position = new Vector2(3, -25);
        }

        protected override void StartDragTargets()
        {
            if (targets[0] is GameObject)
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = new Object[] { targets[0] };
                DragAndDrop.StartDrag("Drag " + labelContent.text);
            }
            else
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.paths = new[] { AssetDatabase.GetAssetPath(targets[0]) };
                DragAndDrop.objectReferences = new Object[] { targets[0] };
                DragAndDrop.StartDrag("Drag " + labelContent.text);
            }
        }

        private void ShowItem(Item item)
        {
            int prevIndex = activeIndex;

            CloseActiveWindow();

            if (prevIndex == item.index) return;

            activeIndex = item.index;

            Rect r = GetWindowRect(Prefs.contextMenuWindowSize);
            r.x += 1;
            AutoSize autoSize = AutoSize.top;
            if (invertVertical)
            {
                r.y += 14;
                autoSize = AutoSize.bottom;
            }

            EditorWindow focusedWindow = EditorWindow.focusedWindow;

            AutoSizePopupWindow wnd;
            if (item.target is Component)
            {
                string title = item.target.GetType().Name;
                Component component = item.target as Component;
                if (item.related) title = "[Related] " + component.gameObject.name + " / " + title;
                wnd = ComponentWindow.ShowPopup(component, r, title);
                wnd.maxHeight = SceneView.lastActiveSceneView.position.height - 100;
            }
            else
            {
                string title = item.content.tooltip;
                if (string.IsNullOrEmpty(title)) title = targets[0].name;
                wnd = ObjectWindow.ShowPopup(new[] { item.target }, r, title);
                wnd.maxHeight = SceneView.lastActiveSceneView.position.height - 100;
            }

            wnd.adjustHeight = item.target is MonoScript ? AutoSize.ignore : autoSize;
            wnd.closeOnLossFocus = false;
            _activeWindow = wnd;
            if (focusedWindow != null) focusedWindow.Focus();
            _activeWindow.OnClose += w =>
            {
                _activeWindow = null;
                activeIndex = -1;
            };
        }

        public static bool ShowItem(int index)
        {
            if (_instance == null) return false;
            if (index < 0 || index >= _instance.items.Count + _instance.relatedItems.Count) return false;

            if (index < _instance.items.Count) showItemLate = _instance.items[index];
            else showItemLate = _instance.relatedItems[index - _instance.items.Count];
            SceneView.RepaintAll();

            return true;
        }

        protected override void UpdateRect(Rect sceneRect)
        {
            if (isSelectionChanged) GenerateLayout();
            base.UpdateRect(sceneRect);
        }

        protected override bool Visible()
        {
            if (!Prefs.objectToolbar) return false;
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null) return false;
            bool maximized = sceneView.maximized;

            if (Prefs.objectToolbarVisibleRules == SceneViewVisibleRules.onMaximized && !maximized ||
                Prefs.objectToolbarVisibleRules == SceneViewVisibleRules.onNormal && maximized) return false;

            return visible;
        }

        public class Item
        {
            public GUIContent content;
            public Object target;
            public int index;
            public float width;
            public bool related;

            public Item(GUIContent content, Object target, int index, float width)
            {
                this.content = content;
                this.target = target;
                this.index = index;
                this.width = width;
            }

            public void Dispose()
            {
                content = null;
                target = null;
            }
        }
    }
}