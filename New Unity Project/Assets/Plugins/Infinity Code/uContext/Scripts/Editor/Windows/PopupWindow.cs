/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext.Windows
{
    public abstract class PopupWindow : EditorWindow
    {
        protected static Texture2D background;

        protected virtual void OnGUI()
        {
            if (background == null)
            {
                background = new Texture2D(1, 1);
                background.SetPixel(0, 0, new Color(0.2f, 0.2f, 0.2f, 1));
                background.Apply();
            }

            GUI.DrawTexture(new Rect(0, 0, position.width, position.height), background, ScaleMode.StretchToFill);
        }
    }
}