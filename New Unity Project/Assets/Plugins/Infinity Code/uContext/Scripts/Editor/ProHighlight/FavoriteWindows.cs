/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.uContext.PopupWindows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext.ProHighlight
{
#if !UCONTEXT_PRO
    public class FavoriteWindows : PopupWindowItem, IValidatableLayoutItem
    {
        private GUIContent labelContent;

        public override float order
        {
            get { return 95; }
        }

        protected override void CalcSize()
        {
            Vector2 s = EditorStyles.whiteLabel.CalcSize(labelContent);
            _size = s;
            _size.y += GUI.skin.label.margin.bottom;

            s = GUI.skin.button.CalcSize(new GUIContent("Available in uContext Pro"));
            _size.x = Mathf.Max(_size.x, s.x);
            _size.y += s.y + GUI.skin.button.margin.bottom;
        }

        public override void Dispose()
        {
            base.Dispose();

            labelContent = null;
        }

        public override void Draw()
        {
            EditorGUILayout.LabelField(labelContent, EditorStyles.whiteLabel);
            if (GUILayout.Button("Available in uContext Pro")) Links.OpenPro();
        }

        protected override void Init()
        {
            labelContent = new GUIContent("Favorite Windows:");
        }

        public bool Validate()
        {
            return Prefs.favoriteWindowsInContextMenu && Prefs.proHighlight;
        }
    }

#endif
}