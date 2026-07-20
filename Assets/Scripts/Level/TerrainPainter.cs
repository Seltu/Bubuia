using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class TerrainPainter : MonoBehaviour
{
    
    [Header("Terrain Layers")]
    [SerializeField] private TerrainLayer[] terrainLayers;
    [SerializeField] private TerrainHeightLayer[] terrainHeights;

    #if UNITY_EDITOR

    private Terrain _terrain;

    private void Awake()
    {
        _terrain = GetComponent<Terrain>();
    }

        [ContextMenu("Paint Terrain")]
        private void PaintInEditor()
        {
            Paint();
        }

    private void Paint()
    {
        if (_terrain == null)
            _terrain = GetComponent<Terrain>();

        if (_terrain == null)
        {
            Debug.LogError("No Terrain component found!");
            return;
        }

        ApplyTerrainLayers();
    }
    private void ApplyTerrainLayers()
    {
        if (_terrain == null || terrainHeights == null || terrainHeights.Length == 0)
            return;

        TerrainData data = _terrain.terrainData;

        // Extrai TerrainLayers
        TerrainLayer[] layers = new TerrainLayer[terrainHeights.Length];
        for (int i = 0; i < terrainHeights.Length; i++)
            layers[i] = terrainLayers[i];

        data.terrainLayers = layers;

        int alphaWidth = data.alphamapWidth;
        int alphaHeight = data.alphamapHeight;
        int layerCount = layers.Length;

        int heightmapRes = data.heightmapResolution;
        float[,] heights = data.GetHeights(0, 0, heightmapRes, heightmapRes);

        float[,,] splatmap = new float[alphaWidth, alphaHeight, layerCount];

        for (int z = 0; z < alphaHeight; z++)
        {
            for (int x = 0; x < alphaWidth; x++)
            {
                // Converte alphamap → heightmap
                int hx = Mathf.RoundToInt(
                    (float)x / (alphaWidth - 1) * (heightmapRes - 1)
                );

                int hz = Mathf.RoundToInt(
                    (float)z / (alphaHeight - 1) * (heightmapRes - 1)
                );

                // ALTURA REAL (metros)
                float worldHeight = heights[hx, hz] * data.size.y + _terrain.transform.position.y;

                float totalWeight = 0f;

                for (int l = 0; l < layerCount; l++)
                {
                    TerrainHeightLayer hl = terrainHeights[l];

                    float weight =
                    (worldHeight >= hl.minHeight && worldHeight < hl.maxHeight)
                    ? 1f
                    : 0f;

                    weight = Mathf.Clamp01(weight);
                    splatmap[x, z, l] = weight;
                    totalWeight += weight;
                }

                // Normalização obrigatória
                if (totalWeight > 0f)
                {
                    for (int l = 0; l < layerCount; l++)
                        splatmap[x, z, l] /= totalWeight;
                }
                else
                {
                    splatmap[x, z, 0] = 1f;
                }
            }
        }

        data.SetAlphamaps(0, 0, splatmap);
    }
    #endif
}