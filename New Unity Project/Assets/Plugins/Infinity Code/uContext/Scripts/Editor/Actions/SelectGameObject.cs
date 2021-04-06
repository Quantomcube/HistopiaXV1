/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext.Actions
{
    public class SelectGameObject : ActionItem
    {
        protected override bool closeOnSelect
        {
            get { return false; }
        }

        public override float order
        {
            get { return -950; }
        }

        protected override void Init()
        {
            guiContent = new GUIContent(Icons.hierarchy, "Select GameObject");
        }

        public override void Invoke()
        {
            List<GameObject> items = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().ToList();
            GenericMenu menu = new GenericMenu();

            for (int i = 0; i < items.Count; i++) GetChilds(items[i].transform, menu, "");

            menu.ShowAsContext();
        }

        private void GetChilds(Transform t, GenericMenu menu, string prefix)
        {
            string title = prefix + t.name;

            if (t.childCount > 0)
            {
                menu.AddItem(new GUIContent(title + "/Select"), Selection.activeGameObject == t.gameObject, () =>
                {
                    Selection.activeGameObject = t.gameObject;
                    SceneViewManager.OnNextGUI += uContextMenu.ShowInLastPosition;
                });
                menu.AddSeparator(title + "/");
                for (int i = 0; i < t.childCount; i++) GetChilds(t.GetChild(i), menu, title + "/");
            }
            else
            {
                menu.AddItem(new GUIContent(title), Selection.activeGameObject == t.gameObject, () =>
                {
                    Selection.activeGameObject = t.gameObject;
                    SceneViewManager.OnNextGUI += uContextMenu.ShowInLastPosition;
                });
            }
        }
    }
}