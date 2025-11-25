using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawFishingLine : MonoBehaviour
{
    [SerializeField] private Transform[] _points;
    private LineRenderer _lineRenderer;

    private void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();

        DrawLines();
    }

    private void Update()
    {
        for (int i = 0; i < _points.Length; i++)
        {
            if (!_points[i].gameObject.activeSelf)
            {
                _lineRenderer.enabled = false;
                return;
            }
            _lineRenderer.enabled = true;
            _lineRenderer.SetPosition(i, _points[i].position);
        }
    }

    private void DrawLines()
    {
        _lineRenderer.positionCount = _points.Length;
    }
}
