using System.Collections.Generic;
using UnityEngine;

public class WalkableArea : MonoBehaviour
{
    [SerializeField]
    private List<Vector2> _points = new()
    {
        new Vector2(-5f, -5f),
        new Vector2(-5f, 5f),
        new Vector2(5f, 5f),
        new Vector2(5f, -5f)
    };

    public List<Vector2> Points => _points;

    private static readonly List<WalkableArea> _activeAreas = new();

    private void OnEnable()
    {
        if (!_activeAreas.Contains(this))
            _activeAreas.Add(this);
    }

    private void OnDisable()
    {
        _activeAreas.Remove(this);
    }

    public static Vector3 GetSafeMovementFromAll(Vector3 position, Vector3 movement)
    {
        if (_activeAreas.Count == 0)
            return movement;

        Vector3 horizontal = new Vector3(movement.x, 0f, movement.z);

        if (horizontal.sqrMagnitude < 0.00000001f)
            return movement;

        Vector3 current = position;
        Vector3 remaining = horizontal;

        const float padding = 0.0001f;

        // Permite deslizar por até 4 arestas no mesmo frame.
        for (int iteration = 0; iteration < 4; iteration++)
        {
            if (remaining.sqrMagnitude < 0.00000001f)
                break;

            Vector3 target = current + remaining;

            // Movimento completamente permitido.
            if (IsInsideAnyArea(target) &&
                !TryFindBoundaryCrossing(current, target, out _, out _))
            {
                current = target;
                break;
            }

            // Encontra a primeira borda cruzada.
            if (!TryFindBoundaryCrossing(
                current,
                target,
                out float hitTime,
                out Vector3 tangent))
            {
                break;
            }

            // Avança até pouco antes da borda.
            float safeTime = Mathf.Max(
                 0f,
                 hitTime - padding / remaining.magnitude
             );

            Vector3 advance = remaining * safeTime;

            current += advance;

            // Calcula o movimento restante.
            Vector3 leftover = remaining * (1f - hitTime);

            // Preserva apenas a componente paralela à borda.
            remaining = Vector3.Project(leftover, tangent);
        }

        return new Vector3(
            current.x - position.x,
            movement.y,
            current.z - position.z
        );
    }

    private static bool TryFindBoundaryCrossing(
    Vector3 start,
    Vector3 end,
    out float hitTime,
    out Vector3 hitTangent)
    {
        hitTime = float.MaxValue;
        hitTangent = Vector3.zero;

        bool found = false;

        Vector2 p = new Vector2(start.x, start.z);
        Vector2 r = new Vector2(end.x - start.x, end.z - start.z);

        const float vertexTolerance = 0.001f;
        const float probeDistance = 0.01f;

        foreach (WalkableArea area in _activeAreas)
        {
            if (area.Points.Count < 3)
                continue;

            if (!area.ContainsPoint(area.WorldToPoint(start)))
                continue;

            for (int i = 0; i < area.Points.Count; i++)
            {
                Vector3 worldA = area.PointToWorld(area.Points[i]);
                Vector3 worldB = area.PointToWorld(
                    area.Points[(i + 1) % area.Points.Count]
                );

                Vector2 q = new Vector2(worldA.x, worldA.z);
                Vector2 s = new Vector2(
                    worldB.x - worldA.x,
                    worldB.z - worldA.z
                );

                float cross = Cross2D(r, s);

                if (Mathf.Abs(cross) < 0.000001f)
                    continue;

                Vector2 delta = q - p;

                float t = Cross2D(delta, s) / cross;
                float u = Cross2D(delta, r) / cross;

                if (t < 0f || t > 1f || u < 0f || u > 1f)
                    continue;

                Vector3 hitPoint = Vector3.Lerp(start, end, t);

                // Ignora arestas que não representam a saída
                // da união das WalkableAreas.
                Vector3 afterHit = hitPoint
                    + (end - start).normalized * 0.001f;

                if (IsInsideAnyArea(afterHit))
                    continue;

                Vector3 tangent = new Vector3(s.x, 0f, s.y).normalized;

                // Projeta o movimento restante nesta aresta.
                Vector3 leftover = end - hitPoint;
                Vector3 slide = Vector3.Project(leftover, tangent);

                // Verifica se a direção do slide é válida.
                bool validSlide = false;

                if (slide.sqrMagnitude > 0.00000001f)
                {
                    Vector3 probe = hitPoint
                        + slide.normalized * probeDistance;

                    // Testa também um ponto ligeiramente
                    // antes da borda para evitar erros numéricos.
                    Vector3 insideOffset = start - hitPoint;
                    insideOffset.y = 0f;

                    if (insideOffset.sqrMagnitude > 0.00000001f)
                        probe += insideOffset.normalized * 0.001f;

                    validSlide = IsInsideAnyArea(probe);
                }

                if (!found || t < hitTime - vertexTolerance)
                {
                    hitTime = t;
                    hitTangent = tangent;
                    found = true;
                }
                else if (Mathf.Abs(t - hitTime) <= vertexTolerance)
                {
                    // Duas arestas no mesmo vértice:
                    // prefere a tangente que permite continuar.
                    Vector3 previousSlide = Vector3.Project(
                        leftover,
                        hitTangent
                    );

                    Vector3 previousProbe = hitPoint;

                    if (previousSlide.sqrMagnitude > 0.00000001f)
                        previousProbe += previousSlide.normalized * probeDistance;

                    Vector3 inward = start - hitPoint;
                    inward.y = 0f;

                    if (inward.sqrMagnitude > 0.00000001f)
                        previousProbe += inward.normalized * 0.001f;

                    bool previousValid = IsInsideAnyArea(previousProbe);

                    if (validSlide && !previousValid)
                    {
                        hitTangent = tangent;
                    }
                    else if (validSlide == previousValid &&
                             slide.sqrMagnitude > previousSlide.sqrMagnitude)
                    {
                        hitTangent = tangent;
                    }
                }
            }
        }

        return found;
    }

    private static float Cross2D(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }

    private static bool IsInsideAnyArea(Vector3 position)
    {
        foreach (WalkableArea area in _activeAreas)
        {
            if (area.Points.Count < 3)
                continue;

            if (area.ContainsPoint(area.WorldToPoint(position)))
                return true;
        }

        return false;
    }

    public bool ContainsPoint(Vector2 point)
    {
        bool inside = false;

        for (int i = 0, j = _points.Count - 1; i < _points.Count; j = i++)
        {
            Vector2 a = _points[i];
            Vector2 b = _points[j];

            bool intersects =
                (a.y > point.y) != (b.y > point.y) &&
                point.x < (b.x - a.x) *
                (point.y - a.y) / (b.y - a.y) + a.x;

            if (intersects)
                inside = !inside;
        }

        return inside;
    }

    public Vector2 WorldToPoint(Vector3 worldPosition)
    {
        Vector3 local = transform.InverseTransformPoint(worldPosition);

        return new Vector2(local.x, local.z);
    }

    public Vector3 PointToWorld(Vector2 point)
    {
        return transform.TransformPoint(
            new Vector3(point.x, 0f, point.y)
        );
    }

    private void OnDrawGizmos()
    {
        if (_points == null || _points.Count < 2)
            return;

        Gizmos.color = new Color(0f, 0.8f, 1f, 1f);

        for (int i = 0; i < _points.Count; i++)
        {
            Vector3 a = PointToWorld(_points[i]);

            Vector3 b = PointToWorld(
                _points[(i + 1) % _points.Count]
            );

            Gizmos.DrawLine(a, b);
        }
    }
}