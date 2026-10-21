using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [Header("Day Duration")]
    [SerializeField, Min(1f)] private float secondsPerDay = 1200f;

    [Header("Transition Config")]
    [SerializeField] private int fromNightToSunrise = 5;
    [SerializeField] private int fromSunriseToDay = 6;
    [SerializeField] private int fromDayToSunset = 18;
    [SerializeField] private int fromSunsetToNight = 19;

    [SerializeField, Range(0.01f, 0.25f)]
    private float transitionDayFraction = 0.02f;

    [Header("Skyboxes")]
    [SerializeField] private Texture2D skyboxNight;
    [SerializeField] private Texture2D skyboxSunrise;
    [SerializeField] private Texture2D skyboxDay;
    [SerializeField] private Texture2D skyboxSunset;

    [Header("Cubemaps")]
    [SerializeField] private Cubemap cubemapNight;
    [SerializeField] private Cubemap cubemapSunrise;
    [SerializeField] private Cubemap cubemapDay;
    [SerializeField] private Cubemap cubemapSunset;
    
    [Header("Lighting Gradients")]
    [SerializeField] private Gradient graddientNightToSunrise;
    [SerializeField] private Gradient graddientSunriseToDay;
    [SerializeField] private Gradient graddientDayToSunset;
    [SerializeField] private Gradient graddientSunsetToNight;

    [Header("Lighting Gradients")]
    [SerializeField] private Light globalLight;

    [Header("Water")]
    [SerializeField] private Material waterMaterial;

    private float TransitionDuration =>
        secondsPerDay * transitionDayFraction;

    private int minutes;

    public int Minutes
    { get { return minutes; } set { minutes = value; OnMinutesChange(value); } }

    private int hours = 0;

    public int Hours
    { get { return hours; } set { hours = value; OnHoursChange(value); } }

    private int days;

    public int Days
    { get { return days; } set { days = value; } }

    private float tempSecond;

    public void Start()
    {
        hours = 0;

        RenderSettings.skybox.SetTexture("_Texture1", skyboxNight);
        RenderSettings.skybox.SetTexture("_Texture2", skyboxSunrise);
        RenderSettings.skybox.SetFloat("_Blend", 0);
    }

    public void Update()
    {
        tempSecond += Time.deltaTime;

        float secondsPerGameMinute = secondsPerDay / 1440f;

        while (tempSecond >= secondsPerGameMinute)
        {
            tempSecond -= secondsPerGameMinute;
            Minutes++;
        }

        // Rotate the light smoothly through 360 degrees per in-game day.
        float degreesPerSecond = 360f / secondsPerDay;

        globalLight.transform.Rotate(
            Vector3.up,
            degreesPerSecond * Time.deltaTime,
            Space.World
        );
    }

    private void OnMinutesChange(int value)
    {
        //globalLight.transform.Rotate(Vector3.up, (1f / (1440f / 4f)) * 360f, Space.World);
        if (value >= 60)
        {
            Hours++;
            minutes = 0;
        }
        if (Hours >= 24)
        {
            Hours = 0;
            Days++;
        }

        EventManager.TriggerEvent("TimePassed", Hours, Minutes);
    }

    private void OnHoursChange(int value)
    {
        if (value == fromNightToSunrise)
        {
            StartCoroutine(LerpSkybox(skyboxNight, skyboxSunrise, cubemapNight, cubemapSunrise, TransitionDuration));
            StartCoroutine(LerpLight(graddientNightToSunrise, TransitionDuration));
        }
        else if (value == fromSunriseToDay)
        {
            StartCoroutine(LerpSkybox(skyboxSunrise, skyboxDay, cubemapSunrise, cubemapDay, TransitionDuration));
            StartCoroutine(LerpLight(graddientSunriseToDay, TransitionDuration));
        }
        else if (value == fromDayToSunset)
        {
            StartCoroutine(LerpSkybox(skyboxDay, skyboxSunset, cubemapDay, cubemapSunset, TransitionDuration));
            StartCoroutine(LerpLight(graddientDayToSunset, TransitionDuration));
        }
        else if (value == fromSunsetToNight)
        {
            StartCoroutine(LerpSkybox(skyboxSunset, skyboxNight, cubemapSunset, cubemapNight, TransitionDuration));
            StartCoroutine(LerpLight(graddientSunsetToNight, TransitionDuration));
        }
    }

    private IEnumerator LerpSkybox(Texture2D a, Texture2D b, Cubemap cubeA, Cubemap cubeB, float time)
    {
        RenderSettings.skybox.SetTexture("_Texture1", a);
        RenderSettings.skybox.SetTexture("_Texture2", b);
        RenderSettings.skybox.SetFloat("_Blend", 0f);

        waterMaterial.SetTexture("_ReflectionA", cubeA);
        waterMaterial.SetTexture("_ReflectionB", cubeB);
        waterMaterial.SetFloat("_ReflectionBlend", 0f);

        for (float i = 0; i < time; i += Time.deltaTime)
        {
            float blend = i / time;
            RenderSettings.skybox.SetFloat("_Blend", blend);
            waterMaterial.SetFloat("_ReflectionBlend", blend);
            yield return null;
        }
        
        RenderSettings.skybox.SetTexture("_Texture1", b);
    }

    private IEnumerator LerpLight(Gradient lightGradient, float time)
    {
        for (float i = 0; i < time; i += Time.deltaTime)
        {
            globalLight.color = lightGradient.Evaluate(i / time);
            RenderSettings.fogColor = globalLight.color;
            yield return null;
        }
    }
}

