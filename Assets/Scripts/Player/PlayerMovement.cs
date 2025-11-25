using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _playerRb;
    [SerializeField] private SpriteRenderer[] _sprites;
    [SerializeField] private Animator _animator;

    [SerializeField] private float _playerSpeed;

    private bool _canMove = true;
    private float moveX;

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

    private void Update()
    { 
        MovePlayer();
    }

    private void MovePlayer()
    {
        if (moveX != 0)
        {
            Vector2 velocity = new Vector2(moveX * _playerSpeed, _playerRb.linearVelocity.y);
            _playerRb.linearVelocity = velocity;

            foreach (var sprite in _sprites)
                sprite.flipX = moveX < 0;

            _animator.SetBool("isWalking", true);
        }
        else
        {
            _playerRb.linearVelocity = new Vector2(0f, _playerRb.linearVelocity.y);
            _animator.SetBool("isWalking", false);
        }

    }

    public void MovePlayerInput(InputAction.CallbackContext context)
    {
        moveX = context.ReadValue<Vector2>().x;
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
