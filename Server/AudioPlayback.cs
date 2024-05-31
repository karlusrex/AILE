using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using Mono.Reflection;
using System.Text.RegularExpressions;
using Unity.VisualScripting;


public class TaskAudioClip {
    public int currentTaskStep;
    public AudioClip audioClip;
    public string content; 

    public TaskAudioClip(int currentTaskStep, AudioClip audioClip, string content)
    {
        this.currentTaskStep = currentTaskStep;
        this.audioClip = audioClip;
        this.content = content; 
    }
}

/*
 * Responsible for generating and playing audio clips. 
 */
public class AudioPlayback : MonoBehaviour
{
    private Client client;
    private AudioSource audioSource;
    public UnityEvent AudioPlaybackFinished = new UnityEvent();

    private FeedbackManager feedbackManager; 
    private EventManager eventManager; 
    private RunManager runManager; 

    private string content; 

    private string uriTTS = "http://127.0.0.1:5000/api/v1/text-to-speech"; 

    private Queue<TaskAudioClip> audioClips = new Queue<TaskAudioClip>(); 

    void Start()
    {
        GameObject mainCamera = Camera.main.gameObject;
        client = mainCamera.GetComponent<Client>();
        audioSource = GetComponent<AudioSource>(); 
        feedbackManager = FindObjectOfType<FeedbackManager>();
        eventManager = FindObjectOfType<EventManager>(); 
        runManager = FindObjectOfType<RunManager>(); 
        client.AllTaskInstructionsFetched += OnInstructionsRecieved; 
        feedbackManager.CorrectionInstructionRecieved += OnInstructionsRecieved;  
        eventManager.EventPlayInstructions.AddListener(PlayInstructions); 
        StartCoroutine(PlayAudioClip());
    }


    private void PlayInstructions(string response){
        OnInstructionsRecieved(response); 
    }

    /** 
    * Adds audioclip to queue of audioclips to be played.
    */
    void AddAudioClip(TaskAudioClip clip){
        audioClips.Enqueue(clip);
    }

    /* 
    * Plays audioclip from queue.
    */
    IEnumerator PlayAudioClip() {
        while (true) {
            if (audioClips.Count > 0) {
                TaskAudioClip audioClip = audioClips.Dequeue();

                if (audioClip.currentTaskStep < client.getCurrentTaskStep() && !runManager.introductionMode)
                {
                    continue;
                }
                // print the time when audio is played
                print("[AUDIO] playing audioclip: " + DateTime.Now.ToString("HH:mm:ss:fff"));
                audioSource.clip = audioClip.audioClip;
                audioSource.PlayOneShot(audioClip.audioClip);
                StartCoroutine(WaitForAudioClipToEnd(audioClip.audioClip.length)); 
                yield return new WaitForSeconds(audioClip.audioClip.length);
            }
            yield return new WaitForSeconds(3.0f);
        }
    }

    /*
    * Waits until current audioclip is done playing, and then 
    * informs listeners. 
    * - Timer is a listener to the event
    */
    IEnumerator WaitForAudioClipToEnd(float length)
    {
        yield return new WaitForSeconds(length);
        AudioPlaybackFinished.Invoke(); //invoke listeners
    }

    private bool unsolvable_data_race = true;
    /* 
     * Callback method that recieves an audioclip as response and checks if its null.
     * If not {PlayAduio} is called.
     */
    void OnTextToSpeechRecieved(AudioClip audioClip)
    {
        if (audioClip != null)
        {   
            int currentTaskStep = client.getCurrentTaskStep();
            TaskAudioClip taskAudioClip = new TaskAudioClip(currentTaskStep, audioClip, content);
            AddAudioClip(taskAudioClip);
        } else
        {
            Debug.LogError("[AUDIO] Audioclip NOT OK");
        }
        if (unsolvable_data_race && !runManager.introductionMode) {
            if (runManager.enableContinuousInstructions){
                OnInstructionsRecieved(client.GetInstruction()); //play first instruction
            } else {
                client.PostRequest("http://127.0.0.1:5000/api/v1/get-overall-task-goal", OnInstructionsRecieved, null, null,false); //play overall task instructions 
            }
            unsolvable_data_race = false;
        }
    }

    /*
     * Callback method that recieves instructions from chatbot. 
     * Checks if response is null, otherwise calls client to 
     * request text-to-speech on the response.
     */
    public void OnInstructionsRecieved(string response)
    {
        if (response != null)
        {
            content = response; 
            client.PostRequest(uriTTS, null, OnTextToSpeechRecieved, response, true);
        } else
        {
            Debug.Log("[AUDIO]Chatbot response NOT OK");
        }
    }
}
