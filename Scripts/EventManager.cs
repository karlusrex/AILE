using System; 
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class EventManager : MonoBehaviour {
    private Timer timer; 
    private Microph mic; 
    private Client client; 
    private AudioPlayback audioPlayback; 
    private RunManager runManager;
    private ImageController imageController;
    private PlayerPickUpDrop playerPickUpDrop; 
    
    // <<<< OUTGOING EVENTS >>>>
    public UnityEvent EventIdle = new UnityEvent(); // For Feedback manager
    public UnityEvent<string> EventSpeech = new UnityEvent<string>(); // For Feedback manager
    public UnityEvent EventInteraction = new UnityEvent(); // On false evaluation
    public UnityEvent EventReset = new UnityEvent(); // On true evaluation

    public UnityEvent<string> EventPlayInstructions = new UnityEvent<string>(); //For AudioPlayback 

    // <<<< INCOMING EVENTS >>>>
    public UnityEvent TimerStart = new UnityEvent(); // For Timer
    public UnityEvent TimerStop = new UnityEvent(); // For Timer

    private List<string> validation = new List<string>(){
        "Good job!", 
        "Well done!", 
        "Perfect!", 
        "Great!", 
        "You are doing great!",
        "Nice work!", 
    };

    void Start(){
        timer = FindObjectOfType<Timer>();
        mic = FindObjectOfType<Microph>();
        client = FindObjectOfType<Client>();
        audioPlayback = FindObjectOfType<AudioPlayback>();
        runManager = FindObjectOfType<RunManager>();
        imageController = FindObjectOfType<ImageController>();

        //subscribe to events 
        timer.Idle.AddListener(Idle); 
        mic.OnSpeechDetected.AddListener(StopTimer); 
        mic.SpeechToText.AddListener(Speech); 
        ObjectGrabbable.OnObjectPickedUp += PickUp;
        ObjectGrabbable.OnObjectDropped += Drop;
        audioPlayback.AudioPlaybackFinished.AddListener(StartTimer); 
        runManager.IntroductionIsOver.AddListener(IntroductionOver);
    }

    public void OnDisable(){
        //unsubscribe to events
        timer.Idle.RemoveListener(Idle);
        mic.OnSpeechDetected.RemoveListener(StopTimer);
        mic.SpeechToText.RemoveListener(Speech);
        ObjectGrabbable.OnObjectPickedUp -= PickUp; 
        ObjectGrabbable.OnObjectDropped -= Drop;
        audioPlayback.AudioPlaybackFinished.RemoveListener(StartTimer);
        runManager.IntroductionIsOver.RemoveListener(IntroductionOver); 
    }
    
    private void StopTimer(){
        if (!runManager.getIntroductionMode()){
            TimerStop.Invoke(); 
        }
    }

    private void StartTimer(){
        if (!runManager.getIntroductionMode()){
            TimerStart.Invoke(); 
        }
    }


    private void Speech(string speech){
        EventSpeech.Invoke(speech); //trigger FeedbackManager
    }

    private void Idle() {
        if (!runManager.getIntroductionMode()) {
            EventIdle.Invoke(); // trigger FeedbackManager
        }
    }
    
    private void Drop() { 
        if (!runManager.getIntroductionMode()) {
            StopTimer();
        }
    }
    
    private void PickUp(GameObject obj){
        if (!runManager.getIntroductionMode()) {
            StopTimer();
            client.PostRequest("http://127.0.0.1:5000/api/v1/evaluation", OnEvaluationRecieved, null, null, false);
        }
    }

    private void IntroductionOver(string response){
        EventPlayInstructions.Invoke(response); //notify audioplayback
    }

    public void OnEvaluationRecieved(string evaluation){
        Debug.Log("[EVALUATION] " + evaluation);

        if (evaluation == "true"){
            EventReset.Invoke(); //trigger FeedbackManager
            string instruction = client.RecieveNextInstruction(); //get next instruction
            if (runManager.getEnablePictogram()){
                client.PostRequest("http://127.0.0.1:5000/api/v1/instruction_image_support", imageController.OnImagesRecieved, null, null, false);
            }
            
            if (runManager.enableContinuousInstructions){ //validate user when evaluation is true
                var random = new System.Random();
                int count = random.Next(validation.Count);
                audioPlayback.OnInstructionsRecieved(validation[count] + instruction);
            }
        } 
        else { //response is 'false'
            EventInteraction.Invoke();
            // trigger FeedbackManager FeedbackInteraction
        }
            
    }
}