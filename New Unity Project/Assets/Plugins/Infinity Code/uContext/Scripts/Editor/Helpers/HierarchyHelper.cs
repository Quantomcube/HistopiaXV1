/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InfinityCode.uContext
{
    public static class HierarchyHelper
    {
        private static FieldInfo sceneHierarchyWindowField;
        private static FieldInfo sceneHierarchyField;
        private static FieldInfo treeViewField;
        private static PropertyInfo guiProp;
        private static FieldInfo iconWidthField;
        private static FieldInfo spaceBetweenIconAndTextField;

        public static void ExpandHierarchy(EditorWindow window, GameObject selection)
        {
            if (selection != null)
            {
                Reflection.InvokeMethod(EditorTypes.sceneHierarchyWindow, "SetExpandedRecursive", window,
                    new[] { typeof(int), typeof(bool) },
                    new object[] { selection.GetInstanceID(), true });
            }
            else
            {
                object sceneHierarchy = Reflection.GetFieldValue(EditorTypes.sceneHierarchyWindow, "m_SceneHierarchy", window);
                Reflection.InvokeMethod(EditorTypes.sceneHierarchy, "SetScenesExpanded", sceneHierarchy, 
                    new[] { typeof(List<string>) }, 
                    new[] {new List<string> { SceneManager.GetActiveScene().name }});
            }
        }

        public static EditorWindow GetLastHierarchyWindow()
        {
            if (sceneHierarchyWindowField == null) sceneHierarchyWindowField = EditorTypes.sceneHierarchyWindow.GetField("s_LastInteractedHierarchy", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            object hierarchyWindow = sceneHierarchyWindowField.GetValue(null);
            return hierarchyWindow as EditorWindow;
        }

        public static void SetDefaultIconsSize(EditorWindow hierarchyWindow, int size = 0)
        {
            if (sceneHierarchyField == null) sceneHierarchyField = EditorTypes.sceneHierarchyWindow.GetField("m_SceneHierarchy", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            object sceneHierarchy = sceneHierarchyField.GetValue(hierarchyWindow);
            if (sceneHierarchy == null) return;

            if (treeViewField == null) treeViewField = EditorTypes.sceneHierarchy.GetField("m_TreeView", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            object treeView = treeViewField.GetValue(sceneHierarchy);
            if (treeView == null) return;

            if (guiProp == null) guiProp = EditorTypes.treeViewController.GetProperty("gui", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            object gui = guiProp.GetValue(treeView);
            if (gui == null) return;

            if (iconWidthField == null) iconWidthField = EditorTypes.treeViewGUI.GetField("k_IconWidth", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            iconWidthField.SetValue(gui, size);

            if (spaceBetweenIconAndTextField == null) spaceBetweenIconAndTextField = EditorTypes.treeViewGUI.GetField("k_SpaceBetweenIconAndText", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            spaceBetweenIconAndTextField.SetValue(gui, 18 - size);
        }
    }
}