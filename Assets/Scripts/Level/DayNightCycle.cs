using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("Time")]
    [Tooltip("Real minutes for a full 24h cycle.")]
    [SerializeField] private float dayLengthMinutes = 24f;

    [Tooltip("Start time in hours (0-24). 0 = midnight, 12 = noon.")]
    [Range(0f, 24f)]
    [SerializeField] private float startTimeHours = 8f;

    [Header("Sun")]
    [Tooltip("Directional Light used as the sun.")]
    [SerializeField] private Light sunLight;

    [Tooltip("Sun intensity at noon.")]
    [SerializeField] private float sunIntensityDay = 1.2f;

    [Tooltip("Sun intensity at midnight.")]
    [SerializeField] private float sunIntensityNight = 0f;

    [SerializeField] private bool controlSkyboxExposure = true;
    [SerializeField] private float skyboxExposureDay = 1.0f;
    [SerializeField] private float skyboxExposureNight = 0.3f;

    // Current time (0..24)
    public float TimeOfDayHours { get; private set; }

    private float DayLengthSeconds => Mathf.Max(1f, dayLengthMinutes * 60f);

    private void Awake()
    {
        TimeOfDayHours = Mathf.Repeat(startTimeHours, 24f);
    }

    private void Update()
    {
        AdvanceTime();
        ApplySun();
        ApplyEnvironment();
    }

    private void AdvanceTime()
    {
        // 24 in-game hours over dayLengthMinutes real minutes
        float hoursPerSecond = 24f / DayLengthSeconds;
        TimeOfDayHours = Mathf.Repeat(TimeOfDayHours + hoursPerSecond * Time.deltaTime, 24f);
    }

    private void ApplySun()
    {
        if (!sunLight) return;

        // Convert time to a 0..1 day fraction
        float t = TimeOfDayHours / 24f;

        // Rotate so: 0h = midnight, 6h = sunrise, 12h = noon, 18h = sunset
        // -90 makes sunrise at 6h roughly on the horizon depending on your scene orientation.
        float sunAngle = (t * 360f) - 90f;
        sunLight.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);

        // Day factor: 0 at night, 1 at noon (based on sun height)
        float sunDot = Vector3.Dot(sunLight.transform.forward, Vector3.down);
        float dayFactor = Mathf.Clamp01(sunDot);

        sunLight.intensity = Mathf.Lerp(sunIntensityNight, sunIntensityDay, dayFactor);
        sunLight.enabled = sunLight.intensity > 0.001f;
    }

    private void ApplyEnvironment()
    {
        // Use the same dayFactor logic but recompute safely if no sun
        float dayFactor = 0f;
        if (sunLight)
        {
            float sunDot = Vector3.Dot(sunLight.transform.forward, Vector3.down);
            dayFactor = Mathf.Clamp01(sunDot);
        }

        if (controlSkyboxExposure && RenderSettings.skybox != null)
        {
            // Standard skybox uses "_Exposure" property
            if (RenderSettings.skybox.HasProperty("_Exposure"))
            {
                float exposure = Mathf.Lerp(skyboxExposureNight, skyboxExposureDay, dayFactor);
                RenderSettings.skybox.SetFloat("_Exposure", exposure);
            }
        }

        // Update reflections/ambient gradually (optional but helps)
        DynamicGI.UpdateEnvironment();
    }
}
