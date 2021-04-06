/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.MeshToTerrain
{
    public partial class MeshToTerrain
    {
        private void Dispose()
        {
            if (meshContainer != null) meshContainer.transform.localScale = Vector3.one;

            FinalizeMeshes();

            if (meshContainer != null)
            {
                DestroyImmediate(meshContainer);
                meshContainer = null;
            }

            checkedTextures = null;
            colors = null;
            heightmap = null;
            lastX = 0;
            lastTransform = null;
            lastMesh = null;
            lastTriangles = null;
            lastVerticles = null;
            material = null;
            mainTexture = null;

            EditorUtility.UnloadUnusedAssetsImmediate();
            GC.Collect();
        }

        private static void FinalizeMeshes()
        {
            foreach (MeshToTerrainObject m in terrainObjects)
            {
                m.gameobject.layer = m.layer;
                if (m.tempCollider != null)
                {
                    DestroyImmediate(m.tempCollider);
                    m.tempCollider = null;
                }

                if (m.changedParent && !m.temporary)
                {
                    m.gameobject.transform.parent = m.originalParent;
                    m.originalParent = null;
                }

                m.gameobject = null;
            }

            terrainObjects = null;
        }

        private void Finish()
        {
#if RTP
            if (prefs.generateTextures)
            {
                ReliefTerrain reliefTerrain = prefs.terrains[0].GetComponent<ReliefTerrain>();
                ReliefTerrainGlobalSettingsHolder settingsHolder = reliefTerrain.globalSettingsHolder;

                settingsHolder.numLayers = 4;
                settingsHolder.splats = new Texture2D[4];
                settingsHolder.Bumps = new Texture2D[4];
                settingsHolder.Heights = new Texture2D[4];

                for (int i = 0; i < 4; i++)
                {
                    settingsHolder.splats[i] = rtpTextures[i * 3];
                    settingsHolder.Heights[i] = rtpTextures[i * 3 + 1];
                    settingsHolder.Bumps[i] = rtpTextures[i * 3 + 2];
                }

                settingsHolder.GlobalColorMapBlendValues = new Vector3(1, 1, 1);
                settingsHolder._GlobalColorMapNearMIP = 1;
                settingsHolder.GlobalColorMapSaturation = 1;
                settingsHolder.GlobalColorMapSaturationFar = 1;
                settingsHolder.GlobalColorMapBrightness = 1;
                settingsHolder.GlobalColorMapBrightnessFar = 1;

                foreach (Terrain item in prefs.terrains) item.GetComponent<ReliefTerrain>().RefreshTextures();

                settingsHolder.Refresh();
            }
#endif

            if (prefs.adjustMeshSize)
            {
                float w = originalBoundsRange.x;
                float h = originalBoundsRange.z;

                float sW = w / prefs.newTerrainCountX;
                float sH = h / prefs.newTerrainCountY;
                float sY = originalBoundsRange.y * 1.5f;

                float tsw = sW;
                float tsh = sH;

                if (prefs.textureCaptureMode == MeshToTerrainTextureCaptureMode.camera)
                {
                    tsw = tsw / prefs.textureWidth * (prefs.textureWidth + 4);
                    tsh = tsh / prefs.textureHeight * (prefs.textureHeight + 4);
                }
                else
                {
                    tsw = tsw / prefs.textureWidth * (prefs.textureWidth + 2);
                    tsh = tsh / prefs.textureHeight * (prefs.textureHeight + 2);
                }

                float offX = (w - sW * prefs.newTerrainCountX) / 2;
                float offY = (h - sH * prefs.newTerrainCountY) / 2;

                for (int x = 0; x < prefs.newTerrainCountX; x++)
                {
                    for (int y = 0; y < prefs.newTerrainCountY; y++)
                    {
                        Terrain t = prefs.terrains[y * prefs.newTerrainCountX + x];
                        t.transform.localPosition = new Vector3(x * sW + offX, 0, y * sH + offY);
                        t.terrainData.size = new Vector3(sW, sY, sH);

#if !RTP
                        if (prefs.generateTextures && prefs.textureResultType == MeshToTerrainTextureResultType.regularTexture)
                        {

                            TerrainLayer[] terrainLayers = t.terrainData.terrainLayers;
                            TerrainLayer layer = terrainLayers[0];

                            layer.tileSize = new Vector2(tsw, tsh);
                            layer.tileOffset = new Vector2(t.terrainData.size.x / prefs.textureWidth / 1.5f, t.terrainData.size.z / prefs.textureHeight / 1.5f);

                            t.terrainData.terrainLayers = terrainLayers;
                        }
#endif
                    }
                }
            }

            if (prefs.generateTextures && prefs.textureCaptureMode == MeshToTerrainTextureCaptureMode.camera && prefs.setAmbientLight)
            {
                PrevLightingSettings.Restore();
            }

            if (prefs.terrainType == MeshToTerrainSelectTerrainType.newTerrains) EditorGUIUtility.PingObject(container);
            else
            {
                foreach (Terrain t in prefs.terrains) EditorGUIUtility.PingObject(t.gameObject);
            }

            Dispose();

            phase = MeshToTerrainPhase.idle;
        }
    }
}