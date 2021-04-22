/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

namespace InfinityCode.uContext.Tools
{
    [InitializeOnLoad]
    public static class HierarchyMainIcon
    {
        private static HashSet<int> hierarchyWindows;
        private static Dictionary<int, CacheItem> bestIconCache;
        private static Texture _unityLogoTexture;
        private static bool inited = false;

        private static Texture unityLogoTexture
        {
            get
            {
                if (_unityLogoTexture == null) _unityLogoTexture = EditorGUIUtility.IconContent("SceneAsset Icon").image;

                return _unityLogoTexture;
            }
        }

        static HierarchyMainIcon()
        {
            hierarchyWindows = new HashSet<int>();
            bestIconCache = new Dictionary<int, CacheItem>();
            HierarchyItemDrawer.Register("HierarchyMainIcon", DrawItem, 0);
        }

        private static Rect DrawItem(int id, Rect rect)
        {
            if (!Prefs.hierarchyOverrideMainIcon) return rect;

            if (!inited) Init();

            Event e = Event.current;

            if (e.type == EventType.Layout)
            {
                EditorWindow lastHierarchyWindow = HierarchyHelper.GetLastHierarchyWindow();
                int wid = lastHierarchyWindow.GetInstanceID();
                if (!hierarchyWindows.Contains(wid)) InitWindow(lastHierarchyWindow, wid);
            }

            CacheItem item;
            Texture texture = null;
            if (bestIconCache.TryGetValue(id, out item))
            {
                if (!item.isPrefab && e.type == EventType.Layout)
                {
                    Object obj = EditorUtility.InstanceIDToObject(id);
                    GameObject go = obj as GameObject;
                    if (go != null)
                    {
                        Component[] components = go.GetComponents<Component>();
                        if (components.Length == item.countComponents) texture = item.texture;
                    }
                    else if (obj == null)
                    {
                        texture = unityLogoTexture;
                    }
                    else
                    {
                        return rect;
                    }
                }
                else texture = item.texture;
            }
            
            if (texture == null)
            {
                Object obj = EditorUtility.InstanceIDToObject(id);
                GameObject go = obj as GameObject;
                if (go != null)
                {
                    item = new CacheItem();

                    if (PrefabUtility.IsPartOfAnyPrefab(go))
                    {
                        texture = EditorGUIUtility.IconContent("Prefab Icon").image;
                        item.isPrefab = true;
                    }
                    else
                    {
                        texture = AssetPreview.GetMiniThumbnail(go);

                        if (texture.name == "d_GameObject Icon" || texture.name == "GameObject Icon")
                        {
                            Component[] components = go.GetComponents<Component>();
                            item.countComponents = components.Length;
                            Component best = components[0];
                            if (components.Length > 1)
                            {
                                best = components[1];
                                if (components.Length > 2)
                                {
                                    if (best is CanvasRenderer)
                                    {
                                        best = components[2];
                                        if (components.Length > 3 && best is Image) best = components[3];
                                    }
                                }
                            }

                            texture = AssetPreview.GetMiniThumbnail(best);
                            if (texture == null) texture = AssetPreview.GetMiniThumbnail(components[0]);
                            if (texture == null) texture = EditorGUIUtility.IconContent("GameObject Icon").image;
                        }
                    }

                    item.texture = texture;

                    bestIconCache[id] = item;
                }
                else if (obj == null)
                {
                    item = new CacheItem();
                    texture = item.texture = unityLogoTexture;
                    bestIconCache[id] = item; 
                }
                else
                {
                    return rect;
                }
            }

            const int iconSize = 16;

            Rect iconRect = new Rect(rect) { width = iconSize, height = iconSize };
            iconRect.y += (rect.height - iconSize) / 2;
            GUI.DrawTexture(iconRect, texture, ScaleMode.ScaleToFit);

            return rect;
        }

        private static void Init()
        {
            inited = true; 
            Object[] windows = UnityEngine.Resources.FindObjectsOfTypeAll(EditorTypes.sceneHierarchyWindow);
            foreach (Object window in windows)
            {
                int wid = window.GetInstanceID();
                if (!hierarchyWindows.Contains(wid)) InitWindow(window as EditorWindow, wid);
            }
        }

        private static void InitWindow(EditorWindow lastHierarchyWindow, int wid)
        {
            if (float.IsNaN(lastHierarchyWindow.rootVisualElement.worldBound.width)) return;

            IMGUIContainer container = lastHierarchyWindow.rootVisualElement.parent.Query<IMGUIContainer>().First();
            container.onGUIHandler = (() => OnGUIBefore(wid)) + container.onGUIHandler;
            HierarchyHelper.SetDefaultIconsSize(lastHierarchyWindow);
            hierarchyWindows.Add(wid);
        }

        private static void OnGUIBefore(int wid)
        {
            if (!Prefs.hierarchyOverrideMainIcon) return;
            if (Event.current.type != EventType.Layout) return;

            List<int> keysForRemove = new List<int>();
            foreach (KeyValuePair<int, CacheItem> pair in bestIconCache)
            {
                if (!pair.Value.used)
                {
                    pair.Value.Dispose();
                    keysForRemove.Add(pair.Key);
                }
            }

            foreach (int key in keysForRemove) bestIconCache.Remove(key);

            foreach (KeyValuePair<int, CacheItem> pair in bestIconCache) pair.Value.used = false;

            EditorWindow w = EditorUtility.InstanceIDToObject(wid) as EditorWindow;
            if (w != null) HierarchyHelper.SetDefaultIconsSize(w);
        }

        internal class CacheItem
        {
            public bool isPrefab;
            public Texture texture;
            public int countComponents;
            public bool used;

            public void Dispose()
            {
                texture = null;
            } 
        }
    }
}