/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext.Tools
{
    [InitializeOnLoad]
    public static class TerrainBrushSize
    {
        private static bool isActive = false;
        private static Terrain terrain;
        private static PropertyInfo selectedToolProp;
        private static FieldInfo activeInstanceField;
        private static PropertyInfo treeBrushSizeField;
        private static Type paintTreeToolType;
        private static PropertyInfo paintTreeInstanceProp;

#if UNITY_2020_2_OR_NEWER
        private static PropertyInfo brushSizeProp;
#else
        private static FieldInfo sizeField;
#endif


        static TerrainBrushSize()
        {
            try
            {
                Selection.selectionChanged += OnSelectionChanged;
                activeInstanceField = EditorTypes.terrainInspector.GetField("s_activeTerrainInspectorInstance", BindingFlags.Static | BindingFlags.NonPublic);

#if UNITY_2020_2_OR_NEWER
                brushSizeProp = EditorTypes.terrainInspector.GetProperty("brushSize", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
#else
                sizeField = EditorTypes.terrainInspector.GetField("m_Size", BindingFlags.Instance | BindingFlags.NonPublic);
#endif

                selectedToolProp = EditorTypes.terrainInspector.GetProperty("selectedTool", BindingFlags.NonPublic | BindingFlags.Instance);

                paintTreeToolType = Reflection.GetEditorType("Experimental.TerrainAPI.PaintTreesTool");
                treeBrushSizeField = paintTreeToolType.GetProperty("brushSize", BindingFlags.Instance | BindingFlags.Public);
                Type ssType = typeof(ScriptableSingleton<>);
                Type[] typeArgs = { paintTreeToolType };
                paintTreeToolType = ssType.MakeGenericType(typeArgs);
                paintTreeInstanceProp = paintTreeToolType.GetProperty("instance", BindingFlags.Static | BindingFlags.Public);

                SceneViewManager.AddListener(OnSceneGUI);

                OnSelectionChanged();
            }
            catch (Exception e)
            {
                Log.Add(e);
            }
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (!isActive || terrain == null) return;
            if (Preview.isActive) return;

            Event e = Event.current;
            if (e.type != EventType.ScrollWheel || e.modifiers != Prefs.terrainBrushSizeModifiers && e.modifiers != Prefs.terrainBrushSizeBoostModifiers) return;

            Editor editor = activeInstanceField.GetValue(null) as Editor;
            if (editor == null) return;

            int value = (int) selectedToolProp.GetValue(editor, new object[0]);

            float size;
            object instance = paintTreeInstanceProp.GetValue(null, null);
            if (value == 2) size = (float)treeBrushSizeField.GetValue(instance, null);
            else
            {
#if UNITY_2020_2_OR_NEWER
                size = (float)brushSizeProp.GetValue(editor);
#else
                size = (float) sizeField.GetValue(editor);
#endif
            }

            float delta = e.delta.y;
            if (e.modifiers == Prefs.terrainBrushSizeBoostModifiers) delta *= 10;
            size = Mathf.Clamp(size + delta, 0.1f, Mathf.Round(Mathf.Min(terrain.terrainData.size.x, terrain.terrainData.size.z) * 15f / 16f));

            if (value == 2) treeBrushSizeField.SetValue(instance, size, null);
            else
            {
#if UNITY_2020_2_OR_NEWER
                brushSizeProp.SetValue(editor, size);
#else
                sizeField.SetValue(editor, size);
#endif
            }

            e.Use();
        }

        private static void OnSelectionChanged()
        {
            if (!Prefs.terrainBrushSize) return;

            isActive = false;
            terrain = null;
            //SceneViewManager.RemoveListener(OnSceneGUI);

            if (Selection.activeGameObject == null) return;
            GameObject go = Selection.activeGameObject;
            terrain = go.GetComponent<Terrain>();
            if (terrain == null) return;

            //SceneViewManager.AddListener(OnSceneGUI);
            isActive = true;
        }
    }
}