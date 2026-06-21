using UnityEngine;
using Cinemachine;
using System;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private CinemachineTargetGroup targetGroup;
    [SerializeField] private InputActionReference zoomAction;
    [SerializeField] private float maxZoom = 150f;
    [SerializeField] private float startZoom = 50f;
    [SerializeField] private float zoomedInRadius;
    [SerializeField] private float zoomedOutRadius;

    private CinemachineBasicMultiChannelPerlin _perlin;
    private float _cameraShakeTimer;
    private bool _cameraShakeToggled;
    private float _currentZoom;
    private float _targetZoomRadius;
    private bool _focusingOnTarget;

    private void Start()
    {
        _targetZoomRadius = zoomedInRadius;
        _currentZoom = startZoom;
        _perlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        _perlin.m_AmplitudeGain = 0f;
        EventManager.AddListener<float, float>("CameraShake", ShakeCamera);
        EventManager.AddListener<float>("ToggleCameraShake", ShakeCamera);
        EventManager.AddListener("ZoomOut", ZoomOut);
        EventManager.AddListener("ZoomIn", ZoomIn);
        EventManager.AddListener<bool>("FocusOnHook", FocusOnHook);
        EventManager.AddListener<bool, Transform>("CameraFocusOnTarget", FocusOnTarget);
    }

    private void FocusOnHook(bool focus)
    {
        if (focus)
        {
            targetGroup.m_Targets[1].weight = 4;
        }
        else
        {
            targetGroup.m_Targets[1].weight = 1;
        }
    }

    private void FocusOnTarget(bool focus, Transform target)
    {
        if (focus)
        {
            _focusingOnTarget = true;
            targetGroup.AddMember(target, 4, 25);
        }
        else
        {
            _focusingOnTarget = false;
            targetGroup.RemoveMember(target);
        }
    }

    private void ZoomOut()
    {
        _targetZoomRadius = zoomedOutRadius;
    }

    private void ZoomIn()
    {
        _targetZoomRadius = zoomedInRadius;
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener<float, float>("CameraShake", ShakeCamera);
        EventManager.RemoveListener<float>("ToggleCameraShake", ShakeCamera);
        EventManager.RemoveListener<bool>("FocusOnHook", FocusOnHook);
        EventManager.RemoveListener<bool, Transform>("CameraFocusOnTarget", FocusOnTarget);
    }

    private void ShakeCamera(float intensity, float time)
    {
        if (_perlin == null) return;

        _perlin.m_AmplitudeGain = intensity;
        _cameraShakeTimer = time;
    }

    private void ShakeCamera(float intensity)
    {
        if (_perlin == null) return;

        _perlin.m_AmplitudeGain = intensity;
    }

    private void Update()
    {
        _currentZoom = Math.Clamp(_currentZoom + zoomAction.action.ReadValue<float>() * Time.deltaTime * 10000f, 0f, maxZoom);

        if(_focusingOnTarget)
            targetGroup.m_Targets[0].radius = Mathf.Lerp(targetGroup.m_Targets[0].radius, _targetZoomRadius, Time.deltaTime);
        else
            targetGroup.m_Targets[0].radius = Mathf.Lerp(targetGroup.m_Targets[0].radius, _targetZoomRadius + _currentZoom, Time.deltaTime);

        if (_cameraShakeTimer > 0)
        {
            _cameraShakeTimer -= Time.deltaTime;
            if (_cameraShakeTimer <= 0f)
            {
                _perlin.m_AmplitudeGain = 0f;
            }
        }
    }
}
