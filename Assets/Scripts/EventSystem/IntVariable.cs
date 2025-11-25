using UnityEngine;
using System;

[CreateAssetMenu(fileName = "NewIntVariable", menuName = "ScriptableObjects/IntVariable", order = 1)]
public class IntVariable : ScriptableObject
{
    [SerializeField] private int value;
    public event Action<int> OnValueChanged;

    public int Value
    {
        get => value;
        set
        {
            if (this.value != value)
            {
                this.value = value;
                OnValueChanged?.Invoke(this.value);
            }
        }
    }

    public void SetValue(int newValue)
    {
        Value = newValue;
    }

    public void ApplyChange(int amount)
    {
        Value += amount;
    }
}
