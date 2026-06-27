using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class TerrainGenerator : MonoBehaviour
{
    [Header("Terrain Size")]
    [SerializeField] private int terrainWidth = 256;
    [SerializeField] private int terrainDepth = 256;
    [SerializeField] private float terrainHeight = 50f;
    [SerializeField] private float tiltStrength = 0.5f;
    [SerializeField] private float maxHeight = 1.0f;

    [Header("Decorations")]
    [SerializeField] private float waterHeight;
    [SerializeField] private float sandHeight;
    [SerializeField] private List<DecorationLayer> decorationLayers = new();


    [Header("Resolution")]
    [Tooltip("Allowed: 33, 65, 129, 257, 513")]
    [SerializeField] private int heightmapResolution = 129;

    [Header("Perlin Noise")]
    [SerializeField] private float noiseScaleShallow = 5f;  // pouco incline, pouco detalhe
    [SerializeField] private float noiseScaleSteep = 25f; // muito incline, muito detalhe

    [Header("Terrain Layers")]
    [SerializeField] private TerrainLayer[] terrainLayers;
    [SerializeField] private TerrainHeightLayer[] terrainHeights;
    // Listas separadas pois a unity não suporta classes que usam TerrainLayer como campo, por algum motivo

    [Header("Islands")]
    [SerializeField] private float islandNoiseScale = 2.2f;
    [SerializeField, Range(1, 8)] private int islandOctaves = 4;
    [SerializeField] private float islandLacunarity = 2.0f;
    [SerializeField] private float islandPersistence = 0.5f;

    [SerializeField, Range(0f, 1f)] private float landThreshold = 0.58f;
    [SerializeField, Range(0.001f, 0.2f)] private float shoreBlend = 0.05f;

    // quanto a ilha “levanta” (em altura normalizada 0..1)
    [SerializeField] private float islandAddStrength = 0.55f;

    // opcional: oceano nas bordas
    [SerializeField, Range(0f, 1f)] private float edgeFalloffStrength = 0.5f;
    
    //Perlin Offsets
    private float offsetX = 0f;
    private float offsetZ = 0f;

    private Terrain _terrain;

    private void Awake()
    {
        _terrain = GetComponent<Terrain>();
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
        if (_terrain == null)
            _terrain = GetComponent<Terrain>();

        if (_terrain == null)
        {
            Debug.LogError("No Terrain component found!");
            return;
        }

        // Randomize offsets so generation changes each time
        offsetX = Random.Range(0f, 9999f);
        offsetZ = Random.Range(0f, 9999f);

        GenerateTerrain();
        GenerateWaterBoundary();
        ApplyTerrainLayers();
        Decorate();
    }

    private void GenerateTerrain()
    {
        TerrainData data = _terrain.terrainData;

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


                //Adição de ilhas
                float tx = (float)x / (res - 1);
                float tz = (float)z / (res - 1);

                // noise field FBM 0..1
                float n = Fbm(tx * islandNoiseScale + offsetX,
                                tz * islandNoiseScale + offsetZ,
                                islandOctaves, islandLacunarity, islandPersistence);

                // falloff de borda (Impede de spawnar ilhas nas bordas do mapa)
                float falloff = EdgeFalloff(tx, tz);
                n = Mathf.Lerp(n, 0f, falloff * edgeFalloffStrength);

                // máscara de terra suave (0 água -> 1 terra)
                float mask = Mathf.SmoothStep(0f, 1f,
                    Mathf.InverseLerp(landThreshold - shoreBlend, landThreshold + shoreBlend, n));

                // levanta as ilhas SEM mexer no tilt
                finalHeight = Mathf.Clamp(finalHeight + mask * islandAddStrength, 0f, maxHeight);

                heights[z, x] = finalHeight;
            }
        }

        data.SetHeights(0, 0, heights);
    }

    // FBM: soma de oitavas de Perlin e normaliza pra 0..1
    private float Fbm(float x, float z, int octaves, float lacunarity, float persistence)
    {
        float amp = 1f;
        float freq = 1f;
        float sum = 0f;
        float max = 0f;

        for (int i = 0; i < octaves; i++)
        {
            float n = Mathf.PerlinNoise(x * freq, z * freq); // 0..1
            sum += n * amp;
            max += amp;

            amp *= persistence;
            freq *= lacunarity;
        }

        return (max > 0f) ? (sum / max) : 0f; // 0..1
    }

    // Falloff radial/“quadrado arredondado”: 0 no centro, 1 nas bordas
    private float EdgeFalloff(float tx, float tz)
    {
        float dx = Mathf.Abs(tx * 2f - 1f);
        float dz = Mathf.Abs(tz * 2f - 1f);
        float d = Mathf.Max(dx, dz); // borda em forma de “quadrado”
                                     // curva pra deixar centro amplo e borda cair rápido
        return Mathf.SmoothStep(0f, 1f, Mathf.Pow(d, 3.0f));
    }


    private void GenerateWaterBoundary()
    {
        List<Vector3> shorelinePoints = new();

        float waterY = waterHeight;
        TerrainData data = _terrain.terrainData;
        int res = data.heightmapResolution;
        float[,] heights = data.GetHeights(0, 0, res, res);

        for (int z = 1; z < res - 1; z++)
        {
            for (int x = 1; x < res - 1; x++)
            {
                float hWorld =
                    heights[z, x] * data.size.y + _terrain.transform.position.y;

                bool isLand = hWorld > waterY;

                bool neighborWater =
                    heights[z + 1, x] * data.size.y + _terrain.transform.position.y <= waterY ||
                    heights[z - 1, x] * data.size.y + _terrain.transform.position.y <= waterY ||
                    heights[z, x + 1] * data.size.y + _terrain.transform.position.y <= waterY ||
                    heights[z, x - 1] * data.size.y + _terrain.transform.position.y <= waterY;

                if (isLand && neighborWater)
                {
                    Vector3 worldPos = new Vector3(
                        (float)x / (res - 1) * data.size.x,
                        waterY,
                        (float)z / (res - 1) * data.size.z
                    ) + _terrain.transform.position;

                    shorelinePoints.Add(worldPos);
                }
            }
        }

        Transform barrierRoot = transform.Find("WaterBoundary");
        if (barrierRoot != null)
            DestroyImmediate(barrierRoot.gameObject);

        barrierRoot = new GameObject("WaterBoundary").transform;
        barrierRoot.parent = transform;

        foreach (var p in shorelinePoints)
        {
            GameObject wall = new GameObject("InvisibleWaterWall");
            wall.transform.position = new Vector3(p.x, waterHeight, p.z);
            wall.transform.parent = barrierRoot;

            CapsuleCollider col = wall.AddComponent<CapsuleCollider>();
            col.height = 20f;
            col.radius = 3f;
            col.center = new Vector3(0, 0, 0);
            wall.layer = LayerMask.NameToLayer("Barrier");
            wall.tag = "Obstacle";
        }
    }

    private void Decorate()
    {
        if (_terrain == null || decorationLayers == null) return;

        TerrainData data = _terrain.terrainData;
        Vector3 terrainPos = _terrain.transform.position;

        Transform decorRoot = transform.Find("Decorations");
        if (decorRoot != null)
            DestroyImmediate(decorRoot.gameObject);

        decorRoot = new GameObject("Decorations").transform;
        decorRoot.parent = transform;

        List<Vector3> placedPositions = new List<Vector3>();

        foreach (var layer in decorationLayers)
        {
            if (layer.prefabs == null || layer.prefabs.Count == 0)
                continue;

            Transform layerRoot = new GameObject(layer.type.ToString()).transform;
            layerRoot.parent = decorRoot;

            int max = Mathf.RoundToInt(
                layer.density * terrainWidth * terrainDepth * 0.001f
            );

            int placed = 0;
            int safety = max * 5;

            while (placed < max && safety-- > 0)
            {
                float x = Random.Range(0f, terrainWidth);
                float z = Random.Range(0f, terrainDepth);

                float normX = x / terrainWidth;
                float normZ = z / terrainDepth;

                float terrainHeightAtPoint =
                    data.GetInterpolatedHeight(normX, normZ) + terrainPos.y;

                Vector3 spawnPos;

                switch (layer.type)
                {
                    case DecorationLayerType.Underwater:
                        if (terrainHeightAtPoint >= waterHeight)
                            continue;

                        spawnPos = new Vector3(
                            x + terrainPos.x,
                            terrainHeightAtPoint,
                            z + terrainPos.z
                        );
                        break;

                    case DecorationLayerType.WaterSurface:
                        if (terrainHeightAtPoint > waterHeight)
                            continue;

                        spawnPos = new Vector3(
                            x + terrainPos.x,
                            waterHeight,
                            z + terrainPos.z
                        );
                        break;

                    case DecorationLayerType.Ground:
                        if (terrainHeightAtPoint <= sandHeight)
                            continue;

                        spawnPos = new Vector3(
                            x + terrainPos.x,
                            terrainHeightAtPoint,
                            z + terrainPos.z
                        );
                        break;

                    default:
                        continue;
                }

                // Raycast só para Ground e Underwater
                if (layer.type != DecorationLayerType.WaterSurface)
                {
                    if (!Physics.Raycast(
                        spawnPos + Vector3.up * 50f,
                        Vector3.down,
                        100f))
                        continue;

                    if (Physics.Raycast(
                        spawnPos + Vector3.up * 50f,
                        Vector3.down,
                        out RaycastHit hit,
                        100f, 3))
                    spawnPos = hit.point;
                }

                // Distancia minima entre decorações
                bool tooClose = false;
                float minDistSqr = layer.minDistance * layer.minDistance;

                foreach (var p in placedPositions)
                {
                    if (Vector3.SqrMagnitude(p - spawnPos) < minDistSqr)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (tooClose)
                    continue;

                // Instanciação

                GameObject prefab =
                    layer.prefabs[Random.Range(0, layer.prefabs.Count)];

                GameObject deco = Instantiate(
                    prefab,
                    spawnPos,
                    Quaternion.identity,
                    layerRoot
                );

                deco.transform.rotation = Quaternion.Euler(
                    0f,
                    Random.Range(0f, 360f),
                    0f
                );

                float scale = Random.Range(0.8f, 1.2f);
                deco.transform.localScale *= scale;

                placedPositions.Add(spawnPos);
                placed++;
            }
        }
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


}