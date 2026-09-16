using UnityEngine;

using System;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private CinemachineTargetGroup targetGroup;
    [SerializeField] private InputActionReference zoomAction;
    [SerializeField] private float maxZoom = 150f;
    [SerializeField] private float startZoom = 50f;
    [SerializeField] private float zoomedInRadius;
    [SerializeField] private float zoomedOutRadius;
    [SerializeField] private float _dialogueFocusRadius = 80f;

    private CinemachineBasicMultiChannelPerlin _perlin;
    private float _cameraShakeTimer;
    private bool _cameraShakeToggled;
    private float _currentZoom;
    private float _targetZoomRadius;
    private bool _focusingOnTarget;
    private bool _zoomInputLockedByCutscene;
    private float _zoomDirectionBuffer;
    private int _dialogueTargetIndex;

    private void Start()
    {
        _targetZoomRadius = zoomedInRadius;
        _currentZoom = startZoom;
        _perlin = virtualCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
        _perlin.AmplitudeGain = 0f;
        EventManager.AddListener<float, float>("CameraShake", ShakeCamera);
        EventManager.AddListener<float>("ToggleCameraShake", ShakeCamera);
        EventManager.AddListener("ZoomOut", ZoomOut);
        EventManager.AddListener("ZoomIn", ZoomIn);
        EventManager.AddListener<bool>("FocusOnHook", FocusOnHook);
        EventManager.AddListener<bool, Transform>("CameraFocusOnTarget", FocusOnTarget);
        EventManager.AddListener("CutsceneStarted", OnCutsceneStarted);
        EventManager.AddListener("CutsceneEnded", OnCutsceneEnded);
    }

    private void FocusOnHook(bool focus)
    {
        if (focus)
        {
            targetGroup.Targets[1].Weight = 2;
        }
        else
        {
            targetGroup.Targets[1].Weight = 1;
        }
    }

    private void FocusOnTarget(bool focus, Transform target)
    {
        if (focus)
        {
            _focusingOnTarget = true;
            targetGroup.AddMember(target, 1, _targetZoomRadius);
            _dialogueTargetIndex = targetGroup.Targets.Count - 1;
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
        EventManager.RemoveListener("CutsceneStarted", OnCutsceneStarted);
        EventManager.RemoveListener("CutsceneEnded", OnCutsceneEnded);
    }

    private void OnCutsceneStarted()
    {
        _zoomInputLockedByCutscene = true;
    }

    private void OnCutsceneEnded()
    {
        _zoomInputLockedByCutscene = false;
    }

    private void ShakeCamera(float intensity, float time)
    {
        if (_perlin == null) return;

        _perlin.AmplitudeGain = intensity;
        _cameraShakeTimer = time;
    }

    private void ShakeCamera(float intensity)
    {
        if (_perlin == null) return;

        _perlin.AmplitudeGain = intensity;
    }

    private void Update()
    {
        if (zoomAction.action.ReadValue<float>() > 0)
            _zoomDirectionBuffer = 0.1f;
        else if(zoomAction.action.ReadValue<float>() < 0)
            _zoomDirectionBuffer = -0.1f;
        _zoomDirectionBuffer = Mathf.MoveTowards(_zoomDirectionBuffer, 0, Time.deltaTime);

        if (!_zoomInputLockedByCutscene)
        {
            _currentZoom = Math.Clamp(_currentZoom + Math.Sign(_zoomDirectionBuffer) * Time.deltaTime * 50f, 0f, maxZoom);
        }

        if (_focusingOnTarget)
        {
            targetGroup.Targets[0].Radius = _dialogueFocusRadius;
            if(targetGroup.Targets[1].Weight < 4)
                targetGroup.Targets[1].Weight = Mathf.MoveTowards(targetGroup.Targets[_dialogueTargetIndex].Weight, 4, Time.deltaTime);
        }
        else
            targetGroup.Targets[0].Radius = _targetZoomRadius + _currentZoom;

        if (_cameraShakeTimer > 0)
        {
            _cameraShakeTimer -= Time.deltaTime;
            if (_cameraShakeTimer <= 0f)
            {
                _perlin.AmplitudeGain = 0f;
            }
        }
    }
}
