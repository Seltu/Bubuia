using UnityEngine;
using Cinemachine;

public class CameraShaker : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    private CinemachineBasicMultiChannelPerlin perlin;
    private float shakeTimer;
    private bool shakeToggled;


    private void Start()
    {
        perlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        perlin.m_AmplitudeGain = 0f;
        EventManager.AddListener<float, float>("CameraShake", ShakeCamera);
        EventManager.AddListener<float>("ToggleCameraShake", ShakeCamera);
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener<float, float>("CameraShake", ShakeCamera);
        EventManager.RemoveListener<float>("ToggleCameraShake", ShakeCamera);
    }

    private void ShakeCamera(float intensity, float time)
    {
        if (perlin == null) return;

        perlin.m_AmplitudeGain = intensity;
        shakeTimer = time;
    }

    private void ShakeCamera(float intensity)
    {
        if (perlin == null) return;

        perlin.m_AmplitudeGain = intensity;
    }

    private void Update()
    {
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
            if (shakeTimer <= 0f)
            {
                perlin.m_AmplitudeGain = 0f;
            }
        }
    }
}
