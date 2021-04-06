/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext
{
    public static partial class Prefs
    {
        public static bool terrainBrushSize = true;
        public static bool terrainBrushSizeBoost = true;

#if !UNITY_EDITOR_OSX
        public static EventModifiers terrainBrushSizeModifiers = EventModifiers.Control;
        public static EventModifiers terrainBrushSizeBoostModifiers = EventModifiers.Control | EventModifiers.Shift;
#else
        public static EventModifiers terrainBrushSizeModifiers = EventModifiers.Command;
        public static EventModifiers terrainBrushSizeBoostModifiers = EventModifiers.Command | EventModifiers.Shift;
#endif

        private class TerrainBrushSizeManager : PrefManager, IHasShortcutPref
        {
            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Terrain Brush Size",
                        "Boost"
                    };
                }
            }

            public override float order
            {
                get { return -30; }
            }

            public override void Draw()
            {
                terrainBrushSize = EditorGUILayout.ToggleLeft("Terrain Brush Size", terrainBrushSize, EditorStyles.boldLabel);
                EditorGUI.indentLevel++;

                EditorGUI.BeginDisabledGroup(!terrainBrushSize);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Modifiers", GUILayout.Width(labelWidth));
                terrainBrushSizeModifiers = DrawModifiers(terrainBrushSizeModifiers);
                EditorGUILayout.EndHorizontal();

                DrawFieldWithModifiers("Boost", ref terrainBrushSizeBoost, ref terrainBrushSizeBoostModifiers);

                EditorGUI.indentLevel--;

                EditorGUI.EndDisabledGroup();
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                if (!terrainBrushSize) return new Shortcut[0];

                return new[]
                {
                    new Shortcut("Change size of Terrain Brush", "Scene View", terrainBrushSizeModifiers, "Mouse Wheel"),
                    new Shortcut("Fast Change size of Terrain Brush", "Scene View", terrainBrushSizeBoostModifiers, "Mouse Wheel"),
                };
            }
        }
    }
}