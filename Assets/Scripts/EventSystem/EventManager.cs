using System;
using System.Collections.Generic;
using UnityEngine;

public static class EventManager
{
    private static Dictionary<string, Delegate> eventDictionary = new Dictionary<string, Delegate>();

    public static void AddListener(string eventName, Action listener)
    {
        eventName = string.Intern(eventName);
        if (!eventDictionary.ContainsKey(eventName))
        {
            eventDictionary[eventName] = listener;
        }
        else
        {
            eventDictionary[eventName] = Delegate.Combine(eventDictionary[eventName], listener);
        }
    }

    public static void AddListener<T>(string eventName, Action<T> listener)
    {
        eventName = string.Intern(eventName);
        if (!eventDictionary.ContainsKey(eventName))
        {
            eventDictionary[eventName] = listener;
        }
        else
        {
            eventDictionary[eventName] = Delegate.Combine(eventDictionary[eventName], listener);
        }
    }

    public static void AddListener<T1, T2>(string eventName, Action<T1, T2> listener)
    {
        eventName = string.Intern(eventName);
        if (!eventDictionary.ContainsKey(eventName))
        {
            eventDictionary[eventName] = listener;
        }
        else
        {
            eventDictionary[eventName] = Delegate.Combine(eventDictionary[eventName], listener);
        }
    }

    public static void AddListener<T1, T2, T3>(string eventName, Action<T1, T2, T3> listener)
    {
        eventName = string.Intern(eventName);
        if (!eventDictionary.ContainsKey(eventName))
        {
            eventDictionary[eventName] = listener;
        }
        else
        {
            eventDictionary[eventName] = Delegate.Combine(eventDictionary[eventName], listener);
        }
    }

    public static void RemoveListener(string eventName, Action listener)
    {
        eventName = string.Intern(eventName);
        if (eventDictionary.ContainsKey(eventName))
        {
            eventDictionary[eventName] = Delegate.Remove(eventDictionary[eventName], listener);
            if (eventDictionary[eventName] == null)
            {
                eventDictionary.Remove(eventName);
            }
        }
    }

    public static void RemoveListener<T>(string eventName, Action<T> listener)
    {
        eventName = string.Intern(eventName);
        if (eventDictionary.ContainsKey(eventName))
        {
            eventDictionary[eventName] = Delegate.Remove(eventDictionary[eventName], listener);
            if (eventDictionary[eventName] == null)
            {
                eventDictionary.Remove(eventName);
            }
        }
    }

    public static void RemoveListener<T1, T2>(string eventName, Action<T1, T2> listener)
    {
        eventName = string.Intern(eventName);
        if (eventDictionary.ContainsKey(eventName))
        {
            eventDictionary[eventName] = Delegate.Remove(eventDictionary[eventName], listener);
            if (eventDictionary[eventName] == null)
            {
                eventDictionary.Remove(eventName);
            }
        }
    }

    public static void RemoveListener<T1, T2, T3>(string eventName, Action<T1, T2, T3> listener)
    {
        eventName = string.Intern(eventName);
        if (eventDictionary.ContainsKey(eventName))
        {
            eventDictionary[eventName] = Delegate.Remove(eventDictionary[eventName], listener);
            if (eventDictionary[eventName] == null)
            {
                eventDictionary.Remove(eventName);
            }
        }
    }

    public static void TriggerEvent(string eventName)
    {
        eventName = string.Intern(eventName);
        if (eventDictionary.TryGetValue(eventName, out Delegate action) && action is Action typedAction)
        {
            typedAction.Invoke();
        }
    }

    public static void TriggerEvent<T>(string eventName, T eventData)
    {
        eventName = string.Intern(eventName);
        if (eventDictionary.TryGetValue(eventName, out Delegate action) && action is Action<T> typedAction)
        {
            typedAction.Invoke(eventData);
        }
    }

    public static void TriggerEvent<T1, T2>(string eventName, T1 param1, T2 param2)
    {
        eventName = string.Intern(eventName);
        if (eventDictionary.TryGetValue(eventName, out Delegate action) && action is Action<T1, T2> typedAction)
        {
            typedAction.Invoke(param1, param2);
        }
    }

    public static void TriggerEvent<T1, T2, T3>(string eventName, T1 param1, T2 param2, T3 param3)
    {
        eventName = string.Intern(eventName);
        if (eventDictionary.TryGetValue(eventName, out Delegate action) && action is Action<T1, T2, T3> typedAction)
        {
            typedAction.Invoke(param1, param2, param3);
        }
    }

    public static void ClearAllEvents()
    {
        eventDictionary.Clear();
    }
}
