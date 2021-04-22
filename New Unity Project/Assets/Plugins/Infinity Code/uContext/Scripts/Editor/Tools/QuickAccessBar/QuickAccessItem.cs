/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.IO;
using InfinityCode.uContext.Windows;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.uContext
{
    [Serializable]
    public class QuickAccessItem
    {
        public bool expanded = true;
        public string[] settings;
        public int[] intSettings;
        public string tooltip;
        public QuickAccessItemType type = QuickAccessItemType.window;
        public SceneViewVisibleRules visibleRules = SceneViewVisibleRules.always;
        public QuickAccessItemIcon icon = QuickAccessItemIcon.editorIconContent;
        public string iconSettings;

        [NonSerialized]
        private GUIContent _content;
        private Type _methodType;
        [NonSerialized]
        private string _typeName;
        [NonSerialized]
        private bool contentMissed;
        private bool methodTypeMissed;
        [NonSerialized]
        private bool scriptableObjectMissed;

        [NonSerialized]
        private ScriptableObject _scriptableObject;

        public GUIContent content
        {
            get
            {
                if (_content == null && !contentMissed)
                {
                    if (string.IsNullOrEmpty(iconSettings))
                    {
                        contentMissed = true;
                        return null;
                    }
                    
                    if (icon == QuickAccessItemIcon.text)
                    {
                        string text = iconSettings.Substring(0, Mathf.Min(3, iconSettings.Length));
                        _content = new GUIContent(text, tooltip);
                    }
                    else if (icon == QuickAccessItemIcon.texture)
                    {
                        Texture t = AssetDatabase.LoadAssetAtPath<Texture>(iconSettings);
                        if (t != null) _content = new GUIContent(t, tooltip);
                        else contentMissed = true;
                    }
                    else
                    {
                        Debug.unityLogger.logEnabled = false;
                        GUIContent c = EditorGUIUtility.IconContent(iconSettings, tooltip);
                        if (c != null)
                        {
                            _content = new GUIContent
                            {
                                tooltip = tooltip,
                                image = c.image,
                                text = c.text
                            };
                        }
                        Debug.unityLogger.logEnabled = true;
                        contentMissed = _content == null;
                    }
                }
                return _content;
            }
        }

        public bool isBasicType
        {
            get
            {
                if (type == QuickAccessItemType.window) return true;
                if (type == QuickAccessItemType.space) return true;
                if (type == QuickAccessItemType.flexibleSpace) return true;
                if (type == QuickAccessItemType.settings) return true;
                return false;
            }
        }

        public bool isButton
        {
            get { return type != QuickAccessItemType.space && type != QuickAccessItemType.flexibleSpace; }
        }

        public string typeName
        {
            get
            {
                if (string.IsNullOrEmpty(_typeName)) _typeName = ObjectNames.NicifyVariableName(type.ToString());
                return _typeName;
            }
            set { _typeName = value; }
        }

        public Type methodType
        {
            get
            {
                if (type != QuickAccessItemType.staticMethod) return null;
                if (settings == null || settings.Length == 0) return null;

                if (_methodType == null && !methodTypeMissed)
                {
                    if (!string.IsNullOrEmpty(settings[0])) _methodType = Type.GetType(settings[0]);
                    methodTypeMissed = _methodType == null;
                }

                return _methodType;
            }
            set
            {
                _methodType = null;
                methodTypeMissed = false;
                ResetContent();
                if (settings == null) settings = new string[2];
                if (value == null)
                {
                    settings[0] = null;
                    settings[1] = null;
                    return;
                }
                if (settings[0] == value.AssemblyQualifiedName) return;
                settings[0] = value.AssemblyQualifiedName;
                settings[1] = null;
            }
        }

        public ScriptableObject scriptableObject 
        {
            get
            {
                if (_scriptableObject == null && !scriptableObjectMissed)
                {
                    try
                    {
                        string path = settings[0];
                        if (!string.IsNullOrEmpty(path) && File.Exists(path))
                        {
                            _scriptableObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                            Debug.Log(_scriptableObject);
                        }
                    }
                    catch
                    {
                        
                    }

                    scriptableObjectMissed = _scriptableObject != null;
                }

                return _scriptableObject;
            }
            set
            {
                _scriptableObject = value;
                scriptableObjectMissed = value == null;
                if (!scriptableObjectMissed)
                {
                    settings[0] = AssetDatabase.GetAssetPath(value); 
                }
            }
        }

        public QuickAccessItem()
        {

        }

        public QuickAccessItem(QuickAccessItemType type)
        {
            this.type = type;
            if (type == QuickAccessItemType.window) settings = new string[2];
        }

        public void Invoke()
        {
            if (!Validate()) return;

            if (type == QuickAccessItemType.window)
            {
                InvokeWindow();
            }
            else if (type == QuickAccessItemType.settings)
            {
                string path = settings[0];
                if (path.StartsWith("Preferences")) SettingsService.OpenUserPreferences(path);
                else SettingsService.OpenProjectSettings(path);
            }
            else if (QuickAccess.OnInvokeExternal != null)
            {
                try
                {
                    QuickAccess.OnInvokeExternal(this);
                }
                catch
                {

                }
            }
        }

        private void InvokeWindow()
        {
            int lastWindowIndex = QuickAccess.activeWindowIndex;
            QuickAccess.CloseActiveWindow();

            if (lastWindowIndex == QuickAccess.invokeItemIndex) return;

            Type windowType = Type.GetType(settings[0]);
            if (windowType == null)
            {
                EditorUtility.DisplayDialog("Error", "Can't find the window class. Please delete the entry and add again.", "OK");
                return;
            }

            EditorWindow wnd = ScriptableObject.CreateInstance(windowType) as EditorWindow;
            if (wnd == null) return;

            wnd.ShowPopup();
            wnd.Focus();
            if (wnd.titleContent.text == windowType.FullName) wnd.titleContent.text = tooltip;
            Rect rect = wnd.position;
            Vector2 pos = new Vector2(QuickAccess.invokeItemRect.xMax, QuickAccess.invokeItemRect.y + PinAndClose.HEIGHT + 40);
#if !UNITY_2020_1_OR_NEWER
            pos += SceneView.lastActiveSceneView.position.position;
#endif
            rect.position = GUIUtility.GUIToScreenPoint(pos);
            rect.size = Prefs.contextMenuWindowSize;

            if (rect.center.y > Screen.currentResolution.height * 0.7f)
            {
                rect.y -= rect.size.y - PinAndClose.HEIGHT + 8;
            }
            wnd.position = rect;
            QuickAccess.activeWindow = wnd;
            QuickAccess.activeWindowIndex = QuickAccess.invokeItemIndex;

            PinAndClose.Show(wnd, rect, wnd.Close, () =>
            {
                EditorWindow nWnd = Object.Instantiate(wnd);
                nWnd.Show();
                Rect wRect = wnd.position;
                wRect.yMin -= PinAndClose.HEIGHT;
                nWnd.position = wRect;
                nWnd.maxSize = new Vector2(4000f, 4000f);
                nWnd.minSize = new Vector2(100f, 100f);
                QuickAccess.CloseActiveWindow();
            }, wnd.titleContent.text).closeOnLossFocus = false;

            if (windowType == EditorTypes.sceneHierarchyWindow)
            {
                HierarchyHelper.ExpandHierarchy(wnd, Selection.activeGameObject);
            }
            else if (windowType == EditorTypes.projectBrowser)
            {
                Reflection.InvokeMethod(windowType, "SetOneColumn", wnd);
            }
        }

        public void ResetContent()
        {
            _content = null;
            contentMissed = false;
        }

        private bool Validate()
        {
            return settings != null && settings.Length > 0;
        }

        public bool Visible(bool maximized)
        {
            if (visibleRules == SceneViewVisibleRules.always) return true;
            if (visibleRules == SceneViewVisibleRules.onMaximized) return maximized;
            if (visibleRules == SceneViewVisibleRules.onNormal) return !maximized;
            return false;
        }
    }
}