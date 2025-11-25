using UnityEngine;
using System;

[CreateAssetMenu(fileName = "NewFloatVariable", menuName = "ScriptableObjects/FloatVariable", order = 1)]
public class FloatVariable : ScriptableObject
{
    [SerializeField] private float value;
    public event Action<float> OnValueChanged;

    public float Value
    {
        get => value;
        set
        {
            if (!Mathf.Approximately(this.value, value))
            {
                this.value = value;
                OnValueChanged?.Invoke(this.value);
            }
        }
    }

    public void SetValue(float newValue)
    {
        Value = newValue;
    }

    public void ApplyChange(float amount)
    {
        Value += amount;
    }
}