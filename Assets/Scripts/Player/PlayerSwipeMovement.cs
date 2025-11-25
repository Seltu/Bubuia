using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSwipeMovement : MonoBehaviour
{
    [Header("Visual References")]
    [SerializeField] private SpriteRenderer[] _sprites;
    [SerializeField] private Animator _animator;

    [Header("Movement Settings")]
    [SerializeField] private List<Transform> houses;
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float swipeThreshold = 50f;
    [SerializeField] private float smoothTime = 0.2f;
    [SerializeField] private Vector3 positionOffset = new Vector3(0, 0, 0);

    private int currentIndex = 0;
    private Vector3 velocity = Vector3.zero;

    private Vector2 startTouchPos;
    private Vector2 endTouchPos;
    private bool isTouching = false;

    private bool isMoving = false;
    private Queue<int> moveQueue = new Queue<int>();
    private int moveDirection = 0;

    private void Start()
    {
        // Start the player exactly at the first house
        if (houses != null && houses.Count > 0)
            transform.position = houses[0].position + positionOffset;

        // Ensure sprites start facing right
        foreach (var sprite in _sprites)
            sprite.flipX = false;

        if (_animator != null)
            _animator.SetBool("isWalking", false);
    }

    private void Update()
    {
        HandleSwipeInput();
        HandleMovement();
    }

    private void HandleSwipeInput()
    {
        // Touchscreen input
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            var touch = Touchscreen.current.primaryTouch;

            if (!isTouching)
            {
                startTouchPos = touch.position.ReadValue();
                isTouching = true;
            }
            else if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended)
            {
                endTouchPos = touch.position.ReadValue();
                DetectSwipeDirection(endTouchPos - startTouchPos);
                isTouching = false;
            }
        }
        else if (Mouse.current != null)
        {
            // Mouse input (for Editor)
            if (Mouse.current.leftButton.wasPressedThisFrame)
                startTouchPos = Mouse.current.position.ReadValue();

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                endTouchPos = Mouse.current.position.ReadValue();
                DetectSwipeDirection(endTouchPos - startTouchPos);
            }
        }
    }

    private void DetectSwipeDirection(Vector2 swipeDelta)
    {
        if (swipeDelta.magnitude < swipeThreshold)
            return;

        float x = swipeDelta.x;
        if (Mathf.Abs(x) > Mathf.Abs(swipeDelta.y))
        {
            if (x < 0)
                QueueMove(1); // swipe right
            else
                QueueMove(-1); // swipe left
        }
    }

    private void QueueMove(int direction)
    {
        // Limit queued swipes to 2 max
        if (moveQueue.Count >= 2)
            return;

        int nextIndex = Mathf.Clamp(currentIndex + direction, 0, houses.Count - 1);
        if (nextIndex != currentIndex)
            moveQueue.Enqueue(nextIndex);
    }


    private void HandleMovement()
    {
        if (!isMoving && moveQueue.Count > 0)
        {
            int nextIndex = moveQueue.Dequeue();
            moveDirection = nextIndex > currentIndex ? 1 : -1;
            currentIndex = nextIndex;
            isMoving = true;

            // Flip sprites visually (fixed inversion)
            foreach (var sprite in _sprites)
                sprite.flipX = moveDirection < 0;

            // Trigger walking animation
            if (_animator != null)
                _animator.SetBool("isWalking", true);
        }

        if (isMoving)
        {
            Vector3 targetPos = houses[currentIndex].position + positionOffset;
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime, moveSpeed);

            if (Vector3.Distance(transform.position, targetPos) < 0.05f)
            {
                transform.position = targetPos;
                isMoving = false;

                // Stop walking animation
                if (_animator != null)
                    _animator.SetBool("isWalking", false);

                // Always end facing right
                foreach (var sprite in _sprites)
                    sprite.flipX = false;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (houses == null || houses.Count < 1)
            return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < houses.Count - 1; i++)
            Gizmos.DrawLine(houses[i].position + positionOffset, houses[i + 1].position + positionOffset);
    }
}
