/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using System.Linq;
using InfinityCode.uContext.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext.Tools
{
    [InitializeOnLoad]
    public static class Group
    {
        private static string defaultName = "GameObject";
        private static GameObject[] _targets;

        static Group()
        {
            KeyManager.KeyBinding binding = KeyManager.AddBinding();
            binding.OnValidate += () => Selection.gameObjects.Length > 0;
            binding.OnInvoke += OnInvoke;
        }

        private static Transform FindParent(Transform p1, Transform p2)
        {
            if (p1 == null || p2 == null) return null;
            if (p1 == p2) return p1;

            List<Transform> rp1 = new List<Transform>{p1};
            List<Transform> rp2 = new List<Transform>{p2};

            Transform tp1 = p1;
            Transform tp2 = p2;

            while (tp1.parent != null)
            {
                tp1 = tp1.parent;
                rp1.Insert(0, tp1);
            }

            while (tp2.parent != null)
            {
                tp2 = tp2.parent;
                rp2.Insert(0, tp2);
            }

            Transform p = null;

            for (int i = 0; i < Mathf.Min(rp1.Count, rp2.Count); i++)
            {
                if (rp1[i] == rp2[i]) p = rp1[i];
                else break;
            }

            return p;
        }

        [MenuItem("Edit/Group", false, 120)]
        [MenuItem("GameObject/Group", false, 0)]
        public static void GroupSelection()
        {
            GroupTargets(Selection.gameObjects);
        }

        public static void GroupTargets(params GameObject[] targets)
        {
            if (targets.Length == 0) return;

            _targets = targets;
            InputDialog.Show("Enter name of GameObject", defaultName, OnCreateGroup);
        }

        private static void OnCreateGroup(string name)
        {
            if (_targets == null || _targets.Length == 0) return;

            Undo.SetCurrentGroupName("Group GameObjects");
            int group = Undo.GetCurrentGroup();

            GameObject go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, go.name);

            Vector3 p = Vector3.zero;
            p = _targets.Aggregate(p, (c, g) => c + g.transform.position) / _targets.Length;
            go.transform.position = p;
            Transform parent = _targets[0].transform;

            for (int i = 1; i < _targets.Length; i++) parent = FindParent(parent, _targets[i].transform);
            
            foreach (GameObject g in _targets) Undo.SetTransformParent(g.transform, go.transform, g.name);

            if (parent != null) Undo.SetTransformParent(go.transform, parent, go.name);
            Selection.activeGameObject = go;

            Undo.CollapseUndoOperations(group);

            _targets = null;
        }

        private static void OnInvoke()
        {
            Event e = Event.current;
            if (e.keyCode == Prefs.groupKeyCode && e.modifiers == Prefs.groupModifiers) GroupSelection();
        }

        [MenuItem("GameObject/Group", true)]
        [MenuItem("Edit/Group", true)]
        public static bool ValidateGroup()
        {
            return Selection.gameObjects.Length > 0;
        }
    }
}