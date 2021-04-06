/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext.Windows
{
    public class ProFeaturesWindow : EditorWindow
    {
        private Vector2 scrollPosition;

        private void DrawGroup(string title, string[] features)
        {
            GUILayout.Label(title, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            foreach (string feature in features) EditorGUILayout.LabelField(feature);

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawGroup("Exclusive Features", new[] {"Replace", "Built-in Update System"});
            DrawGroup("Context Menu", new[] {"History Action", "Favorite Windows In Context Menu"});
            DrawGroup("Object Toolbar", new[] { "Auto Open Best Item", "Related Components" });
            DrawGroup("Quick Access Bar", new[] { "Menu Item Type", "Static Method Type", "Scriptable Objects Type",  });
            DrawGroup("Component Window", new[] { "Debug Mode" });
            DrawGroup("Hierarchy", new[] { "Icons Of Components", "Errors In Components", "Highlight Row On WAILA" });
            DrawGroup("WAILA", new[] { "Names Of All Object Under Cursor", "Smart Selection" });
            DrawGroup("Transform Editor Tools", new[] { "Align", "Bounds" });
            DrawGroup("View Gallery And View States", new[] { "Display Cameras In View Gallery", "View State For Selection", "View States In Scene View" });
            DrawGroup("Quick Preview", new[] { "Set View From Quick Preview" });
            DrawGroup("Drop To Floor", new[] { "Advanced Mode" });
            DrawGroup("Bookmarks", new[] { "Filter By Type" });
            DrawGroup("Rename", new[] { "Mass Rename" });
            DrawGroup("Jump To Point", new[] { "Alternative Mode" });
            DrawGroup("Distance Tool", new[] { "Distance To Cursor" });
            DrawGroup("Duplicate Tool", new[] { "Switch To Duplicate Tool By Shortcut" });

            EditorGUILayout.EndScrollView();

            EditorGUILayout.HelpBox("Details about all the features you can read in the documentation.", MessageType.Info);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Documentation")) Links.OpenDocumentation();
            if (GUILayout.Button("uContext Pro")) Links.OpenPro();
            EditorGUILayout.EndHorizontal();
        }

        [MenuItem(WindowsHelper.MenuPath + "Pro Features", false, 123)]
        public static void OpenWindow()
        {
            GetWindow<ProFeaturesWindow>(false, "uContext Pro Features");
        }
    }
}