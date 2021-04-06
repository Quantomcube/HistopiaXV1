/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext
{
    public static class SceneViewHelper
    {
        public static void AlignViewToCamera(Camera camera)
        {
            if (camera == null) return;

            SceneView view = SceneView.lastActiveSceneView;
            Transform t = camera.transform;
            view.in2DMode = false;
            view.AlignViewToObject(t);
        }
    }
}