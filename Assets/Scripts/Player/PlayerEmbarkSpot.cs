using Unity.Cinemachine;
using UnityEngine;

public class PlayerEmbarkSpot : Interactable
{
    [SerializeField] private GameObject _playerCharacter;
    [SerializeField] private GameObject _playerBoat;
    [SerializeField] private Transform _boatLocation;
    [SerializeField] private Animator _animator;

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
        if (_playerCharacter.activeSelf)
        {
            Debug.Log("PlayerBarco");
            _playerCharacter.SetActive(false);
            _boatLocation.gameObject.SetActive(false);
            _playerBoat.transform.position = _boatLocation.position;
            _playerBoat.SetActive(true);
            EventManager.TriggerEvent("SwitchPlayerTransform", _playerBoat.transform);
        }
        else
        {
            Debug.Log("BarcoPlayer");
            _playerBoat.SetActive(false);
            _boatLocation.gameObject.SetActive(true);
            _playerCharacter.transform.position = transform.position;
            _playerCharacter.SetActive(true);
            EventManager.TriggerEvent("SwitchPlayerTransform", _playerCharacter.transform);
        }
    }
}
