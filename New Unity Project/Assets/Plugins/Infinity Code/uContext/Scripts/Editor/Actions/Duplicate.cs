/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.uContext.Attributes;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext.Actions
{
    [RequireSingleGameObject]
    public class Duplicate : ActionItem
    {
        public override float order
        {
            get { return -900; }
        }

        protected override void Init()
        {
            guiContent = new GUIContent(Icons.duplicate, "Duplicate");
        }

        public override void Invoke()
        {
            SceneView.lastActiveSceneView.Focus(); 
            EditorApplication.ExecuteMenuItem("Edit/Duplicate");
        }
    }
}