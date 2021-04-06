/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.uContext.TransformEditorTools;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext.ProHighlight
{
#if !UCONTEXT_PRO
    public class BoundsTool : TransformEditorTool
    {
        public override void Draw()
        {
            GUILayout.Label("Bounds");

            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 85;

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.Vector3Field("Center", Vector3.zero);
            EditorGUILayout.Vector3Field("Size", Vector3.zero);
            EditorGUI.EndDisabledGroup();

            EditorGUIUtility.labelWidth = labelWidth;

            EditorGUILayout.LabelField("Bounds is available in uContext Pro.", EditorStyles.wordWrappedLabel);
            if (GUILayout.Button("Open uContext Pro"))
            {
                Links.OpenPro();
            }
        }

        public override void Init()
        {
            _content = new GUIContent(Styles.isProSkin ? Icons.bounds : Icons.boundsDark, "Bounds (PRO)");
        }

        public override bool Validate()
        {
            return Prefs.proHighlight;
        }
    }
#endif
}