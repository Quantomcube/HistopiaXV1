/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext.Windows
{
    public partial class ViewGallery
    {
        public class CameraStateItem : ViewItem
        {
            public Camera camera;
            private bool _useInPreview = true;

            public override bool useInPreview
            {
                get { return _useInPreview; }
                set
                {
                    if (_useInPreview == value) return;

                    _useInPreview = value;
                    if (!value)
                    {
                        if (camera.gameObject.GetComponent<HideInPreview>() == null) camera.gameObject.AddComponent<HideInPreview>();
                    }
                    else DestroyImmediate(camera.gameObject.GetComponent<HideInPreview>());
                }
            }

            public override bool allowPreview
            {
                get { return true; }
            }

            public override string name
            {
                get
                {
                    if (camera != null) return camera.name;
                    return string.Empty;
                }
            }

            public CameraStateItem(Camera camera)
            {
                this.camera = camera;
                _useInPreview = camera.gameObject.GetComponent<HideInPreview>() == null;
            }

            public override void PrepareMenu(GenericMenu menu)
            {
                menu.AddItem(new GUIContent("Restore"), false, SetViewFromCamera);
                menu.AddItem(new GUIContent("Select"), false, () => Selection.activeGameObject = camera.gameObject);
                menu.AddItem(new GUIContent("Create View State"), false, CreateViewState, this);
            }

            private void SetViewFromCamera()
            {
                SceneViewHelper.AlignViewToCamera(camera);
                GetWindow<SceneView>();
                isDirty = true;
                GUI.changed = true;
            }
        }
    }
}