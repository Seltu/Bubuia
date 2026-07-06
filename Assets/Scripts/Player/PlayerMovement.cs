using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.U2D;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody _playerRb;
    [SerializeField] private GameObject _playerSpritesHolder;
    [SerializeField] private Animator _animator;

    [SerializeField] private float _playerSpeed;
    [SerializeField] private float _fallSpeed;

    private Vector2 moveInput;

    private void Start()
    {
        EventManager.AddListener<string>("OnOpenStore", BlockPlayerMovement);
        EventManager.AddListener("OnCloseStore", AllowPlayerMovement);
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener<string>("OnOpenStore", BlockPlayerMovement);
        EventManager.RemoveListener("OnCloseStore", AllowPlayerMovement);
    }

    private void FixedUpdate()
    { 
        MovePlayer();
    }

    private void MovePlayer()
    {
        if (moveInput.magnitude != 0 && !InputLock.movementLocked)
        {
            Vector3 velocity = new Vector3(moveInput.x * _playerSpeed, _playerRb.linearVelocity.y-_fallSpeed*Time.deltaTime, moveInput.y * _playerSpeed);
            _playerRb.linearVelocity = velocity;

            if (moveInput.x < 0)
                _playerSpritesHolder.transform.rotation = Quaternion.Euler(25, 0, 0);
            else
                _playerSpritesHolder.transform.rotation = Quaternion.Euler(-25, 180, 0);

            _animator.SetBool("isWalking", true);
        }
        else
        {
            _playerRb.linearVelocity = new Vector2(0f, _playerRb.linearVelocity.y - _fallSpeed * Time.deltaTime);
            _animator.SetBool("isWalking", false);
        }

    }

    public void MovePlayerInput(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void BlockPlayerMovement(string _storeId)
    {
        _playerRb.linearVelocity = new Vector2(0f, _playerRb.linearVelocity.y);
        EventManager.TriggerEvent("OnBlockPlayerMovement");
    }

    private void AllowPlayerMovement()
    {
        EventManager.TriggerEvent("OnAllowPlayerMovement");
    }
}
