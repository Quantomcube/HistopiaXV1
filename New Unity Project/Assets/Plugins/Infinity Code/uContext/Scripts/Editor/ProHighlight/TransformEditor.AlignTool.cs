/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Linq;
using InfinityCode.uContext.TransformEditorTools;
using InfinityCode.uContext.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext.ProHighlight
{
#if !UCONTEXT_PRO
    public class AlignTool : TransformEditorTool
    {
        private GUIStyle rightLabel;

        public override void Draw()
        {
            Transform[] transforms = TransformEditorWindow.GetTransforms();
            if (transforms.Length <= 1) return;

            if (rightLabel == null)
            {
                rightLabel = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleRight
                };
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Align", GUILayout.ExpandWidth(false));
            GUILayout.Label("Distribute", rightLabel);
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("X", GUILayout.ExpandWidth(false));
            if (GUILayout.Button("Min")) { }
            if (GUILayout.Button("Center")) { }
            if (GUILayout.Button("Max")) { }

            GUILayout.Space(10);

            if (GUILayout.Button("X")) { }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("Y", GUILayout.ExpandWidth(false));
            if (GUILayout.Button("Min")) { }
            if (GUILayout.Button("Center")) { }
            if (GUILayout.Button("Max")) { }

            GUILayout.Space(10);

            if (GUILayout.Button("Y")) { }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("Z", GUILayout.ExpandWidth(false));
            if (GUILayout.Button("Min")) { }
            if (GUILayout.Button("Center")) { }
            if (GUILayout.Button("Max")) { }

            GUILayout.Space(10);

            if (GUILayout.Button("Z")) { }

            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.LabelField("Align & Distribute is available in uContext Pro.", EditorStyles.wordWrappedLabel);
            if (GUILayout.Button("Open uContext Pro"))
            {
                Links.OpenPro();
            }
        }

        public override void Init()
        {
            _content = new GUIContent(Styles.isProSkin ? Icons.align : Icons.alignDark, "Align & Distribute (PRO)");
        }

        public override bool Validate()
        {
            return Prefs.proHighlight && Selection.gameObjects.Count(g => g.scene.name != null) > 1;
        }
    }

#endif
}