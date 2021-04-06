/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Linq;
using System.Text.RegularExpressions;
using InfinityCode.uContext.Windows;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.uContext.Actions
{
    public class SceneViewActions : ActionItem
    {
        public static Action<GenericMenu> OnViewStateCreateFromSelection;

        protected override bool closeOnSelect
        {
            get { return false; }
        }

        private void AlignViewToCamera(object userdata)
        {
            SceneViewHelper.AlignViewToCamera(userdata as Camera);
            uContextMenu.Close();
        }

        private void AlignViewToSelected()
        {
            SceneView.lastActiveSceneView.AlignViewToObject(targets[0].transform);
            uContextMenu.Close();
        }

        private void AlignWithView()
        {
            SceneView.lastActiveSceneView.AlignWithView();
            uContextMenu.Close();
        }

        private static Camera CreateCameraFromSceneView()
        {
            if (!EditorApplication.ExecuteMenuItem("GameObject/Camera")) return null;

            Camera sceneViewCamera = SceneView.lastActiveSceneView.camera;
            Camera camera = Selection.activeGameObject.GetComponent<Camera>();
            camera.transform.position = sceneViewCamera.transform.position;
            camera.transform.rotation = sceneViewCamera.transform.rotation;
            return camera;
        }

        [MenuItem(WindowsHelper.MenuPath + "Cameras/Create Permanent", false, 101)]
        private static void CreatePermanentCameraFromSceneView()
        {
            CreateCameraFromSceneView();
        }

        [MenuItem(WindowsHelper.MenuPath + "Cameras/Create Temporary", false, 101)]
        private static void CreateTemporaryCameraFromSceneView()
        {
            GameObject container = TemporaryContainer.GetContainer();
            if (container == null) return;

            string pattern = @"Camera \((\d+)\)";

            int maxIndex = 1;
            Camera[] cameras = container.GetComponentsInChildren<Camera>();
            for (int i = 0; i < cameras.Length; i++)
            {
                string name = cameras[i].gameObject.name;
                Match match = Regex.Match(name, pattern);
                if (match.Success)
                {
                    string strIndex = match.Groups[1].Value;
                    int index = Int32.Parse(strIndex);
                    if (index >= maxIndex) maxIndex = index + 1;
                }
            }

            string defaultName = "Camera (" + maxIndex + ")";
            InputDialog.Show("Enter name of Camera GameObject", defaultName, s =>
            {
                Camera camera = CreateCameraFromSceneView();
                if (camera == null) return;

                camera.gameObject.name = !string.IsNullOrEmpty(s) ? s : defaultName;
                camera.farClipPlane = Mathf.Max(camera.farClipPlane, SceneView.lastActiveSceneView.size * 2);

                camera.transform.SetParent(container.transform, true);
                camera.tag = "EditorOnly";
            });
        }

        private void DeleteAllViewStates()
        {
            ViewState[] views = Object.FindObjectsOfType<ViewState>();
            for (int i = 0; i < views.Length; i++) Object.DestroyImmediate(views[i].gameObject);
            uContextMenu.Close();
        }

        private void DeleteViewState(object userdata)
        {
            Object.DestroyImmediate((userdata as ViewState).gameObject);
            uContextMenu.Close();
        }

        private void FrameSelected()
        {
            SceneView.FrameLastActiveSceneView();
            uContextMenu.Close();
        }

        protected override void Init()
        {
            guiContent = new GUIContent(Icons.focus, "Views and Cameras");
        }

        private void InitCreateCameraFromViewMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Create Camera From View/Permanent"), false, CreatePermanentCameraFromSceneView);
            menu.AddItem(new GUIContent("Create Camera From View/Temporary"), false, CreateTemporaryCameraFromSceneView);
        }

        private void InitAlignViewToCameraMenu(GenericMenu menu)
        {
            Camera[] cameras = Object.FindObjectsOfType<Camera>().OrderBy(c => c.name).ToArray();
            if (cameras.Length > 0)
            {
                for (int i = 0; i < cameras.Length; i++)
                {
                    menu.AddItem(new GUIContent("Align View To Camera/" + cameras[i].gameObject.name), false, AlignViewToCamera, cameras[i]);
                }
            }
        }

        private void InitViewStatesMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("View States/Gallery"), false, ViewGallery.OpenWindow);
            menu.AddSeparator("View States/");
            menu.AddItem(new GUIContent("View States/Create"), false, SaveViewState);

            if (OnViewStateCreateFromSelection != null) OnViewStateCreateFromSelection(menu);

            ViewState[] views = Object.FindObjectsOfType<ViewState>().OrderBy(v => v.title).ToArray();
            if (views.Length > 0)
            {
                for (int i = 0; i < views.Length; i++)
                {
                    menu.AddItem(new GUIContent("View States/Restore/" + views[i].title), false, RestoreViewState, views[i]);

                    if (i == 0)
                    {
                        menu.AddItem(new GUIContent("View States/Delete/All States"), false, DeleteAllViewStates);
                        menu.AddSeparator("View States/Delete/");
                    }
                    menu.AddItem(new GUIContent("View States/Delete/" + views[i].title), false, DeleteViewState, views[i]);
                }
            }
        }

        public override void Invoke()
        {
            GenericMenu menu = new GenericMenu();

            InitCreateCameraFromViewMenu(menu);
            InitAlignViewToCameraMenu(menu);
            InitViewStatesMenu(menu);

            if (targets != null && targets.Length > 0 && targets[0] != null)
            {
                menu.AddItem(new GUIContent("Frame Selected"), false, FrameSelected);
                menu.AddItem(new GUIContent("Move To View"), false, MoveToView);
                menu.AddItem(new GUIContent("Align With View"), false, AlignWithView);
                menu.AddItem(new GUIContent("Align View To Selected"), false, AlignViewToSelected);
            }

            menu.ShowAsContext();
        }

        private void MoveToView()
        {
            SceneView.lastActiveSceneView.MoveToView();
            uContextMenu.Close();
        }

        private void RestoreViewState(object userdata)
        {
            ViewState state = userdata as ViewState;
            SceneView view = SceneView.lastActiveSceneView;
            view.in2DMode = state.is2D;
            view.pivot = state.pivot;
            view.size = state.size;
            if (!view.in2DMode) view.rotation = state.rotation;
            uContextMenu.Close();
        }

        [MenuItem(WindowsHelper.MenuPath + "View States/Create", false, 104)]
        public static void SaveViewState()
        {
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

                SceneView view = SceneView.lastActiveSceneView;
                viewState.pivot = view.pivot;
                viewState.rotation = view.rotation;
                viewState.size = view.size;
                viewState.is2D = view.in2DMode;
                viewState.title = s;

                go.transform.SetParent(container.transform, true);
                uContextMenu.Close();
            });
        }
    }
}