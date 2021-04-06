/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using InfinityCode.uContext.Windows;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.uContext.Tools
{
    [InitializeOnLoad]
    public static class ObjectPlacer
    {
        private static Vector3 lastWorldPosition;
        private static GameObject parent;

        static ObjectPlacer()
        {
            SceneViewManager.AddListener(Invoke);
        }

        public static string GetHelpMessage()
        {
#if !UNITY_EDITOR_OSX
            string rootKey = "CTRL";
#else
            string rootKey = "CMD";
#endif

            string helpMessage = $"Hold {rootKey} to create an object at the root of the scene.\nHold SHIFT to create an object without alignment.";
            return helpMessage;
        }

        private static void Invoke(SceneView sceneView)
        {
            Event e = Event.current;

            if (e.type != EventType.MouseDown) return;
            if (e.button != 1) return;
            if (e.modifiers != Prefs.objectPlacerModifiers) return;

            Waila.Close();
            CreateBrowser wnd = CreateBrowser.OpenWindow();
            wnd.OnClose += OnCreateBrowserClose;
            wnd.OnSelectCreate += OnSelectCreate;
            wnd.OnSelectPrefab += OnBrowserPrefab;
            wnd.helpMessage = GetHelpMessage();
            lastWorldPosition = SceneViewManager.lastWorldPosition;
            parent = SceneViewManager.lastGameObjectUnderCursor;

            e.Use();
        }

        private static void OnBrowserPrefab(string assetPath)
        {
            GameObject go = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(assetPath)) as GameObject;
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            PlaceObject(go);
        }

        private static void OnCreateBrowserClose(CreateBrowser browser)
        {
            browser.OnClose = null;
            browser.OnSelectCreate = null;
            browser.OnSelectPrefab = null;
        }

        private static void OnSelectCreate(string menuItem)
        {
            GameObject go = Selection.activeGameObject;
            EditorApplication.ExecuteMenuItem(menuItem);
            if (go != Selection.activeGameObject) PlaceObject(Selection.activeGameObject);
        }

        private static void PlaceObject(GameObject go)
        {
            if (go == null) return;

            if (go.GetComponent<Camera>() != null)
            {
                if (Object.FindObjectsOfType<AudioListener>().Length > 1)
                {
                    AudioListener audioListener = go.GetComponent<AudioListener>();
                    if (audioListener != null) Object.DestroyImmediate(audioListener);
                }

                parent = null;
            }

            Event e = Event.current;

            RectTransform rectTransform = go.GetComponent<RectTransform>();
            Vector2 sizeDelta = rectTransform != null? rectTransform.sizeDelta: Vector2.zero;

            if (((e.modifiers & EventModifiers.Control) == 0 && (e.modifiers & EventModifiers.Command) == 0) && parent != null)
            {
                go.transform.SetParent(parent.transform);
            }
            bool allowDown = true;
            bool useCanvas = parent != null && parent.GetComponent<RectTransform>() != null;
            bool hasRectTransform = rectTransform != null;

            if (useCanvas || hasRectTransform) allowDown = false;

            go.transform.position = lastWorldPosition;
            if (allowDown && (e.modifiers & EventModifiers.Shift) == 0)
            {
                Collider c = go.GetComponent<Collider>();
                if (c != null) go.transform.Translate(0, c.bounds.extents.y, 0);
                else
                {
                    Renderer r = go.GetComponent<Renderer>();
                    if (r != null) go.transform.Translate(0, r.bounds.extents.y, 0);
                }
            }
            else if (!useCanvas && hasRectTransform)
            {
                Canvas canvas = CanvasUtils.GetCanvas();
                go.transform.SetParent(canvas.transform);
                go.transform.localPosition = Vector3.zero;
                useCanvas = true;
            }

            if (useCanvas && rectTransform != null)
            {
                Vector3 pos = rectTransform.localPosition;
                if (Math.Abs(rectTransform.anchorMin.x) < float.Epsilon && Math.Abs(rectTransform.anchorMax.x - 1) < float.Epsilon) pos.x = 0;
                if (Math.Abs(rectTransform.anchorMin.y) < float.Epsilon && Math.Abs(rectTransform.anchorMax.y - 1) < float.Epsilon) pos.y = 0;

                rectTransform.localPosition = pos;
                rectTransform.sizeDelta = sizeDelta;
            }

            Selection.activeGameObject = go;
        }
    }
}