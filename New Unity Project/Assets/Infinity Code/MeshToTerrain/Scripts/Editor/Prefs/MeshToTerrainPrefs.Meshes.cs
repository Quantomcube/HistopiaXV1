/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.MeshToTerrain
{
    public partial class MeshToTerrainPrefs
    {
        private bool showMeshes = true;

        private void MeshesUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            showMeshes = Foldout(showMeshes, "Meshes");
            if (showMeshes)
            {
                meshFindType = (MeshToTerrainFindType)EditorGUILayout.EnumPopup("Mesh select type", meshFindType);

                if (meshFindType == MeshToTerrainFindType.gameObjects) MeshesUIGameObjects();
                else if (meshFindType == MeshToTerrainFindType.layers) meshLayer = EditorGUILayout.LayerField("Layer", meshLayer);

                direction = (MeshToTerrainDirection)EditorGUILayout.EnumPopup("Direction", direction);
                if (direction == MeshToTerrainDirection.reversed) GUILayout.Label("Use the reverse direction, if that model has inverted the normal.");

                yRange = (MeshToTerrainYRange)EditorGUILayout.EnumPopup("Y Range", yRange);
                if (yRange == MeshToTerrainYRange.fixedValue) yRangeValue = EditorGUILayout.IntField("Y Range Value", yRangeValue);
            }
            EditorGUILayout.EndVertical();
        }

        private void MeshesUIGameObjects()
        {
            bool hasEmpty = false;
            for (int i = 0; i < meshes.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                meshes[i] = (GameObject)EditorGUILayout.ObjectField("Mesh " + (i + 1), meshes[i], typeof(GameObject), true);
                if (GUILayout.Button("X", GUILayout.ExpandWidth(false))) meshes[i] = null;
                EditorGUILayout.EndHorizontal();

                if (meshes[i] == null) hasEmpty = true;
            }
            if (hasEmpty) meshes.RemoveAll(m => m == null);

            GameObject newMesh = (GameObject)EditorGUILayout.ObjectField("Mesh GameObject", null, typeof(GameObject), true);
            if (newMesh != null)
            {
                if (newMesh.scene.name == null)
                {
                    EditorUtility.DisplayDialog("Error", "GameObject must be in the scene, not in the project!", "OK");
                }
                else if (!meshes.Contains(newMesh)) meshes.Add(newMesh);
                else EditorUtility.DisplayDialog("Error", "The selected GameObject is already in the list!", "OK");
            }
        }
    }
}