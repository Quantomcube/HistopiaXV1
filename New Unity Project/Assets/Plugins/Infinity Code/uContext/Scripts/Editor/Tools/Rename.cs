/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Linq;
using InfinityCode.uContext.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext.Tools
{
    [InitializeOnLoad]
    public static class Rename
    {
        public static Action OnMassRename;
        public static GameObject[] gameObjects;

        static Rename()
        {
            KeyManager.KeyBinding binding = KeyManager.AddBinding();
            binding.OnValidate = OnValidate;
            binding.OnInvoke = OnInvoke;
        }

        public static void OnDialogClose(InputDialog dialog)
        {
            gameObjects = null;
        }

        private static void OnInvoke()
        {
            Event e = Event.current;
            if (e.keyCode != KeyCode.F2) return;
            if (e.modifiers != EventModifiers.FunctionKey) return;

            EditorWindow wnd = EditorWindow.focusedWindow;
            if (wnd.GetType() == typeof(ComponentWindow))
            {
                gameObjects = new[]
                {
                    (wnd as ComponentWindow).component.gameObject
                };
            }
            else if (wnd.GetType() == typeof(PinAndClose))
            {
                ComponentWindow cw = (wnd as PinAndClose).targetWindow as ComponentWindow;
                gameObjects = new[]
                {
                    cw.component.gameObject
                };
            }
            else
            {
                gameObjects = Selection.gameObjects.Where(g => g.scene.name != null).ToArray();
                if (Selection.gameObjects.Length == 0) return;
            }

            if (gameObjects.Length == 0) return;

            if (gameObjects.Length == 1)
            {
                InputDialog dialog = InputDialog.Show("Enter a new GameObject name", gameObjects[0].name, OnRename);
                dialog.OnClose += OnDialogClose;
                return;
            }

            if (OnMassRename == null)
            {
                if (EditorUtility.DisplayDialog("Warning", "Mass rename available in uContext Pro.", "Open uContext Pro", "Cancel"))
                {
                    Links.OpenPro();
                }
            }
            else OnMassRename();
        }

        public static void OnRename(string name)
        {
            if (gameObjects == null || gameObjects.Length == 0) return;

            Undo.RecordObjects(gameObjects, "Rename GameObjects");
            foreach (GameObject go in gameObjects.Where(g => g.scene.name != null))
            {
                go.name = name;
            }

            gameObjects = null;
        }

        private static bool OnValidate()
        {
            EditorWindow wnd = EditorWindow.focusedWindow;
            if (wnd == null) return false;
            Type type = wnd.GetType();
            return type == typeof(SceneView) || 
                   type == EditorTypes.inspectorWindow || 
                   type == typeof(ComponentWindow) || 
                   type == typeof(LayoutWindow) ||
                   type == EditorTypes.consoleWindow ||
                   (type == EditorTypes.sceneHierarchyWindow && Selection.gameObjects.Length > 1) || 
                   (type == typeof(PinAndClose) && (wnd as PinAndClose).targetWindow is ComponentWindow);
        }

        public static void Show(params GameObject[] targets)
        {
            if (targets == null || targets.Length == 0) return;

            gameObjects = targets;
            
            if (targets.Length == 1)
            {
                InputDialog dialog = InputDialog.Show("Enter a new GameObject name", gameObjects[0].name, OnRename);
                dialog.OnClose += OnDialogClose;
                return;
            }

            if (OnMassRename != null)
            {
                OnMassRename();
                return;
            }

            if (EditorUtility.DisplayDialog("Mass rename available in uContext Pro.", "Open uContext Pro", "Cancel"))
            {
                Links.OpenPro();
            }
        }
    }
}