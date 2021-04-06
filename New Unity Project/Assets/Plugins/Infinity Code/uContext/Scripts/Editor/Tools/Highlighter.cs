/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace InfinityCode.uContext.Tools
{
    [InitializeOnLoad]
    public static class Highlighter
    {
        private static GameObject hoveredGO;
        private static Dictionary<SceneView, SceneViewOutline> viewDict;
        private static Renderer[] lastRenderers;
        private static RectTransform lastRectTransform;
        private static Vector3[] rectTransformCorners;
        private static Transform lastNoRendererTransform; 

        public static GameObject lastGameObject { get; private set; }

        static Highlighter()
        {
            SceneViewManager.AddListener(OnSceneGUI, 0, true);
            HierarchyItemDrawer.Register("Highlighter", DrawHierarchyItem, 0);
            EditorApplication.modifierKeysChanged += RepaintAllHierarchies;
            EditorApplication.update += EditorUpdate;
            viewDict = new Dictionary<SceneView, SceneViewOutline>();
        }

        private static Rect DrawHierarchyItem(int id, Rect rect)
        {
            if (!Prefs.highlight) return rect;

            Event e = Event.current;
            GameObject go = EditorUtility.InstanceIDToObject(id) as GameObject;

            if (Prefs.highlightOnHierarchy) HighlightOnHierarchy(rect, e, go);

            return rect;
        }

        private static void EditorUpdate()
        {
            var wnd = EditorWindow.mouseOverWindow;
            if (wnd != null && wnd.GetType() == EditorTypes.sceneHierarchyWindow && !wnd.wantsMouseMove)
            {
                wnd.wantsMouseMove = true;
            }
        }

        public static bool Highlight(GameObject go)
        {
            bool state = false;
            if (HighlightUI(go)) state = true;
            else if (HighlightRenderers(go)) state = true;
            else if (HighlightWithoutRenderer(go)) state = true;

            lastGameObject = go;

            return state;
        }

        private static bool HighlightRenderers(GameObject go)
        {
            Renderer[] renderers = null;
            if (go != null) renderers = go.GetComponentsInChildren<Renderer>();

            if (lastGameObject == go) return false;

            lastRenderers = renderers;

            if (renderers != null && renderers.Length > 0)
            {
                foreach (SceneView view in SceneView.sceneViews)
                {
                    SceneViewOutline outline;
                    if (!viewDict.TryGetValue(view, out outline))
                    {
                        outline = view.camera.gameObject.AddComponent<SceneViewOutline>();
                        viewDict.Add(view, outline);
                    }

                    outline.SetRenderer(renderers);
                    view.Repaint();
                }
            }
            else
            {
                foreach (SceneView view in SceneView.sceneViews)
                {
                    SceneViewOutline outline;
                    if (!viewDict.TryGetValue(view, out outline))
                    {
                        outline = view.camera.gameObject.AddComponent<SceneViewOutline>();
                        viewDict.Add(view, outline);
                    }

                    outline.SetRenderer(null);
                    view.Repaint();
                }
            }

            return true;
        }

        private static void HighlightOnHierarchy(Rect rect, Event e, GameObject go)
        {
            switch (e.type)
            {
                case EventType.MouseDrag:
                case EventType.DragUpdated:
                case EventType.DragPerform:
                case EventType.DragExited:
                case EventType.MouseMove:
                    if (go == null) break;
                    bool contains = rect.Contains(e.mousePosition);
                    if (contains && e.modifiers == EventModifiers.Control)
                    {
                        if (hoveredGO != go)
                        {
                            hoveredGO = go;
                            Highlight(go);
                        }
                    }
                    else if (hoveredGO == go)
                    {
                        hoveredGO = null;
                        Highlight(null);
                    }

                    break;
            }
        }

        private static bool HighlightUI(GameObject go)
        {
            RectTransform rectTransform = null;
            if (go != null) rectTransform = go.GetComponent<RectTransform>();

            if (rectTransform == lastRectTransform) return false;

            lastRectTransform = rectTransform;
            return true;
        }

        private static bool HighlightWithoutRenderer(GameObject go)
        {
            Transform transform = null;
            if (go != null) transform = go.transform;

            if (lastNoRendererTransform == transform) return false;

            lastNoRendererTransform = transform;

            return true;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (lastRectTransform == null) return;
            if (rectTransformCorners == null) rectTransformCorners = new Vector3[4];
            lastRectTransform.GetWorldCorners(rectTransformCorners);

            Color color = Handles.color;
            Handles.color = Color.red;

#if UNITY_2020_2_OR_NEWER
            int thickness = 2;
            Handles.DrawLine(rectTransformCorners[0], rectTransformCorners[1], thickness);
            Handles.DrawLine(rectTransformCorners[1], rectTransformCorners[2], thickness);
            Handles.DrawLine(rectTransformCorners[2], rectTransformCorners[3], thickness);
            Handles.DrawLine(rectTransformCorners[3], rectTransformCorners[0], thickness);
#else
            Handles.DrawLine(rectTransformCorners[0], rectTransformCorners[1]);
            Handles.DrawLine(rectTransformCorners[1], rectTransformCorners[2]);
            Handles.DrawLine(rectTransformCorners[2], rectTransformCorners[3]);
            Handles.DrawLine(rectTransformCorners[3], rectTransformCorners[0]);
#endif
            Handles.color = color;
        }

        public static void RepaintAllHierarchies()
        {
            if (!Prefs.highlightHierarchyRow) return;

            Object[] windows = UnityEngine.Resources.FindObjectsOfTypeAll(EditorTypes.sceneHierarchyWindow);
            foreach (Object w in windows) (w as EditorWindow).Repaint();
        }


        [ExecuteInEditMode]
        public class SceneViewOutline : MonoBehaviour
        {
            private bool useRenderImage;
            private CommandBuffer _buffer;
            private Material _material;
            private Material _childMaterial;
            private Shader _shader;

            private CommandBuffer buffer
            {
                get
                {
                    if (_buffer == null) _buffer = new CommandBuffer();

                    return _buffer;
                }
            }

            private Material material
            {
                get
                {
                    if (_material == null) _material = new Material(shader);
                    return _material;
                }
            }

            private Shader shader
            {
                get
                {
                    if (_shader == null) _shader = Shader.Find("Hidden/InfinityCode/uContext/SceneViewHighlight");
                    return _shader;
                }
            }

            public void OnRenderImage(RenderTexture source, RenderTexture destination)
            {
                if (source == null || destination == null) return;
                CommandBuffer b = buffer;

                if (!useRenderImage)
                {
                    b.Clear();
                    Graphics.Blit(source, destination);
                    return;
                }

                int width = source.width;
                int height = source.height;
                RenderTexture t1 = RenderTexture.GetTemporary(width, height, 32);
                RenderTexture t2 = RenderTexture.GetTemporary(width, height, 32);

                RenderTexture.active = t1;
                GL.Clear(true, true, Color.clear);

                Graphics.ExecuteCommandBuffer(b);
                RenderTexture.active = null;

                Material m = material;

                Graphics.Blit(t1, t2, m, 1);
                Graphics.Blit(t2, t1, m, 2);

                m.SetTexture("_OutlineTex", t1);
                m.SetTexture("_FillTex", t2);
                m.SetColor("_Color", Prefs.highlightColor);
                Graphics.Blit(source, destination, m, 3);

                RenderTexture.ReleaseTemporary(t1);
                RenderTexture.ReleaseTemporary(t2);
            }

            public void SetRenderer(Renderer[] renderers)
            {
                CommandBuffer b = buffer;
                b.Clear();

                useRenderImage = renderers != null && renderers.Length > 0;
                if (!useRenderImage) return;

                Material m = material;

                int id = 1;
                foreach (Renderer r in renderers.OrderBy(r => SortingLayer.GetLayerValueFromID(r.sortingLayerID)).ThenBy(r => r.sortingOrder))
                {
                    int count = 1;
                    if (r is MeshRenderer)
                    {
                        MeshFilter f = r.GetComponent<MeshFilter>();
                        if (f != null && f.sharedMesh != null) count = f.sharedMesh.subMeshCount;
                    }
                    else 
                    {
                        SkinnedMeshRenderer smr = r as SkinnedMeshRenderer;
                        if (smr != null && smr.sharedMesh != null) count = smr.sharedMesh.subMeshCount;
                    }

                    Color32 objectID = new Color32((byte)(id & 0xff), (byte)((id >> 8) & 0xff), 0, 0);
                    b.SetGlobalColor("_ObjectID", objectID);
                    for (int i = 0; i < count; i++) b.DrawRenderer(r, m, i, 0);
                    id++;
                }
            }
        }
    }
}
