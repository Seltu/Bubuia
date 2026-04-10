using UnityEngine;
using Cinemachine;
using System;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private CinemachineTargetGroup targetGroup;
    [SerializeField] private float zoomedInRadius;
    [SerializeField] private float zoomedOutRadius;

    private CinemachineBasicMultiChannelPerlin _perlin;
    private float _cameraShakeTimer;
    private bool _cameraShakeToggled;
    private float _targetZoomRadius;


    private void Start()
    {
        _targetZoomRadius = zoomedInRadius;
        _perlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        _perlin.m_AmplitudeGain = 0f;
        EventManager.AddListener<float, float>("CameraShake", ShakeCamera);
        EventManager.AddListener<float>("ToggleCameraShake", ShakeCamera);
        EventManager.AddListener("ZoomOut", ZoomOut);
        EventManager.AddListener("ZoomIn", ZoomIn);
        EventManager.AddListener<bool>("FocusOnHook", FocusOnHook);
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
        targetGroup.m_Targets[0].radius = Mathf.Lerp(targetGroup.m_Targets[0].radius, _targetZoomRadius, Time.deltaTime);

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
