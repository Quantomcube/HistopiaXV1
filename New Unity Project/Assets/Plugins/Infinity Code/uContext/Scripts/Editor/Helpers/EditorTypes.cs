/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using UnityEngine;

namespace InfinityCode.uContext
{
    public static class EditorTypes
    {
        private static Type _addComponentWindow;
        private static Type _assetImporterEditor;
        private static Type _audioUtils;
        private static Type _consoleWindow;
        private static Type _gameObjectInspector;
        private static Type _gameView;
        private static Type _guiView;
        private static Type _inspectorWindow;
        private static Type _iWindowBackend;
        private static Type _logEntries;
        private static Type _logEntry;
        private static Type _menuUtils;
        private static Type _objectSelector;
        private static Type _paintTreesTool;
        private static Type _playModeView;
        private static Type _prefabImporter;
        private static Type _prefabStageUtility;
        private static Type _projectBrowser;
        private static Type _sceneHierarchy;
        private static Type _sceneHierarchyWindow;
        private static Type _sceneVisibilityManager;
        private static Type _sceneVisibilityState;
        private static Type _projectSettingsWindow;
        private static Type _spriteUtility;
        private static Type _terrainInspector;
        private static Type _textureImporterInspector;
        private static Type _toolbar;
        private static Type _treePainter;
        private static Type _treeViewController;
        private static Type _treeViewGUI;

        public static Type addComponentWindow
        {
            get
            {
                if (_addComponentWindow == null) _addComponentWindow = Reflection.GetEditorType("AddComponent.AddComponentWindow");
                return _addComponentWindow;
            }
        }

        public static Type assetImporterEditor
        {
            get
            {
                if (_assetImporterEditor == null)
                {
#if UNITY_2020_2_OR_NEWER
                    string name = "AssetImporters.AssetImporterEditor";
#else
                    string name = "Experimental.AssetImporters.AssetImporterEditor";
#endif
                    _assetImporterEditor = Reflection.GetEditorType(name);
                }
                return _assetImporterEditor;
            }
        }

        public static Type audioUtils
        {
            get
            {
                if (_audioUtils == null) _audioUtils = Reflection.GetEditorType("AudioUtil");
                return _audioUtils;
            }
        }

        public static Type consoleWindow
        {
            get
            {
                if (_consoleWindow == null) _consoleWindow = Reflection.GetEditorType("ConsoleWindow");
                return _consoleWindow;
            }
        }

        public static Type gameObjectInspector
        {
            get
            {
                if (_gameObjectInspector == null) _gameObjectInspector = Reflection.GetEditorType("GameObjectInspector");
                return _gameObjectInspector;
            }
        }

        public static Type gameView
        {
            get
            {
                if (_gameView == null) _gameView = Reflection.GetEditorType("GameView");
                return _gameView;
            }
        }

        public static Type guiView
        {
            get
            {
                if (_guiView == null) _guiView = Reflection.GetEditorType("GUIView");
                return _guiView;
            }
        }

        public static Type inspectorWindow
        {
            get
            {
                if (_inspectorWindow == null) _inspectorWindow = Reflection.GetEditorType("InspectorWindow");
                return _inspectorWindow;
            }
        }

        public static Type iWindowBackend
        {
            get
            {
                if (_iWindowBackend == null) _iWindowBackend = Reflection.GetEditorType("IWindowBackend");
                return _iWindowBackend;
            }
        }

        public static Type logEntries
        {
            get
            {
                if (_logEntries == null)
                {
                    _logEntries = Reflection.GetEditorType("LogEntries", "UnityEditorInternal");
                    if (_logEntries == null) _logEntries = Reflection.GetEditorType("LogEntries");
                }
                return _logEntries;
            }
        }

        public static Type logEntry
        {
            get
            {
                if (_logEntry == null)
                {
                    _logEntry = Reflection.GetEditorType("LogEntry", "UnityEditorInternal");
                    if (_logEntry == null) _logEntry = Reflection.GetEditorType("LogEntry");
                }
                return _logEntry;
            }
        }

