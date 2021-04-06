/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.uContext.Tools
{
    [InitializeOnLoad]
    public static class SelectionBoundsManager
    {
        public static Action OnChanged;
        private static Bounds _bounds;

        public static Bounds bounds
        {
            get { return _bounds; }
        }

        public static bool hasBounds { get; private set; }
        public static List<Renderer> renderers { get; }

        static SelectionBoundsManager()
        {
            renderers = new List<Renderer>();
            Selection.selectionChanged += OnSelectionChanged;

            SceneViewManager.AddListener(OnSceneView);

            OnSelectionChanged();
        }

        private static void OnSceneView(SceneView scene)
        {
            if (!hasBounds) return;
            if (renderers.Count == 0 || renderers[0] == null) return;

            Bounds b = renderers[0].bounds;

            for (int i = 1; i < renderers.Count; i++)
            {
                if (renderers[i] != null) b.Encapsulate(renderers[i].bounds);
            }

            if (b != _bounds)
            {
                _bounds = b;
                if (OnChanged != null) OnChanged();
            }
        }

        private static void OnSelectionChanged()
        {
            renderers.Clear();
            hasBounds = false;

            int[] instanceIDs = Selection.instanceIDs;

            bool isFirst = true;

            foreach (int instanceID in instanceIDs)
            {
                GameObject gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

                if (gameObject == null || gameObject.scene.name == null) continue;

                Renderer[] rs = gameObject.GetComponentsInChildren<Renderer>();
                if (rs == null || rs.Length == 0) continue;
                renderers.AddRange(rs);

                foreach (Renderer renderer in rs)
                {
                    if (isFirst)
                    {
                        _bounds = renderer.bounds;
                        isFirst = false;
                    }
                    else
                    {
                        _bounds.Encapsulate(renderer.bounds);
                    }
                }
            }

            if (!isFirst) hasBounds = true;
            if (OnChanged != null) OnChanged();
        }
    }
}