/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;

namespace InfinityCode.uContext.FloatToolbar
{
    [InitializeOnLoad]
    public static class FloatToolbarManager
    {
        private static List<FloatToolbar> toolbars = new List<FloatToolbar>();
        private static Dictionary<int, Rect> sizes;
        private static PrefabStage lastPrefabStage;

        static FloatToolbarManager()
        {
            sizes = new Dictionary<int, Rect>();
            SceneViewManager.AddListener(OnSceneViewGUI, 10000, true);
        }

        public static void Add(FloatToolbar toolbar)
        {
            toolbars.Add(toolbar);
        }

        private static void OnSceneViewGUI(SceneView sceneView)
        {
            if (toolbars == null || toolbars.Count == 0) return;

            bool sizeChanged = false;
            if (Event.current.type == EventType.Layout && DragAndDrop.objectReferences.Length == 0)
            {
                int id = sceneView.GetInstanceID();

                Rect rect;
                if (sizes.TryGetValue(id, out rect))
                {
                    if (sceneView.position != rect)
                    {
                        sizeChanged = true;
                        sizes[id] = sceneView.position;
                    }
                }
                else
                {
                    sizeChanged = true;
                    sizes[id] = sceneView.position;
                }

                PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                if (prefabStage != lastPrefabStage)
                {
                    sizeChanged = true;
                    lastPrefabStage = prefabStage;
                }
            }

            for (int i = toolbars.Count - 1; i >= 0; i--)
            {
                try
                {
                    FloatToolbar toolbar = toolbars[i];
                    if (toolbar == null) continue;

                    toolbar.isDirty = sizeChanged;
                    toolbar.OnSceneViewGUI(sceneView);
                }
                catch (Exception e)
                {
                    Log.Add(e);
                }
            }
        }

        public static void Remove(FloatToolbar toolbar)
        {
            toolbars.Remove(toolbar);
        }
    }
}