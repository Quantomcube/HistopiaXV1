/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections;
using System.Linq;
using InfinityCode.uContext.FloatToolbar;
using InfinityCode.uContext.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext.Tools
{
    [InitializeOnLoad]
    public static class Switcher
    {
        static Switcher()
        {
            KeyManager.KeyBinding binding = KeyManager.AddBinding();
            binding.OnValidate += () => Prefs.switcher;
            binding.OnInvoke += OnInvoke;

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnInvoke()
        {
            Event e = Event.current;
            if (Prefs.switcherWindows && e.keyCode == Prefs.switcherWindowsKeyCode && e.modifiers == Prefs.switcherWindowsModifiers) OnSwitch();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange mode)
        {
            try
            {
                if (mode == PlayModeStateChange.EnteredPlayMode)
                {
                    EditorApplication.delayCall += () =>
                    {
                        SceneView sceneView = SceneView.lastActiveSceneView;
                        if (sceneView != null && sceneView.maximized)
                        {
                            sceneView.maximized = false;
                            sceneView.Repaint();

                            IList list = EditorReflected.GetGameViews();
                            if (list != null && list.Count > 0)
                            {
                                EditorWindow gameView = list[0] as EditorWindow;
                                if (gameView != null)
                                {
                                    gameView.maximized = true;
                                    gameView.Focus();
                                }
                            }
                        }
                    };
                }
                else if (mode == PlayModeStateChange.EnteredEditMode)
                {
                    IList list = EditorReflected.GetGameViews();
                    if (list != null && list.Count > 0)
                    {
                        EditorWindow gameView = list[0] as EditorWindow;
                        if (gameView != null && gameView.maximized)
                        {
                            gameView.maximized = false;
                            gameView.Repaint();

                            SceneView sceneView = SceneView.lastActiveSceneView;

                            if (sceneView != null)
                            {
                                sceneView.Focus();
                                sceneView.maximized = true;
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private static void OnSwitch()
        {
            EditorWindow window = EditorWindow.focusedWindow;
            if (window == null)
            {
                window = EditorWindow.mouseOverWindow;
                if (window == null) return;
            }

            EditorWindow[] activeWindows = UnityEngine.Resources.FindObjectsOfTypeAll<EditorWindow>();
            EditorWindow maximizedWindow = activeWindows.FirstOrDefault(w => w.maximized);

            bool maximized = maximizedWindow != null;

            if (maximized)
            {
                maximizedWindow.maximized = false;
                window.Repaint();
                window = maximizedWindow;
            }
            if (window is SceneView)
            {
                SwitchToGameView(maximized);
            }
            else if (window is PinAndClose || window == QuickAccess.activeWindow || window == ObjectToolbar.activeWindow)
            {
                SwitchToGameView(maximized);
            }
            else
            {
                SwitchToSceneView(maximized);
            }
        }

        private static void SwitchToGameView(bool maximized)
        {
            IList list = EditorReflected.GetGameViews();
            if (list != null && list.Count > 0)
            {
                EditorWindow gameView = list[0] as EditorWindow;
                if (gameView != null)
                {
                    if (maximized) gameView.maximized = true;
                    gameView.Focus();
                    ObjectToolbar.CloseActiveWindow();
                    QuickAccess.CloseActiveWindow();
                }
            }
        }

        private static void SwitchToSceneView(bool maximized)
        {
            if (SceneView.sceneViews == null || SceneView.sceneViews.Count == 0) return;
            EditorWindow sceneView = SceneView.sceneViews[0] as EditorWindow;
            if (sceneView != null)
            {
                if (maximized) sceneView.maximized = true;
                sceneView.Focus();
                if (EditorApplication.isPlaying) EditorApplication.isPaused = true;
            }
        }
    }
}