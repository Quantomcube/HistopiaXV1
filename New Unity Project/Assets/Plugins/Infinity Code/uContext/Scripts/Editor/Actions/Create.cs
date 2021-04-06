/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.uContext.Tools;
using InfinityCode.uContext.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext.Actions
{
    public class Create : ActionItem
    {
        private int type; // 0 - Root, 1 - Child, 2 - Sibling, 3 - Parent, 4 - Temporary

        protected override bool closeOnSelect
        {
            get { return false; }
        }

        public override float order
        {
            get { return -1000; }
        }

        protected override void Init()
        {
            guiContent = new GUIContent(Icons.createObject, "Create Object");
        }

        private void ApplyType()
        {
            GameObject activeGameObject = Selection.activeGameObject;
            if (targets == null || targets.Length == 0 || targets[0] == null || activeGameObject == targets[0]) return;

            Transform t = targets[0].transform;

            if (type == 1)
            {
                activeGameObject.transform.SetParent(t, false);
                activeGameObject.transform.localPosition = Vector3.zero;
                activeGameObject.transform.localRotation = Quaternion.identity;
                activeGameObject.transform.localScale = Vector3.one;
            }
            else if (type == 2)
            {
                activeGameObject.transform.SetParent(t.parent, false);
                activeGameObject.transform.SetSiblingIndex(t.GetSiblingIndex() + 1);
                activeGameObject.transform.localPosition = Vector3.zero;
                activeGameObject.transform.localRotation = Quaternion.identity;
                activeGameObject.transform.localScale = Vector3.one;
            }
            else if (type == 3)
            {
                Transform parent = t.parent;
                if (parent != null) activeGameObject.transform.SetParent(parent);

                activeGameObject.transform.position = t.position;
                t.SetParent(activeGameObject.transform, true);
            }
            else if (type == 4)
            {
                GameObject parent = TemporaryContainer.GetContainer();
                if (parent == null || activeGameObject == null || activeGameObject == parent) return;

                activeGameObject.transform.SetParent(parent.transform, false);
                activeGameObject.tag = "EditorOnly";
            }
        }

        public override void Invoke()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Root"), false, () => ShowCreateBrowser(0));
            if (targets.Length == 1 && targets[0] != null)
            {
                menu.AddItem(new GUIContent("Child"), false, () => ShowCreateBrowser(1));
                if (targets[0].transform.parent != null) menu.AddItem(new GUIContent("Sibling"), false, () => ShowCreateBrowser(2));
                menu.AddItem(new GUIContent("Parent"), false, () => ShowCreateBrowser(3));
            }
            menu.AddItem(new GUIContent("Temporary"), false, () => ShowCreateBrowser(4));
            menu.ShowAsContext();
        }

        private void OnBrowserPrefab(string assetPath)
        {
            Selection.activeGameObject = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(assetPath)) as GameObject;
            ApplyType();
        }

        private void OnBrowserCreate(string menuItem)
        {
            EditorApplication.ExecuteMenuItem(menuItem);
            ApplyType();
        }

        private void OnBrowserClose(CreateBrowser browser)
        {
            browser.OnClose -= OnBrowserClose;
            browser.OnSelectCreate -= OnBrowserCreate;
            browser.OnSelectPrefab -= OnBrowserPrefab;
        }

        private void ShowCreateBrowser(int type)
        {
            uContextMenu.Close();

            this.type = type;
            CreateBrowser browser = CreateBrowser.OpenWindow();
            browser.OnClose += OnBrowserClose;
            browser.OnSelectCreate += OnBrowserCreate;
            browser.OnSelectPrefab += OnBrowserPrefab;

            if (type == 1)
            {
                browser.createLabel = "Create Child Item";
                browser.prefabsLabel = "Instantiate Child Prefab";
            }
            else if (type == 2)
            {
                browser.createLabel = "Create Sibling Item";
                browser.prefabsLabel = "Instantiate Sibling Prefab";
            }
            else if (type == 3)
            {
                browser.createLabel = "Create Parent Item";
                browser.prefabsLabel = "Instantiate Parent Prefab";
            }
            else if (type == 4)
            {
                browser.createLabel = "Create Temporary Item";
                browser.prefabsLabel = "Instantiate Temporary Prefab";
            }
        }
    }
}