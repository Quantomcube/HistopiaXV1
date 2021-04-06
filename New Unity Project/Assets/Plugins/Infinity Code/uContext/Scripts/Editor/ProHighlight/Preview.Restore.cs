/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.uContext.Tools;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext.ProHighlight
{
#if !UCONTEXT_PRO
    [InitializeOnLoad]
    public static class PreviewRestore
    {
        static PreviewRestore()
        {
            KeyManager.KeyBinding focusBinding = KeyManager.AddBinding();
            focusBinding.OnValidate += () =>
            {
                if (!Prefs.preview) return false;
                if (SceneView.lastActiveSceneView == null) return false;
                if (Preview.texture == null) return false;
                if (Event.current.keyCode != KeyCode.F) return false;
                if (Event.current.modifiers != Prefs.previewModifiers) return false;
                return true;
            };
            focusBinding.OnInvoke += FocusActiveItem;

            Preview.OnPostSceneGUI += OnPostSceneGUI;
        }

        private static void FocusActiveItem()
        {
            if (EditorUtility.DisplayDialog("Warning", "Set view available in uContext Pro.", "Open uContext Pro", "Cancel"))
            {
                Links.OpenPro();
            }
        }

        private static void OnPostSceneGUI(float width, GUIStyle style, ref float lastY)
        {
            if (!Prefs.proHighlight) return;
            if (Preview.activeItem is Preview.CameraItem) GUI.Label(new Rect(5, 55, width, 20), "F - Set view (Available in uContext PRO)", style);
        }
    }
#endif
}