using System.Collections.Generic;
using UnityEngine;

public class RopeVerlet : MonoBehaviour
{
    [Header("Rope")]
    [SerializeField] private int _numRopeSegments = 50;
    [SerializeField] private float _ropeSegmentLengh = 0.225f;

    [Header("Physics")]
    [SerializeField] private Vector2 _gravityForce = new Vector2(0f, 2f);
    [SerializeField] private float _dampingFactor = 0.98f;
    // Colision
    [SerializeField] private LayerMask _collisionMask;
    [SerializeField] private float _collisionRadius = 0.1f;
    [SerializeField] private float _bounceFactor = 0.1f;

    [Header("Constraints")]
    [SerializeField] private int _numConstraintsRuns = 50;

    [Header("Optmizations")]
    [SerializeField] private int _collisionSegmentInterval = 2;

    [Header("Rope Points")]
    [SerializeField] private Transform _rodEndPoint;
    [SerializeField] private Transform _bobberObject;
    private Vector3 _ropeStartPoint;

    private LineRenderer _lineRederer;
    private List<RopeSegment> _ropeSegments = new List<RopeSegment>();


    public struct RopeSegment
    {
        public Vector2 currentPosition;
        public Vector2 oldPosition;

        public RopeSegment(Vector2 pos)
        {
            currentPosition = pos;
            oldPosition = pos;
        }
    }


    private void Awake()
    {
        _lineRederer = GetComponent<LineRenderer>();
        _lineRederer.positionCount = _numConstraintsRuns;

        // ponta da vara
        _ropeStartPoint = _rodEndPoint.position;

        for (int i = 0; i < _numRopeSegments; i++)
        {
            _ropeSegments.Add(new RopeSegment(_ropeStartPoint));
            _ropeStartPoint.y -= _ropeSegmentLengh;
        }
    }

    private void Update()
    {
        DrawRope();

        if (_bobberObject != null) _bobberObject.position = _ropeSegments[_ropeSegments.Count - 1].currentPosition;
    }

    private void FixedUpdate()
    {
        // ponta da vara
        _ropeStartPoint = _rodEndPoint.position;

        Simulate();

        for (int i = 0; i < _numConstraintsRuns; i++)
        {
            ApplyConstraints();

            if (i % _collisionSegmentInterval == 0)
                HandleCollisions();
        }

        var bobbleSegment = _ropeSegments[_ropeSegments.Count - 1];
        Vector2 bobbleVelocity = (bobbleSegment.currentPosition - bobbleSegment.oldPosition) / Time.fixedDeltaTime;
        _bobberObject.GetComponent<Rigidbody2D>().linearVelocity = bobbleVelocity;
    }

    private void DrawRope()
    {
        Vector3[] ropePositions = new Vector3[_numRopeSegments];

        for (int i = 0; i < _numRopeSegments; i++)
        {
            ropePositions[i] = _ropeSegments[i].currentPosition;
        }

        _lineRederer.SetPositions(ropePositions);
    }

    private void ApplyConstraints()
    {
        // prendendo a corda na ponta da vara
        RopeSegment firstSegment = _ropeSegments[0];
        firstSegment.currentPosition = _ropeStartPoint;
        _ropeSegments[0] = firstSegment;

        for (int i = 0; i < _numRopeSegments - 1; i++)
        {
            RopeSegment currentSegment = _ropeSegments[i];
            RopeSegment nextSegment = _ropeSegments[i + 1];

            float dist = (currentSegment.currentPosition - nextSegment.currentPosition).magnitude;
            float diff = dist - _ropeSegmentLengh;

            Vector2 changeDir = (currentSegment.currentPosition - nextSegment.currentPosition).normalized;
            Vector2 changeVector = changeDir * diff;

            if (i != 0)
            {
                currentSegment.currentPosition -= (changeVector * 0.5f);
                nextSegment.currentPosition += (changeVector * 0.5f);
            }
            else
            {
                nextSegment.currentPosition += changeVector;
            }

            _ropeSegments[i] = currentSegment;
            _ropeSegments[i + 1] = nextSegment;
        }
    }

    private void Simulate()
    {
        for (int i = 0; i < _ropeSegments.Count; i++)
        {
            RopeSegment segment = _ropeSegments[i];
            Vector2 velocity = (segment.currentPosition - segment.oldPosition) * _dampingFactor;

            if (i == _ropeSegments.Count - 1) velocity += _gravityForce * Time.fixedDeltaTime * 1.5f;

            segment.oldPosition = segment.currentPosition;
            segment.currentPosition += velocity;
            segment.oldPosition += _gravityForce * Time.fixedDeltaTime;

            _ropeSegments[i] = segment;
        }
    }

    private void HandleCollisions()
    {
        for (int i = 0; i < _ropeSegments.Count; i++)
        {
            RopeSegment segment = _ropeSegments[i];
            Vector2 velocity = segment.currentPosition - segment.oldPosition;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(segment.currentPosition, _collisionRadius, _collisionMask);

            foreach (Collider2D collider in colliders)
            {
                Vector2 closestPoint = collider.ClosestPoint(segment.currentPosition);

                float dist = Vector2.Distance(segment.currentPosition, closestPoint);

                if (dist < _collisionRadius)
                {
                    Vector2 normal = (segment.currentPosition - closestPoint).normalized;
                    if (normal == Vector2.zero)
                    {
                        normal = (segment.currentPosition - (Vector2)collider.transform.position).normalized;
                    }

                    float depth = _collisionRadius - dist;
                    segment.currentPosition += normal * depth;

                    velocity = Vector2.Reflect(velocity, normal) * _bounceFactor;
                }
            }

            segment.oldPosition = segment.currentPosition - velocity;
            _ropeSegments[i] = segment;
        }
    }
}