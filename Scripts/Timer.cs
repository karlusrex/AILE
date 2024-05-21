using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

/* Responsible for listening to actions made by user, and manage the timer */
public class Timer : MonoBehaviour
{
    private static Coroutine timerCoroutine;
    public UnityEvent Idle = new UnityEvent();
    private EventManager eventManager;

    private void OnEnable()
    {
        eventManager = FindObjectOfType<EventManager>(); 

        // Subscribe to the events
        eventManager.TimerStart.AddListener(StartTimer);
        eventManager.TimerStop.AddListener(StopTimer);

    }

    private void OnDisable()
    {
        // Unsubscribe from the event
        eventManager.TimerStart.RemoveListener(StartTimer);
        eventManager.TimerStop.RemoveListener(StopTimer);
    }

    public void StartTimer()
    {
        StopTimer();
        timerCoroutine = StartCoroutine(TimerCoroutine());
    }

    public void StopTimer()
    {

        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
    }

    IEnumerator TimerCoroutine() //used to be static? 
    {
        yield return new WaitForSeconds(30f); //30 sec
        Debug.Log("[Timer] No action taken in 30 seconds. Player needs support!");
        Debug.Log("[Timer] Timer triggered at: " + DateTime.Now.ToString("HH:mm:ss:fff"));
        Idle.Invoke();
    }
}
