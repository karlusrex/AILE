using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using System;

public class Microph : MonoBehaviour {

    private string microphoneDevice;
    private bool silent = true; // Audio Level
    private bool switcher = true; // Init variable for switching from active listening to recording
    private AudioSource sharedAudioSource; 
    private int _sampleWindow = 128; 
    private int timer = 0; 
    private float threshold = 0.01f; // Audio Level Threshold 0.001
    private Client client;
    private AudioPlayback audioPlayback; // For text to speech playback
    public UnityEvent OnSpeechDetected = new UnityEvent();
    public UnityEvent<string> SpeechToText = new UnityEvent<string>(); 


    /*
    Start microphone recording to listen to user
    Start CountSeconds coroutine to get ticks
    */
    void Start() {
        microphoneDevice = Microphone.devices[0];
        Debug.Log("[MIC] Microphone Device found: " + microphoneDevice + " : hard mode");

        sharedAudioSource = GetComponent<AudioSource>();
        sharedAudioSource.clip = Microphone.Start(microphoneDevice, true, 600, 44100);
        sharedAudioSource.loop = true;
        while (!(Microphone.GetPosition(microphoneDevice) > 0)) { }

        StartCoroutine(CountSeconds());
        StartCoroutine(AudioLevelChecker());
        StartCoroutine(EndCurrentRecording());

        client = FindObjectOfType<Client>();

        audioPlayback = FindObjectOfType<AudioPlayback>();
    }

    /*
    Ends the current recording and saves the audioclip 
    Starts new recording
    */
    IEnumerator EndCurrentRecording() 
    {
        
        while (true) {
            if (timer > 5 && !switcher) {
                Debug.Log("[MIC] Saving Recording and restarting Microphone");
                // End recording
                timer = 0;
                switcher = true;
                Microphone.End(microphoneDevice);
                sharedAudioSource.clip = SavWav.TrimSilence(sharedAudioSource.clip, 0.005f);

                SaveRecording();
                // Restart recording
                sharedAudioSource = GetComponent<AudioSource>();
                sharedAudioSource.clip = Microphone.Start(microphoneDevice, true, 600, 44100);
                sharedAudioSource.loop = true;
                while (!(Microphone.GetPosition(microphoneDevice) > 0)) { }     
            }
            yield return new WaitForSeconds(1);
        }
    }

    /*
    Tick function - 1 tick per second
    */
    IEnumerator CountSeconds()
    {
        while (true)
        {
            if (silent && !switcher) { // If silent and not recording
                Debug.Log("[MIC] Started Counter");
                yield return new WaitForSeconds(1);
                timer++;
            }
            yield return null; // Add this line
           
        }
    }

    /*
    Checks current audio level and sets `silent` to true or false
    Resets timer if there is audio
    Sets first time detecting audio over threshold
    */
    IEnumerator AudioLevelChecker() {
        while (true) {
            // Detected audio over threshold
            if (MicrophoneAudioLevel() > threshold) {
                Debug.Log("[MIC] Detected Audio");
                silent = false;
                switcher = false;
                timer = 0;
                OnSpeechDetected.Invoke(); // TimerStop in EventManager
            } else {
                //Debug.Log("Silent");
                silent = true;
            }
            yield return null; // Add this line
            
        } 
    }

    /*
    Gets the current audio level as a float 
    */
    float MicrophoneAudioLevel() // TODO: GIVE CREDIT TO ORIGINAL AUTHOR
    {
        float levelMax = 0;
        float[] waveData = new float[_sampleWindow];
        int micPosition = Microphone.GetPosition(null) - (_sampleWindow + 1);
        if (micPosition < 0) return 0;
        sharedAudioSource.clip.GetData(waveData, micPosition);
        for (int i = 0; i < _sampleWindow; i++)
        {
            float wavePeak = waveData[i] * waveData[i];
            if (levelMax < wavePeak)
            {
                levelMax = wavePeak;
            }
        }
        return levelMax;
    }

    /*
    Saves the current audioclip to the local machine
    Do a POST request to the server to get the speech to text
    */
    void SaveRecording()
    {
        Debug.Log("[MIC] Saved Recording and sending to STT: " + DateTime.Now.ToString("HH:mm:ss:fff"));
        SavWav.Save("recording", sharedAudioSource.clip);
        client.GetRequest("http://127.0.0.1:5000/api/v1/speech-to-text", OnSpeechToTextRecieved);
    }

    /*
    Callback function for the POST request to the server
    */
    void OnSpeechToTextRecieved(string response)
    {
        if (response != null)
        {
            Debug.Log(" [MIC] Recieved input" + response);
            SpeechToText.Invoke(response);
        } else
        {
            Debug.LogError("[MIC] Failed to get speech to text.");
        }
    }
}