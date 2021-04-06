/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Linq;
using InfinityCode.uContext.Windows;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.uContext.Tools
{
    [InitializeOnLoad]
    public static class ToolValues
    {
        public const string StyleID = "sv_label_0";

        private static GUIStyle style;

        private static Transform firstTransform;
        private static bool samePositionX;
        private static bool samePositionY;
        private static bool samePositionZ;
        private static bool sameRotationX;
        private static bool sameRotationY;
        private static bool sameRotationZ;
        private static bool sameScaleX;
        private static bool sameScaleY;
        private static bool sameScaleZ;
        public static Vector3 lastScreenPosition;
        public static bool isBelowHandle;
        private static string label;

        static ToolValues()
        {
            SceneViewManager.AddListener(OnSceneViewGUI, 0, true);
            Selection.selectionChanged += OnSelectionChanged;
            OnSelectionChanged();
        }

        private static void AppendPosition()
        {
            StaticStringBuilder.Append("Position (");
            StaticStringBuilder.Append(samePositionX ? firstTransform.localPosition.x.ToString("F2", Culture.numberFormat) : "---");
            StaticStringBuilder.Append(", ");
            StaticStringBuilder.Append(samePositionY ? firstTransform.localPosition.y.ToString("F2", Culture.numberFormat) : "---");
            StaticStringBuilder.Append(", ");
            StaticStringBuilder.Append(samePositionZ ? firstTransform.localPosition.z.ToString("F2", Culture.numberFormat) : "---");
            StaticStringBuilder.Append(")");
        }

        private static void AppendRotation()
        {
            StaticStringBuilder.Append("Rotation (");
            StaticStringBuilder.Append(sameRotationX ? firstTransform.eulerAngles.x.ToString("F2", Culture.numberFormat) : "---");
            StaticStringBuilder.Append(", ");
            StaticStringBuilder.Append(sameRotationY ? firstTransform.eulerAngles.y.ToString("F2", Culture.numberFormat) : "---");
            StaticStringBuilder.Append(", ");
            StaticStringBuilder.Append(sameRotationZ ? firstTransform.eulerAngles.z.ToString("F2", Culture.numberFormat) : "---");
            StaticStringBuilder.Append(")");
        }

        private static void AppendScale()
        {
            StaticStringBuilder.Append("Scale (");
            StaticStringBuilder.Append(sameScaleX ? firstTransform.localScale.x.ToString("F2", Culture.numberFormat) : "---");
            StaticStringBuilder.Append(", ");
            StaticStringBuilder.Append(sameScaleY ? firstTransform.localScale.y.ToString("F2", Culture.numberFormat) : "---");
            StaticStringBuilder.Append(", ");
            StaticStringBuilder.Append(sameScaleZ ? firstTransform.localScale.z.ToString("F2", Culture.numberFormat) : "---");
            StaticStringBuilder.Append(")");
        }

        private static void BlockMouseUp(SceneView view)
        {
            Event e = Event.current;
            if (e.type != EventType.MouseUp) return;

            SceneViewManager.RemoveListener(BlockMouseUp);
            GUIUtility.hotControl = 0;
        }

        private static void DrawLabel(SceneView sceneView, string text)
        {
            if (style == null)
            {
                style = new GUIStyle(StyleID)
                {
                    fontSize = 10,
                    alignment = TextAnchor.MiddleLeft,
                    wordWrap = false,
                    fixedHeight = 0,
                    border = new RectOffset(8, 8, 8, 8)
                };
            }

            GUIContent content = new GUIContent(text);
            Vector2 size = style.CalcSize(content);

            Handles.BeginGUI();
            Vector3 screenPoint = sceneView.camera.WorldToScreenPoint(UnityEditor.Tools.handlePosition);
            if (screenPoint.y > size.y + 150)
            {
                screenPoint.y -= size.y + 50;
                isBelowHandle = true;
            }
            else
            {
                screenPoint.y += size.y + 150;
                isBelowHandle = false;
            }

            lastScreenPosition = screenPoint;

            Rect rect = new Rect(screenPoint.x - size.x / 2, Screen.height - screenPoint.y - size.y / 2, size.x, size.y);
            GUI.Label(rect, content, style);

            Event e = Event.current;

            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                Transform[] transforms = Selection.gameObjects.Select(g => g.GetComponent<Transform>()).ToArray();
                TransformEditorWindow.ShowPopup(transforms);

                SceneViewManager.AddListener(BlockMouseUp);
                e.Use();

                GUIUtility.hotControl = 1000;
            }
            Handles.EndGUI();
        }

        private static void InitLabel()
        {
            Event e = Event.current;
            label = null;

            if (!Prefs.showToolValues) return;
            if (firstTransform == null) return;
            if (TransformEditorWindow.instance != null) return;
            if (UnityEditor.Tools.hidden || UnityEditor.Tools.current == Tool.View) return;

            if (e.modifiers != Prefs.toolValuesModifiers)
            {
                bool isHandlePressed = IsHandlesHotControl();
                if (!isHandlePressed) return;
            }

            StaticStringBuilder.Clear();

            Tool tool = UnityEditor.Tools.current;
#if UNITY_2020_2_OR_NEWER
            if (tool == Tool.Move || ToolManager.activeToolType == typeof(DuplicateTool))
#else
            if (tool == Tool.Move || EditorTools.activeToolType == typeof(DuplicateTool))
#endif
            {
                AppendPosition();
                label = StaticStringBuilder.GetString();
            }
            else if (tool == Tool.Rotate)
            {
                AppendRotation();
                label = StaticStringBuilder.GetString();
            }
            else if (tool == Tool.Scale)
            {
                AppendScale();
                label = StaticStringBuilder.GetString();
            }
            else if (tool == Tool.Rect)
            {
                AppendPosition();
                StaticStringBuilder.Append("\n");
                AppendScale();
                label = StaticStringBuilder.GetString();
            }
            else if (tool == Tool.Transform)
            {
                AppendPosition();
                StaticStringBuilder.Append("\n");
                AppendRotation();
                StaticStringBuilder.Append("\n");
                AppendScale();
                label = StaticStringBuilder.GetString();
            }

            StaticStringBuilder.Clear();
        }

        private static bool IsHandlesHotControl()
        {
            int c = GUIUtility.hotControl;
            if (c == 0) return false;

            if (c >= 205 && c <= 211) return true;
            if (c >= 1412 && c <= 1419) return true;
            if (c >= 1440 && c <= 1441) return true;
            if (c >= 1456 && c <= 1467) return true;
            if (c >= 1628 && c <= 1643) return true;

            return false;
        }

        private static void OnSceneViewGUI(SceneView sceneView)
        {
            try
            {
                if (Event.current.type == EventType.Layout) InitLabel();
                if (!string.IsNullOrEmpty(label)) DrawLabel(sceneView, label);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static void OnSelectionChanged()
        {
            firstTransform = null;
            if (Selection.gameObjects.Length == 0) return;

            samePositionX = samePositionY = samePositionZ = sameRotationX = sameRotationY = sameRotationZ = sameScaleX = sameScaleY = sameScaleZ = true;

            int[] instanceIDs = Selection.instanceIDs;

            for (int i = 0; i < instanceIDs.Length; i++)
            {
                GameObject go = EditorUtility.InstanceIDToObject(instanceIDs[i]) as GameObject;
                if (go == null || go.scene.name == null) continue;

                if (firstTransform == null)
                {
                    firstTransform = go.transform;
                    continue;
                }

                Transform t = go.transform;
                Vector3 p1 = t.localPosition;
                Vector3 p2 = firstTransform.localPosition;
                Vector3 r1 = t.eulerAngles;
                Vector3 r2 = firstTransform.eulerAngles;
                Vector3 s1 = t.localScale;
                Vector3 s2 = firstTransform.localScale;
                if (samePositionX && Math.Abs(p1.x - p2.x) > float.Epsilon) samePositionX = false;
                if (samePositionY && Math.Abs(p1.y - p2.y) > float.Epsilon) samePositionY = false;
                if (samePositionZ && Math.Abs(p1.z - p2.z) > float.Epsilon) samePositionZ = false;
                if (sameRotationX && Math.Abs(r1.x - r2.x) > float.Epsilon) sameRotationX = false;
                if (sameRotationY && Math.Abs(r1.y - r2.y) > float.Epsilon) sameRotationY = false;
                if (sameRotationZ && Math.Abs(r1.z - r2.z) > float.Epsilon) sameRotationZ = false;
                if (sameScaleX && Math.Abs(s1.x - s2.x) > float.Epsilon) sameScaleX = false;
                if (sameScaleY && Math.Abs(s1.y - s2.y) > float.Epsilon) sameScaleY = false;
                if (sameScaleZ && Math.Abs(s1.z - s2.z) > float.Epsilon) sameScaleZ = false;
            }
        }
    }
}