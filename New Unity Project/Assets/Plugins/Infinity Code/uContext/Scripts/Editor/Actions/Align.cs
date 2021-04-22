/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.uContext.Attributes;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext.Actions
{
    [RequireMultipleGameObjects]
    public class Align : ActionItem
    {
        protected override bool closeOnSelect
        {
            get { return false; }
        }

        public override float order
        {
            get { return -900; }
        }

        private static void AlignSelection(int side, float x, float y, float z)
        {
            GameObjectUtils.Align(Selection.gameObjects, side, x, y, z);
            uContextMenu.Close();
        }

        private static void Distribute(float x, float y, float z)
        {
            GameObjectUtils.Distribute(Selection.gameObjects, x, y, z);
            uContextMenu.Close();
        }

        protected override void Init()
        {
            guiContent = new GUIContent(Icons.align, "Align & Distribute");
        }

        public override void Invoke()
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Align/X/Min"), false, () => AlignSelection(0, 1, 0, 0));
            menu.AddItem(new GUIContent("Align/X/Center"), false, () => AlignSelection(1, 1, 0, 0));
            menu.AddItem(new GUIContent("Align/X/Max"), false, () => AlignSelection(2, 1, 0, 0));

            menu.AddItem(new GUIContent("Align/Y/Min"), false, () => AlignSelection(0, 0, 1, 0));
            menu.AddItem(new GUIContent("Align/Y/Center"), false, () => AlignSelection(1, 0, 1, 0));
            menu.AddItem(new GUIContent("Align/Y/Max"), false, () => AlignSelection(2, 0, 1, 0));

            menu.AddItem(new GUIContent("Align/Z/Min"), false, () => AlignSelection(0, 0, 0, 1));
            menu.AddItem(new GUIContent("Align/Z/Center"), false, () => AlignSelection(1, 0, 0, 1));
            menu.AddItem(new GUIContent("Align/Z/Max"), false, () => AlignSelection(2, 0, 0, 1));

            if (Selection.gameObjects.Length > 2)
            {
                menu.AddItem(new GUIContent("Distribute/X"), false, () => Distribute(1, 0, 0));
                menu.AddItem(new GUIContent("Distribute/Y"), false, () => Distribute(0, 1, 0));
                menu.AddItem(new GUIContent("Distribute/Z"), false, () => Distribute(0, 0, 1));
            }

            menu.ShowAsContext();
        }
    }
}