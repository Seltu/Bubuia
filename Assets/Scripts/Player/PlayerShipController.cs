using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerShipController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform shipSprite;
    [SerializeField] private Transform playerSprite;
    [SerializeField] private Rigidbody shipRigidbody;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 6f;
    [SerializeField] private float turnSpeed = 200f;
    [SerializeField] private float drag = 2f;

    [Header("Effects")]
    [SerializeField] private float splashStrength = 0.2f;
    [SerializeField] private float splashInterval = 0.1f;

    private float _splashTimer;
    private Vector3 moveInput;
    private Vector3 currentVelocity;
    private bool _stopped;

    private void Start()
    {
        EventManager.AddListener("TurnOffControls", PauseMovement);
        EventManager.AddListener<Fish>("StartFishingMinigame", PauseMovement);
        EventManager.AddListener<bool>("EndFishingMinigame", UnpauseMovement);
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener("TurnOffControls", PauseMovement);
        EventManager.RemoveListener<Fish>("StartFishingMinigame", PauseMovement);
        EventManager.RemoveListener<bool>("EndFishingMinigame", UnpauseMovement);
    }

    private void PauseMovement() => _stopped = true;
    private void PauseMovement(Fish f) => _stopped = true;
    private void UnpauseMovement(bool v) => _stopped = false;

    private void Update()
    {
        MoveShip3D();
    }

    private void MoveShip3D()
    {
        if (Time.timeScale == 0) return;

        // --- Convert input (X,Y) from InputSystem to (X,0,Z)
        Vector3 desiredInput = new Vector3(moveInput.x, 0, moveInput.z).normalized;

        // --- Smooth rotation only if moving
        if (desiredInput.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(desiredInput, Vector3.up);
            shipSprite.rotation = Quaternion.RotateTowards(
                shipSprite.rotation,
                targetRot,
                turnSpeed * Time.deltaTime
            );
        }

        // --- Apply acceleration toward target velocity
        Vector3 targetVelocity = desiredInput * moveSpeed;

        currentVelocity = Vector3.MoveTowards(
            currentVelocity,
            targetVelocity,
            acceleration * Time.deltaTime
        );

        // --- Apply velocity to Rigidbody
        shipRigidbody.linearVelocity = currentVelocity;

        // --- Auto-drag when not pressing movement
        if (desiredInput.sqrMagnitude < 0.01f)
        {
            currentVelocity = Vector3.MoveTowards(
                currentVelocity,
                Vector3.zero,
                drag * Time.deltaTime
            );
        }

        // --- Splash effect
        if (currentVelocity.sqrMagnitude > 0.25f)
        {
            _splashTimer -= Time.deltaTime;
            if (_splashTimer < 0)
            {
                _splashTimer = splashInterval;
                EventManager.TriggerEvent("Splash", transform.position, splashStrength);
            }
        }
    }

    // INPUT
    public void MoveShipInput(InputAction.CallbackContext context)
    {
        if (InputLock.movementLocked)
        {
            moveInput = Vector3.zero;
            return;
        }

        if (!_stopped)
        {
            Vector2 input = context.ReadValue<Vector2>();
            moveInput = new Vector3(input.x, 0, input.y); // Y vira Z
        }
        else
        {
            moveInput = Vector3.zero;
        }
    }
}
