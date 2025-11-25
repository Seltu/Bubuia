using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    [SerializeField] private float _parallaxMultiplier;
    private Transform _cameraTransform;
    private Vector3 _lastCamPos;
    private float textureSizeX;

    private void Start()
    {
        _cameraTransform = Camera.main.transform;
        _lastCamPos = _cameraTransform.position;

        Sprite sprite = GetComponent<SpriteRenderer>().sprite;
        Texture2D texture = sprite.texture;
        textureSizeX = texture.width / sprite.pixelsPerUnit;
    }

    private void LateUpdate()
    {
        Vector3 deltaMove = _cameraTransform.position - _lastCamPos;

        transform.position += deltaMove * _parallaxMultiplier;
        _lastCamPos = _cameraTransform.position;
    }
}