        public static Type menuUtils
        {
            get
            {
                if (_menuUtils == null) _menuUtils = Reflection.GetEditorType("MenuUtils");
                return _menuUtils;
            }
        }

        public static Type objectSelector
        {
            get
            {
                if (_objectSelector == null) _objectSelector = Reflection.GetEditorType("ObjectSelector");
                return _objectSelector;
            }
        }

        public static Type paintTreesTool
        {
            get
            {
                if (_paintTreesTool == null) _paintTreesTool = Reflection.GetEditorType("Experimental.TerrainAPI.PaintTreesTool");
                return _paintTreesTool;
            }
        }

        public static Type playModeView
        {
            get
            {
                if (_playModeView == null) _playModeView = Reflection.GetEditorType("PlayModeView");
                return _playModeView;
            }
        }

        public static Type prefabImporter
        {
            get
            {
                if (_prefabImporter == null) _prefabImporter = Reflection.GetEditorType("PrefabImporter");
                return _prefabImporter;
            }
        }

        public static Type prefabStageUtility
        {
            get
            {
                if (_prefabStageUtility == null) _prefabStageUtility = Reflection.GetEditorType("Experimental.SceneManagement.PrefabStageUtility");
                return _prefabStageUtility;
            }
        }

        public static Type projectBrowser
        {
            get
            {
                if (_projectBrowser == null) _projectBrowser = Reflection.GetEditorType("ProjectBrowser");
                return _projectBrowser;
            }
        }

        public static Type projectSettingsWindow
        {
            get
            {
                if (_projectSettingsWindow == null) _projectSettingsWindow = Reflection.GetEditorType("ProjectSettingsWindow");
                return _projectSettingsWindow;
            }
        }

        public static Type sceneHierarchy
        {
            get
            {
                if (_sceneHierarchy == null) _sceneHierarchy = Reflection.GetEditorType("SceneHierarchy");
                return _sceneHierarchy;
            }
        }

        public static Type sceneHierarchyWindow
        {
            get
            {
                if (_sceneHierarchyWindow == null) _sceneHierarchyWindow = Reflection.GetEditorType("SceneHierarchyWindow");
                return _sceneHierarchyWindow;
            }
        }

        public static Type sceneVisibilityManager
        {
            get
            {
                if (_sceneVisibilityManager == null) _sceneVisibilityManager = Reflection.GetEditorType("SceneVisibilityManager");
                return _sceneVisibilityManager;
            }
        }

        public static Type sceneVisibilityState
        {
            get
            {
                if (_sceneVisibilityState == null) _sceneVisibilityState = Reflection.GetEditorType("SceneVisibilityState");
                return _sceneVisibilityState;
            }
        }

        public static Type spriteUtility
        {
            get
            {
                if (_spriteUtility == null) _spriteUtility = Reflection.GetEditorType("SpriteUtility");
                return _spriteUtility;
            }
        }

        public static Type terrainInspector
        {
            get
            {
                if (_terrainInspector == null) _terrainInspector = Reflection.GetEditorType("TerrainInspector");
                return _terrainInspector;
            }
        }

        public static Type textureImporterInspector
        {
            get
            {
                if (_textureImporterInspector == null) _textureImporterInspector = Reflection.GetEditorType("TextureImporterInspector");
                return _textureImporterInspector;
            }
        }

        public static Type toolbar
        {
            get
            {
                if (_toolbar == null) _toolbar = Reflection.GetEditorType("Toolbar");
                return _toolbar;
            }
        }

        public static Type treePainter
        {
            get
            {
                if (_treePainter == null) _treePainter = Reflection.GetEditorType("TreePainter");
                return _treePainter;
            }
        }

        public static Type treeViewController
        {
            get
            {
                if (_treeViewController == null) _treeViewController = Reflection.GetEditorType("IMGUI.Controls.TreeViewController");
                return _treeViewController;
            }
        }
        public static Type treeViewGUI
        {
            get
            {
                if (_treeViewGUI == null) _treeViewGUI = Reflection.GetEditorType("IMGUI.Controls.TreeViewGUI");
                return _treeViewGUI;
            }
        }
    }
}