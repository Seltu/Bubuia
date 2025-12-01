using UnityEngine;

[ExecuteAlways]
public class SimplePerlinTerrain : MonoBehaviour
{
    [Header("Terrain Size")]
    public int terrainWidth = 256;
    public int terrainDepth = 256;
    public float terrainHeight = 50f;
    public float tiltStrength = 0.5f;

    [Header("Resolution")]
    [Tooltip("Allowed: 33, 65, 129, 257, 513")]
    public int heightmapResolution = 129;

    [Header("Perlin Noise")]
    public float noiseScaleShallow = 5f;  // pouco incline, pouco detalhe
    public float noiseScaleSteep = 25f; // muito incline, muito detalhe
    public float offsetX = 0f;
    public float offsetZ = 0f;

    [Header("Rendering")]
    [Tooltip("Use this for URP/HDRP/custom shaders")]
    public Material terrainMaterial;

    [Tooltip("Optional Terrain Layers (grass, rock, etc.)")]
    public TerrainLayer[] terrainLayers;

    private Terrain terrain;

    private void Awake()
    {
        terrain = GetComponent<Terrain>();
    }

    private void Start()
    {
        Regenerate();
    }

#if UNITY_EDITOR
    [ContextMenu("Regenerate Terrain")]
    private void RegenerateInEditor()
    {
        Regenerate();
    }
#endif

    private void Regenerate()
    {
        if (terrain == null)
            terrain = GetComponent<Terrain>();

        if (terrain == null)
        {
            Debug.LogError("No Terrain component found!");
            return;
        }

        // Randomize offsets so generation changes each time
        offsetX = Random.Range(0f, 9999f);
        offsetZ = Random.Range(0f, 9999f);

        GenerateTerrain();
        ApplyMaterial();
        ApplyTerrainLayers();
    }

    private void GenerateTerrain()
    {
        TerrainData data = terrain.terrainData;

        // Set resolution + size
        data.heightmapResolution = heightmapResolution;
        data.size = new Vector3(terrainWidth, terrainHeight, terrainDepth);

        int res = data.heightmapResolution;
        float[,] heights = new float[res, res];

        for (int z = 0; z < res; z++)
        {
            float t = (float)z / (res - 1);

            // quanto maior t = maior detalhe
            float localScale = Mathf.Lerp(noiseScaleShallow, noiseScaleSteep, t);

            for (int x = 0; x < res; x++)
            {
                float xCoord = (float)x / res * localScale + offsetX;
                float zCoord = (float)z / res * localScale + offsetZ;

                // Perlin 0–1
                float h = Mathf.PerlinNoise(xCoord, zCoord);

                // mínimo inicial baseado no tiltStrength
                float minBase = tiltStrength;
                float baseHeight = minBase + (1f - minBase) * h;

                // inclinação empurra tudo para baixo até 0
                float tilt = t * tiltStrength;
                float finalHeight = Mathf.Clamp01(baseHeight - tilt);

                heights[z, x] = finalHeight;
            }
        }

        data.SetHeights(0, 0, heights);
    }

    private void ApplyMaterial()
    {
        if (terrainMaterial != null)
        {
            terrain.materialTemplate = terrainMaterial;
        }
    }

    private void ApplyTerrainLayers()
    {
        if (terrainLayers != null && terrainLayers.Length > 0)
        {
            terrain.terrainData.terrainLayers = terrainLayers;
        }
    }
}
