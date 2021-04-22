/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Reflection;
using InfinityCode.uContext.Windows;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace InfinityCode.uContext.Tools
{
    [InitializeOnLoad]
    public static class DragAndDropBehaviour
    {
        private static MethodInfo cleanUpSprite;
        private static FieldInfo dragObjectField;

        static DragAndDropBehaviour()
        {
            SceneViewManager.AddListener(OnSceneGUI);
            SceneViewManager.AddListener(OnDragComponent);

            cleanUpSprite = EditorTypes.spriteUtility.GetMethod("CleanUp", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(bool) }, null);
            dragObjectField = EditorTypes.gameObjectInspector.GetField("dragObject", BindingFlags.Static | BindingFlags.Public);
        }

        private static void OnDragComponent(SceneView sceneView)
        {
            Event e = Event.current;

            if (e.modifiers != EventModifiers.Control && e.modifiers != EventModifiers.Command) return;
            if (e.type != EventType.DragUpdated && e.type != EventType.DragPerform) return;

            EditorWindow wnd = EditorWindow.mouseOverWindow;

            if (wnd == null) return;
            if (!(wnd is SceneView)) return;
            if (DragAndDrop.objectReferences.Length != 1) return;
            if (!(DragAndDrop.objectReferences[0] is Component)) return;

            if (e.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                e.Use();
            }
            else if (e.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();

                ComponentWindow.Show(DragAndDrop.objectReferences[0] as Component);

                e.Use();
            }
        }

        private static void OnDragGameObjectPerform(SceneView view, Event e)
        {
            GameObject go = DragAndDrop.objectReferences[0] as GameObject;
            if (go.scene.name != null || go.GetComponent<RectTransform>() == null) return;

            GameObject target = HandleUtility.PickGameObject(e.mousePosition, false, null);
            if (target == null) return;
            RectTransform parent = target.GetComponent<RectTransform>();
            if (parent == null) return;

            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
#if UNITY_2020_1_OR_NEWER
                if (prefabStage.assetPath == DragAndDrop.paths[0]) return;
#else
                if (prefabStage.prefabAssetPath == DragAndDrop.paths[0]) return;
#endif
                if (e.alt)
                {
                    RectTransform p = prefabStage.prefabContentsRoot.transform as RectTransform;
                    parent = p != null ? p : GameObjectUtils.GetRoot(parent);
                }
            }
            else if (e.alt) parent = GameObjectUtils.GetRoot(parent);

            DragAndDrop.AcceptDrag();
            GameObject instance = PrefabUtility.InstantiatePrefab(go) as GameObject;
            Undo.RegisterCreatedObjectUndo(instance, "Drag Instance");
            instance.transform.SetParent(parent, false);
            instance.transform.position = SceneViewManager.lastWorldPosition;
            e.Use();

        }

        private static void OnDragGameObjectUpdated(Event e)
        {
            GameObject go = DragAndDrop.objectReferences[0] as GameObject;
            if (go.GetComponent<RectTransform>() == null) return;

            GameObject target = HandleUtility.PickGameObject(Event.current.mousePosition, false, null);
            if (target == null || target.GetComponent<RectTransform>() == null) return;

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            e.Use();

            if (dragObjectField != null)
            {
                GameObject preview = dragObjectField.GetValue(null) as GameObject;
                if (preview != null)
                {
                    Object.DestroyImmediate(preview);
                    dragObjectField.SetValue(null, null);
                }
            }
        }

        private static void OnDragPerform(SceneView view, Event e)
        {
            if (DragAndDrop.objectReferences.Length != 1) return;

            Object obj = DragAndDrop.objectReferences[0];
            if (obj is Texture) OnDragTexturePerform(view, e);
            else if (obj is Sprite) OnDragSpritePerform(view, e);
            else if (obj is GameObject) OnDragGameObjectPerform(view, e);
        }

        private static void OnDragSpritePerform(SceneView view, Event e)
        {
            GameObject go = HandleUtility.PickGameObject(Event.current.mousePosition, false, null);
            if (go == null) return;

            Image image = go.GetComponent<Image>();
            RectTransform rt = go.GetComponent<RectTransform>();
            if (image != null && e.modifiers == EventModifiers.None)
            {
                DragAndDrop.AcceptDrag();
                SetReferenceValue(image, "m_Sprite");
                e.Use();
            }
            else if (rt != null)
            {
                DragAndDrop.AcceptDrag();
                EditorApplication.ExecuteMenuItem("GameObject/UI/Image");
                GameObject newGO = Selection.activeGameObject;
                if (e.alt) rt = GameObjectUtils.GetRoot(rt);
                SetPosition(view, newGO, rt);

                SetReferenceValue(newGO.GetComponent<Image>(), "m_Sprite");
                e.Use();
            }
        }

        private static void OnDragSpriteUpdated(Event e)
        {
            GameObject go = HandleUtility.PickGameObject(Event.current.mousePosition, false, null);
            if (go == null) return;

            if (go.GetComponent<Image>() != null && e.modifiers == EventModifiers.None)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                e.Use();
            }
            else if (go.GetComponent<RectTransform>() != null)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                e.Use();
            }

            if (e.type == EventType.Used)
            {
                cleanUpSprite.Invoke(null, new object[] { true });
            }
        }

        private static void OnDragTexturePerform(SceneView view, Event e)
        {
            GameObject go = HandleUtility.PickGameObject(Event.current.mousePosition, false, null);
            if (go == null) return;

            RectTransform rt = go.GetComponent<RectTransform>();

            if (e.modifiers == EventModifiers.None)
            {
                RawImage rawImage = go.GetComponent<RawImage>();
                if (rawImage != null)
                {
                    DragAndDrop.AcceptDrag();
                    SetReferenceValue(rawImage, "m_Texture");
                    e.Use();
                    return;
                }

                Image image = go.GetComponent<Image>();
                if (image != null)
                {
                    TextureImporter importer = AssetImporter.GetAtPath(DragAndDrop.paths[0]) as TextureImporter;
                    if (importer != null && importer.textureType == TextureImporterType.Sprite)
                    {
                        DragAndDrop.AcceptDrag();
                        SetReferenceValue(image, "m_Sprite", AssetDatabase.LoadAssetAtPath<Sprite>(DragAndDrop.paths[0]));
                        e.Use();
                        return;
                    }
                }
            }
            
            if (rt != null)
            {
                DragAndDrop.AcceptDrag();
                EditorApplication.ExecuteMenuItem("GameObject/UI/Raw Image");
                GameObject newGO = Selection.activeGameObject;
                if (e.alt) rt = GameObjectUtils.GetRoot(rt);
                SetPosition(view, newGO, rt);
                SetReferenceValue(newGO.GetComponent<RawImage>(), "m_Texture");
                e.Use();
            }
        }

        private static void OnDragTextureUpdated(Event e)
        {
            GameObject go = HandleUtility.PickGameObject(Event.current.mousePosition, false, null);
            if (go == null) return;

            if (e.modifiers == EventModifiers.None)
            {
                if (go.GetComponent<RawImage>() != null)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                    e.Use();
                    return;
                }

                if (go.GetComponent<Image>())
                {
                    TextureImporter importer = AssetImporter.GetAtPath(DragAndDrop.paths[0]) as TextureImporter;
                    if (importer != null && importer.textureType == TextureImporterType.Sprite)
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                        e.Use();
                        cleanUpSprite.Invoke(null, new object[] {true});
                        return;
                    }
                }
            }

            if (go.GetComponent<RectTransform>() != null)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                e.Use();
            }
        }

        private static void OnDragUpdated(SceneView view, Event e)
        {
            if (DragAndDrop.objectReferences.Length != 1) return;

            Object obj = DragAndDrop.objectReferences[0];
            if (obj is Texture) OnDragTextureUpdated(e);
            else if (obj is Sprite) OnDragSpriteUpdated(e);
            else if (obj is GameObject) OnDragGameObjectUpdated(e);
        }

        private static void OnSceneGUI(SceneView view)
        {
            if (!Prefs.improveDragAndDropBehaviour) return;

            Event e = Event.current;
            if (e.type == EventType.DragPerform) OnDragPerform(view, e);
            else if (e.type == EventType.DragUpdated) OnDragUpdated(view, e);
        }

        private static void SetPosition(SceneView view, GameObject newGO, RectTransform rt)
        {
            RectTransform rectTransform = newGO.GetComponent<RectTransform>();
            rectTransform.SetParent(rt);
            Vector3 screenPos = Event.current.mousePosition;
            screenPos.y = view.position.height - screenPos.y;
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, screenPos, view.camera, out pos);
            rectTransform.anchoredPosition = pos;
        }

        private static void SetReferenceValue(Object obj, string field)
        {
            Object value = DragAndDrop.objectReferences[0];
            SetReferenceValue(obj, field, value);
        }

        private static void SetReferenceValue(Object obj, string field, Object value)
        {
            SerializedObject so = new SerializedObject(obj);
            so.Update();
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedProperties();
        }
    }
}