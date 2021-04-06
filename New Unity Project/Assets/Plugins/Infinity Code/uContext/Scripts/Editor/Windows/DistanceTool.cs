/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace InfinityCode.uContext.Windows
{
    public class DistanceTool: EditorWindow
    {
        private const string StyleID = "sv_label_3";

        public delegate void UseCursorDelegate(Vector3 prev, bool hasPrev, ref float distance);

        public static UseCursorDelegate OnUseCursorGUI;
        public static UseCursorDelegate OnUseCursorSceneGUI;

        [SerializeField]
        private List<Transform> targets;
        private ReorderableList reorderableList;

        private float totalDistance;
        private bool hasPrev = false;
        private Vector3 prevPosition;
        private Vector2 scrollPosition;
        public static GUIStyle style;
        public static bool isDirty;

        private void AddItem(ReorderableList list)
        {
            targets.Add(null);
        }

        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            Transform t = targets[index];

            EditorGUI.BeginChangeCheck();
            Rect r = new Rect(rect);
            r.height /= 2;
            t = EditorGUI.ObjectField(r, t, typeof(Transform), true) as Transform;
            if (EditorGUI.EndChangeCheck()) targets[index] = t;

            string message = "Ignored";

            if (t != null)
            {
                message = t.position.ToString("F1");
                if (hasPrev)
                {
                    float d = (t.position - prevPosition).magnitude;
                    message += ", Distance: " + d.ToString("F1") + "m";
                    totalDistance += d;
                }
            }

            r.y += r.height;
            EditorGUI.LabelField(r, message);

            if (t != null)
            {
                prevPosition = t.position;
                hasPrev = true;
            }
        }

        private void DrawHeader(Rect rect)
        {
            GUI.Label(rect, "Transforms");
        }

        private void OnDestroy()
        {
            SceneViewManager.RemoveListener(OnSceneView);
        }

        private void OnEnable()
        {
            SceneViewManager.AddListener(OnSceneView, 0, true);
        }

        private void OnGUI()
        {
            if (targets == null) targets = new List<Transform>();

            if (reorderableList == null)
            {
                reorderableList = new ReorderableList(targets, typeof(Transform), true, true, true, true);
                reorderableList.drawHeaderCallback += DrawHeader;
                reorderableList.drawElementCallback += DrawElement;
                reorderableList.onAddCallback += AddItem;
                reorderableList.onRemoveCallback += RemoveItem;
                reorderableList.elementHeight = 48;
            }

            totalDistance = 0;
            hasPrev = false;

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            reorderableList.DoLayoutList();
            EditorGUILayout.EndScrollView();

            if (OnUseCursorGUI != null) OnUseCursorGUI(prevPosition, hasPrev, ref totalDistance);

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField("Total Distance: " + totalDistance.ToString("F1") + "m");
            EditorGUILayout.EndHorizontal();

            ProcessEvents();

            if (isDirty)
            {
                isDirty = false;
                Repaint();
            }
        }

        private void OnSceneView(SceneView sceneView)
        {
            if (!Prefs.showDistanceInScene) return;

            if (targets == null) targets = new List<Transform>();

            if (style == null)
            {
                style = new GUIStyle(StyleID)
                {
                    fontSize = 10,
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = false,
                    fixedHeight = 16,
                    normal =
                    {
                        textColor = Color.white
                    },
                    padding = new RectOffset(2, 2, 0, 0),
                };
            }

            Color color = Handles.color;

            Handles.color = Color.green;

            Vector3 prev = Vector3.zero;
            hasPrev = false;

            foreach (Transform t in targets)
            {
                if (t == null) continue;

                Vector3 p = t.position;

                if (hasPrev)
                {
                    Handles.DrawLine(p, prev);
                    Handles.Label((p + prev) / 2, (p - prev).magnitude.ToString("F1"), style);
                }
                else hasPrev = true;

                prev = p;
            }

            if (OnUseCursorSceneGUI != null) OnUseCursorSceneGUI(prev, hasPrev, ref totalDistance);

            Handles.color = color;
        }

        [MenuItem(WindowsHelper.MenuPath + "Distance Tool", false, 100)]
        public static void OpenWindow()
        {
            GetWindow<DistanceTool>(false, "Distance Tool").autoRepaintOnSceneChange = true;
        }

        private void ProcessEvents()
        {
            Event e = Event.current;

            if (e.type == EventType.DragUpdated)
            {
                for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                {
                    Object obj = DragAndDrop.objectReferences[i];
                    if (!(obj is GameObject || obj is Component)) return;
                }

                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                e.Use();
            }
            else if (e.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();

                for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                {
                    Object obj = DragAndDrop.objectReferences[i];
                    if (obj is GameObject) targets.Add((obj as GameObject).transform);
                    else if (obj is Component) targets.Add((obj as Component).transform);
                }

                e.Use();
            }
        }

        private void RemoveItem(ReorderableList list)
        {
            targets.RemoveAt(list.index);
        }
    }
}