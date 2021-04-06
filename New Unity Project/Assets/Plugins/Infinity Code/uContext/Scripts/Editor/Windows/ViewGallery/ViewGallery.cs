/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using InfinityCode.uContext.Actions;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext.Windows
{
    [InitializeOnLoad]
    public partial class ViewGallery : EditorWindow
    {
        public delegate void DrawCamerasDelegate(ViewGallery gallery, float rowHeight, float maxLabelWidth, ref int offsetY, ref int row);

        public static Action<ViewGallery> OnCamerasMenuItem;
        public static DrawCamerasDelegate OnDrawCameras;
        public static Func<CameraStateItem[]> OnGetCameras;

        private static GUIStyle selectedStyle;
        public static bool isDirty = true;

        public CameraStateItem[] cameras;
        private ViewStateItem[] views;
        public int countCols;
        private int countRows;
        public float itemWidth;
        public float itemHeight;
        public Vector2 lastSize;
        public float offsetX;
        private int countTemporaryCameras;
        private int sceneViewsCount;

        static ViewGallery()
        {
            KeyManager.KeyBinding binding = KeyManager.AddBinding();
            binding.OnValidate += OnValidate;
            binding.OnInvoke += OnInvoke;
        }

        private void CacheItems()
        {
            DestroyTextures();

            ArrayList sceneViews = SceneView.sceneViews;
            if (sceneViews == null || sceneViews.Count == 0)
            {
                sceneViews = new ArrayList();
                sceneViews.Add(GetWindow<SceneView>());
            }

            InitItems(sceneViews);

            CalcItemSize();

            RenderItems();

            isDirty = false;
        }

        private void CalcItemSize()
        {
            Vector2 size = lastSize = position.size;
            size.x -= 20; // margin horizontal
            size.y -= 120; // margin vertical + labels height

            countCols = Mathf.Max(cameras.Length, views.Length);
            countRows = cameras.Length > 0 ? 2 : 1;
            itemWidth = size.x / countCols;
            itemHeight = itemWidth * 0.75f; // 4:3

            if (itemHeight * countRows < size.y)
            {
                for (int cols = countCols - 1; cols > 0; cols--)
                {
                    float w = size.x / cols;
                    float h = w * 0.75f;
                    int rows = Mathf.CeilToInt(cameras.Length / (float)cols) + Mathf.CeilToInt(views.Length / (float)cols);
                    if (w > itemWidth && h * rows < size.y)
                    {
                        itemWidth = w;
                        itemHeight = h;
                        countCols = cols;
                        countRows = rows;
                    }
                    else break;
                }

                itemHeight = Mathf.FloorToInt((itemHeight - 15) / 3) * 3;
                itemWidth = itemHeight / 3 * 4;
                
            }
            else
            {
                itemHeight = Mathf.FloorToInt(size.y / countRows / 3) * 3;
                itemWidth = itemHeight / 3 * 4;
            }

            offsetX = (lastSize.x - itemWidth * countCols) / (countCols + 1);
        }

        private static void CreateViewState(object userdata)
        {
            ViewStateItem vi = userdata as ViewStateItem;
            if (vi != null)
            {
                SceneViewActions.SaveViewState();
                return;
            }

            Camera cam;
            if (userdata is Camera) cam = userdata as Camera;
            else if (userdata is CameraStateItem) cam = (userdata as CameraStateItem).camera;
            else return;

            GameObject container = TemporaryContainer.GetContainer();
            if (container == null) return;

            string pattern = @"View State \((\d+)\)";

            int maxIndex = 1;
            ViewState[] viewStates = container.GetComponentsInChildren<ViewState>();
            for (int i = 0; i < viewStates.Length; i++)
            {
                string name = viewStates[i].gameObject.name;
                Match match = Regex.Match(name, pattern);
                if (match.Success)
                {
                    string strIndex = match.Groups[1].Value;
                    int index = int.Parse(strIndex);
                    if (index >= maxIndex) maxIndex = index + 1;
                }
            }

            string viewStateName = "View State (" + maxIndex + ")";
            InputDialog.Show("Enter title of View State", viewStateName, s =>
            {
                GameObject go = new GameObject(viewStateName);
                go.tag = "EditorOnly";
                ViewState viewState = go.AddComponent<ViewState>();

                Transform t = cam.transform;
                float dist = cam.orthographic ? 0 : 5 / Mathf.Tan((float) (SceneView.lastActiveSceneView.camera.fieldOfView * 0.5 * (Math.PI / 180.0)));
                viewState.pivot = t.position + t.forward * dist;
                viewState.rotation = t.rotation;
                viewState.size = 10;
                viewState.is2D = cam.orthographic;
                viewState.title = s;

                go.transform.SetParent(container.transform, true);
                uContextMenu.Close();
            });
        }

        private void DestroyTextures()
        {
            if (cameras != null)
            {
                foreach (CameraStateItem cam in cameras)
                {
                    if (cam.texture != null) DestroyImmediate(cam.texture);
                }
            }

            if (views != null)
            {
                foreach (ViewStateItem view in views)
                {
                    if (view.texture != null) DestroyImmediate(view.texture);
                }
            }
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (countTemporaryCameras > 0)
            {
                if (OnCamerasMenuItem != null) OnCamerasMenuItem(this);
            }

            if (GUILayout.Button("View States", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
            {
                GenericMenu menu = new GenericMenu();

                menu.AddItem(new GUIContent("Create/From Current View"), false, SceneViewActions.SaveViewState);

                if (views.Length > sceneViewsCount)
                {
                    menu.AddItem(new GUIContent("Remove/All View States"), false, RemoveAllViewStates);
                    menu.AddSeparator("Remove/");

                    for (int i = sceneViewsCount; i < views.Length; i++)
                    {
                        ViewStateItem v = views[i];
                        menu.AddItem(new GUIContent("Remove/" + v.title), false, RemoveViewState, v);
                    }
                }

                menu.ShowAsContext();
            }

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false))) isDirty = true;
            EditorGUILayout.Space();

            EditorGUILayout.EndHorizontal();
        }

        private void InitItems(ArrayList sceneViews)
        {
            if (OnGetCameras == null) cameras = new CameraStateItem[0];
            else cameras = OnGetCameras();
            countTemporaryCameras = cameras.Count(c => c.camera.GetComponentInParent<TemporaryContainer>() != null);

            ViewState[] viewStates = FindObjectsOfType<ViewState>().OrderBy(v => v.gameObject.name).ToArray();

            sceneViewsCount = sceneViews.Count;
            views = new ViewStateItem[viewStates.Length + sceneViewsCount];
            for (int i = 0; i < sceneViewsCount; i++)
            {
                SceneView sceneView = sceneViews[i] as SceneView;
                string t = "Scene View";
                if (sceneViewsCount > 1) t += " " + (i + 1);
                views[i] = new ViewStateItem
                {
                    title = t,
                    pivot = sceneView.pivot,
                    size = sceneView.size,
                    rotation = sceneView.rotation,
                    is2D = sceneView.in2DMode,
                    view = sceneView
                };
            }
            
            for (int i = 0; i < viewStates.Length; i++) views[i + sceneViewsCount] = new ViewStateItem(viewStates[i]);
        }

        private void OnDestroy()
        {
            isDirty = true;
            DestroyTextures(); 
        }

        private void OnEnable()
        {
            isDirty = true;
        }

        private void OnFocus()
        {
            isDirty = true;
        }

        private void OnGUI()
        {
            if (selectedStyle == null)
            {
                selectedStyle = new GUIStyle(Styles.selectedRow);
                selectedStyle.fixedHeight = 0;
            }

            if (position.size != lastSize) isDirty = true;
            else if (cameras == null || views == null) isDirty = true;
            else if (cameras.Any(c => c == null) || views.Any(v => v == null)) isDirty = true;

            if (isDirty) CacheItems();

            DrawToolbar();

            int offsetY = 25;
            int row = 0;
            float rowHeight = itemHeight + 25;
            float maxLabelWidth = lastSize.x / countCols - 10;

            if (OnDrawCameras != null) OnDrawCameras(this, rowHeight, maxLabelWidth, ref offsetY, ref row);

            GUI.Label(new Rect(new Vector2(offsetX, row * rowHeight + offsetY), new Vector2(lastSize.x, 20)), "View States:", EditorStyles.boldLabel);
            offsetY += 20;

            for (int i = 0; i < views.Length; i++)
            {
                int col = i % countCols;

                float x = col * itemWidth + (col + 1) * offsetX;
                Rect rect = new Rect(x, row * rowHeight + offsetY, itemWidth, itemHeight);
                ViewStateItem view = views[i];
                if (view == null) continue;
                if (view.Draw(rect, maxLabelWidth))
                {
                    SceneView sceneView = SceneView.lastActiveSceneView;
                    sceneView.in2DMode = view.is2D;
                    sceneView.pivot = view.pivot;
                    sceneView.size = view.size;
                    if (!view.is2D)
                    {
                        sceneView.rotation = view.rotation;
                        sceneView.camera.fieldOfView = 60;
                    }
                    GetWindow<SceneView>();
                    GUI.changed = true;
                }

                if (i != views.Length - 1 && col == countCols - 1) row++;
            }

            Repaint();
        }

        private void OnHierarchyChange()
        {
            isDirty = true;
        }

        private static void OnInvoke()
        {
            OpenWindow();
        }

        private static bool OnValidate()
        {
            if (!Prefs.viewGalleryHotKey) return false;

            Event e = Event.current;
            if (e.modifiers != Prefs.viewGalleryModifiers) return false;
            if (e.keyCode != Prefs.viewGalleryKeyCode) return false;
            return true;
        }

        [MenuItem(WindowsHelper.MenuPath + "View Gallery", false, 102)]
        public static void OpenWindow()
        {
            GetWindow<ViewGallery>(false, "View Gallery", true);
        }

        private void RemoveAllViewStates()
        {
            if (!EditorUtility.DisplayDialog(
                "Confirmation", 
                "Are you sure you want to remove all View States?", 
                "Remove", "Cancel")) return;

            GameObject container = TemporaryContainer.GetContainer();
            if (container == null) return;

            ViewState[] viewStates = container.GetComponentsInChildren<ViewState>();
            for (int i = 0; i < viewStates.Length; i++) DestroyImmediate(viewStates[i].gameObject);

            isDirty = true;
        }

        private static void RemoveViewState(object userdata)
        {
            ViewStateItem item = userdata as ViewStateItem;

            if (!EditorUtility.DisplayDialog(
                "Confirmation",
                "Are you sure you want to remove " + item.title + "?",
                "Remove", "Cancel")) return;

            DestroyImmediate(item.viewState.gameObject);
            isDirty = true;
        }

        private static void RenameViewState(object userdata)
        {
            ViewStateItem item = userdata as ViewStateItem;
            InputDialog.Show("Rename View State", item.title, delegate(string s)
            {
                item.viewState.title = item.title = s;
            });
        }

        private void RenderItems()
        {
            RenderTexture renderTexture = new RenderTexture((int) itemWidth, (int) itemHeight, 16, RenderTextureFormat.ARGB32);
            RenderTexture.active = renderTexture;
            RenderTexture lastAT;
            CameraClearFlags clearFlags;

            if (cameras != null)
            {
                for (int i = 0; i < cameras.Length; i++)
                {
                    CameraStateItem camera = cameras[i];
                    if (camera.texture != null) DestroyImmediate(camera.texture);
                }
            }

            for (int i = 0; i < cameras.Length; i++)
            {
                CameraStateItem cameraState = cameras[i];
                if (cameraState == null || cameraState.camera == null) continue;
                try
                {
                    Camera camera = cameraState.camera;
                    clearFlags = camera.clearFlags;
                    if (clearFlags == CameraClearFlags.Depth || clearFlags == CameraClearFlags.Nothing) camera.clearFlags = CameraClearFlags.Skybox;
                    lastAT = camera.targetTexture;
                    camera.targetTexture = renderTexture;
                    camera.Render();

                    Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
                    texture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
                    texture.Apply();
                    cameraState.texture = texture;

                    camera.targetTexture = lastAT;
                    camera.clearFlags = clearFlags;
                }
                catch (Exception e)
                {
                    Log.Add(e);
                }
            }

            if (views != null)
            {
                for (int i = 0; i < views.Length; i++)
                {
                    ViewStateItem item = views[i];
                    if (item.texture != null) DestroyImmediate(item.texture);
                }
            }

            SceneView sceneView = views[0].view;
            Camera cam = sceneView.camera;
            lastAT = cam.activeTexture;
            clearFlags = cam.clearFlags;
            float nearClipPlane = cam.nearClipPlane;
            cam.nearClipPlane = 0.01f;
            if (clearFlags == CameraClearFlags.Depth || clearFlags == CameraClearFlags.Nothing) cam.clearFlags = CameraClearFlags.Skybox;
            cam.targetTexture = renderTexture;

            for (int i = 0; i < views.Length; i++)
            {
                views[i].SetView(sceneView);
                cam.Render();

                Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
                texture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
                texture.Apply();
                views[i].texture = texture;
            }

            views[0].SetView(sceneView);

            cam.targetTexture = lastAT;
            cam.clearFlags = clearFlags;
            cam.nearClipPlane = nearClipPlane;

            RenderTexture.active = null;
            renderTexture.Release();
            DestroyImmediate(renderTexture);
        }

        private static void RestoreViewState(object userdata)
        {
            ViewStateItem viewItem = userdata as ViewStateItem;
            SceneView sceneView = SceneView.lastActiveSceneView;
            sceneView.in2DMode = viewItem.is2D;
            sceneView.pivot = viewItem.pivot;
            sceneView.size = viewItem.size;

            if (!viewItem.is2D)
            {
                sceneView.rotation = viewItem.rotation;
                sceneView.camera.fieldOfView = 60;
            }

            GetWindow<SceneView>();
        }
    }
}