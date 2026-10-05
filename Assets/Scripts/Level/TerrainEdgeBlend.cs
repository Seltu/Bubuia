using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Terrain))]
public class TerrainEdgeBlend : MonoBehaviour
{
    [Header("Source")]
    [SerializeField] private Terrain _sourceTerrain;

    [Header("Blend")]
    [SerializeField] private float _blendDistance = 30f;

    [SerializeField]
    private AnimationCurve _blendCurve =
        AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    private Terrain _terrain;

    private void OnEnable()
    {
        _terrain = GetComponent<Terrain>();
    }

    [ContextMenu("Blend Terrain Edge")]
    public void BlendTerrainEdge()
    {
        if (_sourceTerrain == null)
        {
            Debug.LogError("Source Terrain não definido.");
            return;
        }

        if (_terrain == null)
            _terrain = GetComponent<Terrain>();

        TerrainData terrainData = _terrain.terrainData;

        int resolution = terrainData.heightmapResolution;

        float[,] heights =
            terrainData.GetHeights(0, 0, resolution, resolution);

        Vector3 targetPosition = transform.position;
        Vector3 sourcePosition = _sourceTerrain.transform.position;

        Vector3 targetSize = terrainData.size;
        Vector3 sourceSize = _sourceTerrain.terrainData.size;

        bool sourceLeft = Mathf.Approximately(
            targetPosition.x,
            sourcePosition.x + sourceSize.x
        );

        bool sourceRight = Mathf.Approximately(
            targetPosition.x + targetSize.x,
            sourcePosition.x
        );

        bool sourceBottom = Mathf.Approximately(
            targetPosition.z,
            sourcePosition.z + sourceSize.z
        );

        bool sourceTop = Mathf.Approximately(
            targetPosition.z + targetSize.z,
            sourcePosition.z
        );

        if (!sourceLeft &&
            !sourceRight &&
            !sourceBottom &&
            !sourceTop)
        {
            Debug.LogError(
                "O Source Terrain não está adjacente a este Terrain."
            );

            return;
        }

        float metersPerSampleX =
            targetSize.x / (resolution - 1);

        float metersPerSampleZ =
            targetSize.z / (resolution - 1);

        if (sourceLeft || sourceRight)
        {
            BlendVerticalEdge(
                heights,
                resolution,
                metersPerSampleX,
                sourceLeft
            );
        }
        else
        {
            BlendHorizontalEdge(
                heights,
                resolution,
                metersPerSampleZ,
                sourceBottom
            );
        }

        terrainData.SetHeights(
            0,
            0,
            heights
        );

        _terrain.Flush();
    }

    private void BlendVerticalEdge(
        float[,] heights,
        int resolution,
        float metersPerSample,
        bool sourceIsLeft)
    {
        int samples =
            Mathf.CeilToInt(
                _blendDistance / metersPerSample
            );

        samples = Mathf.Clamp(
            samples,
            1,
            resolution - 1
        );

        for (int z = 0; z < resolution; z++)
        {
            float normalizedZ =
                z / (float)(resolution - 1);

            Vector3 worldPosition =
                new Vector3(
                    sourceIsLeft
                        ? transform.position.x
                        : transform.position.x +
                          _terrain.terrainData.size.x,

                    0f,

                    transform.position.z +
                    normalizedZ *
                    _terrain.terrainData.size.z
                );

            float sourceBorderHeight =
                GetSourceHeightNormalized(
                    worldPosition
                );

            for (int i = 0; i <= samples; i++)
            {
                int x = sourceIsLeft
                    ? i
                    : resolution - 1 - i;

                float t =
                    i / (float)samples;

                float influence =
                    _blendCurve.Evaluate(t);

                float originalHeight =
                    heights[z, x];

                heights[z, x] =
                    Mathf.Lerp(
                        originalHeight,
                        sourceBorderHeight,
                        influence
                    );
            }
        }
    }

    private void BlendHorizontalEdge(
        float[,] heights,
        int resolution,
        float metersPerSample,
        bool sourceIsBottom)
    {
        int samples =
            Mathf.CeilToInt(
                _blendDistance / metersPerSample
            );

        samples = Mathf.Clamp(
            samples,
            1,
            resolution - 1
        );

        for (int x = 0; x < resolution; x++)
        {
            float normalizedX =
                x / (float)(resolution - 1);

            Vector3 worldPosition =
                new Vector3(
                    transform.position.x +
                    normalizedX *
                    _terrain.terrainData.size.x,

                    0f,

                    sourceIsBottom
                        ? transform.position.z
                        : transform.position.z +
                          _terrain.terrainData.size.z
                );

            float sourceBorderHeight =
                GetSourceHeightNormalized(
                    worldPosition
                );

            for (int i = 0; i <= samples; i++)
            {
                int z = sourceIsBottom
                    ? i
                    : resolution - 1 - i;

                float t =
                    i / (float)samples;

                float influence =
                    _blendCurve.Evaluate(t);

                float originalHeight =
                    heights[z, x];

                heights[z, x] =
                    Mathf.Lerp(
                        originalHeight,
                        sourceBorderHeight,
                        influence
                    );
            }
        }
    }

    private float GetSourceHeightNormalized(
        Vector3 worldPosition)
    {
        TerrainData sourceData =
            _sourceTerrain.terrainData;

        Vector3 sourcePosition =
            _sourceTerrain.transform.position;

        float normalizedX =
            (worldPosition.x - sourcePosition.x) /
            sourceData.size.x;

        float normalizedZ =
            (worldPosition.z - sourcePosition.z) /
            sourceData.size.z;

        normalizedX =
            Mathf.Clamp01(normalizedX);

        normalizedZ =
            Mathf.Clamp01(normalizedZ);

        float height =
            sourceData.GetInterpolatedHeight(
                normalizedX,
                normalizedZ
            );

        float worldHeight =
            sourcePosition.y + height;

        float localTargetHeight =
            worldHeight -
            transform.position.y;

        return Mathf.Clamp01(
            localTargetHeight /
            _terrain.terrainData.size.y
        );
    }
}