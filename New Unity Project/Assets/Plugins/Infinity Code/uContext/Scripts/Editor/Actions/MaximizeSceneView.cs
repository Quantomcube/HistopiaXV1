/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext.Actions
{
    public class MaximizeSceneView : ActionItem, IValidatableLayoutItem
    {
        public override float order
        {
            get { return 900; }
        }

        protected override void Init()
        {
            bool maximized;
            if (EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.GetType() == EditorTypes.gameView) maximized = EditorWindow.focusedWindow.maximized;
            else maximized = SceneView.lastActiveSceneView.maximized;

            if (maximized)
            {
                guiContent = new GUIContent(Icons.minimize, "Minimize Active Window");
            }
            else
            {
                guiContent = new GUIContent(Icons.maximize, "Maximize Active Window");
            }
        }

        public override void Invoke()
        {
            try
            {
                if (uContextMenu.lastWindow != null && uContextMenu.lastWindow.GetType() == EditorTypes.gameView)
                {
                    uContextMenu.lastWindow.maximized = !uContextMenu.lastWindow.maximized;
                }
                else SceneView.lastActiveSceneView.maximized = !SceneView.lastActiveSceneView.maximized;
            }
            catch
            {
                
            }
        }

        public bool Validate()
        {
            return SceneView.lastActiveSceneView != null || (EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.GetType() == EditorTypes.gameView);
        }
    }
}