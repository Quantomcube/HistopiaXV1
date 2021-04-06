/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext.Windows
{
    public partial class ViewGallery
    {
        internal class ViewStateItem: ViewItem
        {
            public Vector3 pivot;
            public float size;
            public Quaternion rotation;
            public string title;
            public ViewState viewState;
            public bool is2D;
            public SceneView view;

            public override bool useInPreview
            {
                get
                {
                    if (viewState != null) return viewState.useInPreview;
                    return false;
                }
                set
                {
                    if (viewState != null) viewState.useInPreview = value;
                }
            }

            public override bool allowPreview
            {
                get { return viewState != null; }
            }

            public override string name
            {
                get { return title; }
            }

            public ViewStateItem()
            {

            }

            public ViewStateItem(ViewState viewState)
            {
                this.viewState = viewState;
                pivot = viewState.pivot;
                size = viewState.size;
                rotation = viewState.rotation;
                title = viewState.title;
                is2D = viewState.is2D;
            }

            public override void PrepareMenu(GenericMenu menu)
            {
                menu.AddItem(new GUIContent("Restore"), false, RestoreViewState, this);

                if (viewState != null)
                {
                    menu.AddItem(new GUIContent("Rename"), false, RenameViewState, this);
                    menu.AddItem(new GUIContent("Delete"), false, RemoveViewState, this);
                }
                else menu.AddDisabledItem(new GUIContent("Delete"));

                if (viewState == null) menu.AddItem(new GUIContent("Create View State"), false, CreateViewState, this);
            }

            public void SetView(SceneView view)
            {
                Camera camera = view.camera;
                Transform t = camera.transform;

                if (!is2D)
                {
                    camera.orthographic = false;
                    camera.fieldOfView = 60;
                    t.position = pivot - rotation * Vector3.forward * ViewState.GetPerspectiveCameraDistance(size, 60);
                    t.rotation = rotation;
                }
                else
                {
                    camera.orthographic = true;
                    camera.orthographicSize = size;
                    t.position = pivot - Vector3.forward * size;
                    t.rotation = Quaternion.identity;
                }
            }
        }
    }
}