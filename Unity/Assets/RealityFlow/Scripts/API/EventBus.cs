using System;
using UnityEngine;

public static class EventBus<T>
{
    static Action<T> subscribers;

    public static void Send(T ev)
    {
        Debug.Log($"Event {ev} invoked");
        subscribers?.Invoke(ev);
    }

    public static void Subscribe(Action<T> subscription) => subscribers += subscription;

    public static void Unsubscribe(Action<T> unsubscription) => subscribers -= unsubscription;
}