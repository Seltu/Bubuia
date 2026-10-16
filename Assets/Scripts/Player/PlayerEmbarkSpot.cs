using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerEmbarkSpot : Interactable
{
    private PlayerMovement _playerCharacter;
    private PlayerShipController _playerShip;
    [SerializeField] private Transform _shipLocation;
    [SerializeField] private Animator _animator;
    private bool _awaitingBoat;
    private const float _jumpDuration = 0.5f;
    private const float _jumpHeight = 2f;

    protected override void Start()
    {
        base.Start();
        _playerCharacter = FindAnyObjectByType<PlayerMovement>(FindObjectsInactive.Include);
        _playerShip = FindAnyObjectByType<PlayerShipController>(FindObjectsInactive.Include);
        EventManager.AddListener("ShipParkingEnd", ShipParkingEnd);
    }

    protected override void OnDestroy()
    {
        EventManager.RemoveListener("ShipParkingEnd", ShipParkingEnd);
        base.OnDestroy();
    }

    protected override void OnSelect()
    {
        _animator.SetBool("Hover", true);
    }

    protected override void OnDeselect()
    {
        _animator.SetBool("Hover", false);
    }

    protected override void Interact()
    {
        if (_awaitingBoat) return;
        EventManager.TriggerEvent("ToggleControls", false);
        if (_playerCharacter.gameObject.activeSelf)
        {
            StartCoroutine(ShipPlayerEnter());
        }
        else
        {
            _awaitingBoat = true;
            EventManager.TriggerEvent("ParkShip", _shipLocation);
        }
    }

    private void ShipParkingEnd()
    {
        if (!_awaitingBoat) return; // Quando o evento for chamado, só o EmbarkSpot que está esperando um barco vai fazer algo
        StartCoroutine(ShipPlayerExit());
    }

    private IEnumerator ShipPlayerEnter()
    {
        _awaitingBoat = true;

        _playerCharacter.gameObject.SetActive(false);
        _playerShip.transform.position = _shipLocation.position;
        _playerShip.gameObject.SetActive(true);
        _shipLocation.gameObject.SetActive(false);

        EventManager.TriggerEvent("SwitchPlayerTransform", _playerShip.transform);

        Transform playerSprite = _playerShip.GetPlayerTransform();

        Vector3 originalPivot = playerSprite.localPosition;
        Vector3 targetPosition = playerSprite.position;
        Vector3 startPosition = _playerCharacter.transform.position + Vector3.up;

        playerSprite.position = startPosition;

        float elapsed = 0f;

        while (elapsed < _jumpDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / _jumpDuration);

            Vector3 position = Vector3.Lerp(startPosition, targetPosition, t);

            position.y += 4f * _jumpHeight * t * (1f - t);

            playerSprite.position = position;

            yield return null;
        }

        playerSprite.localPosition = originalPivot;

        _awaitingBoat = false;
        EventManager.TriggerEvent("ToggleControls", true);
    }

    private IEnumerator ShipPlayerExit()
    {
        Transform playerSprite = _playerShip.GetPlayerTransform();

        Vector3 originalPivot = playerSprite.localPosition;
        Vector3 startPosition = playerSprite.position;
        Vector3 targetPosition = transform.position + Vector3.up;

        float elapsed = 0f;

        while (elapsed < _jumpDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / _jumpDuration);

            Vector3 position = Vector3.Lerp(startPosition, targetPosition, t);

            position.y += 4f * _jumpHeight * t * (1f - t);

            playerSprite.position = position;

            yield return null;
        }

        playerSprite.position = targetPosition;

        _awaitingBoat = false;
        _playerShip.gameObject.SetActive(false);
        _shipLocation.rotation = _playerShip.transform.GetChild(0).rotation;
        _shipLocation.gameObject.SetActive(true);
        _playerCharacter.gameObject.SetActive(true);
        _playerCharacter.Teleport(transform.position);

        playerSprite.localPosition = originalPivot;

        EventManager.TriggerEvent( "SwitchPlayerTransform", _playerCharacter.transform);
        EventManager.TriggerEvent("ToggleControls", true);
    }
}
