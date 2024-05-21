/*
Responsible for managing the feedback to the user based on the users actions, voice input and if the user has been idle. 

*/
using UnityEngine;
using UnityEngine.Events;

public class FeedbackManager: MonoBehaviour{

    private float feedbackCounter;
    private GameObject virtualInteractionGameObject; // Object that glows
    private EmissionControl virtualInteractionEmissionControl; // EmissionController for object that glows
    private Client client; 
    private AudioPlayback audioPlayback; 
    private EventManager eventManager; 
    private RunManager runManager;

    private double max; 

    public event System.Action<string> CorrectionInstructionRecieved;

    private string uri = "http://127.0.0.1:5000/api/v1/";

    private void OnEnable()
    {
        //initialize objects
        audioPlayback = FindObjectOfType<AudioPlayback>(); 
        eventManager = FindObjectOfType<EventManager>(); 
        runManager = FindObjectOfType<RunManager>();

        // Subscribe to the events
        eventManager.EventIdle.AddListener(FeedbackIDLE); 
        eventManager.EventSpeech.AddListener(FeedbackQuestion);
        eventManager.EventInteraction.AddListener(FeedbackInteraction); 
        eventManager.EventReset.AddListener(ResetCounter);
    }

    private void OnDisable()
    {
        // Unsubscribe from the event
        eventManager.EventIdle.RemoveListener(FeedbackIDLE); 
        eventManager.EventSpeech.RemoveListener(FeedbackQuestion);
        eventManager.EventInteraction.RemoveListener(FeedbackInteraction); 
        eventManager.EventReset.RemoveListener(ResetCounter);
    }

    void Start() {
        feedbackCounter = 0.0f;
        client = FindObjectOfType<Client>(); 

        if (runManager.isSupportedMode()){
            max = 1.5; 
        } else {
            max = 3.0; 
        }
    }

    // If the evaluation is true, then reset the counter
    public void ResetCounter(){
        Debug.Log("[FEEDBACKMANAGER] Feedbackcounter = " + feedbackCounter);
        if (feedbackCounter >= max){
            DisableVirtualInteraction();
        }
        feedbackCounter = 0;

        //reset conversation history 
        client.GetRequest("http://127.0.0.1:5000/api/v1/clear-conversation", NullCallback);
    }
    public void NullCallback(string response) { } //null callback method

    /**
    * Feedback Interaction when user performs an interaction, if it evaluates to false add 1 points, and if at 3 it lights up the object
    * if under 3, then synonym instructions is generated.
    */
    void FeedbackIDLE() {
        Debug.Log(" [FEEDBACKMANAGER] Feedback timer");

        feedbackCounter = feedbackCounter + 1.0f;
        if (feedbackCounter >= max) {
            string shut_down = "Shutting down";
            audioPlayback.OnInstructionsRecieved(shut_down);
            Application.Quit();
        } else {
            string support = "Do you need help?";
            audioPlayback.OnInstructionsRecieved(support);
        }
    }

    /*
     * Feedback Interaction when user performs an interaction, if it evaluates to false add 1 points, and if at 3 it lights up the object
     * if under 3, then synonym instructions is generated
     */
    void FeedbackQuestion(string response) {
        Debug.Log(" [FEEDBACKMANAGER] Question: " + response);
        // If else for this dependent on runManager.getIntroductionMode() -> true send to RunManager, else send to AudioPlayback
        if (runManager.getIntroductionMode()) {
            // Debug.Log(" [Input]" + response);
            client.PostRequest("http://127.0.0.1:5000/api/v1/task-helper-question", runManager.CheckIfIntroductionConversationIsOver, null, response, false);
        } else {
            client.PostRequest("http://127.0.0.1:5000/api/v1/task-helper-question", audioPlayback.OnInstructionsRecieved, null, response, false);
        }
    }
    
    /*
     * Feedback Interaction when user performs an interaction, if it evaluates to false add 1/2 points, and if at 3 it lights up the object
     * if under 3, then synonym instructions is generated
     */
    void FeedbackInteraction() {
        feedbackCounter = feedbackCounter + 1.0f/2.0f;
        if (feedbackCounter >= max) {
            client.PostRequest("http://127.0.0.1:5000/api/v1/get-object-from-instruction", OnGameObjectRecieved, null, null, false);
        } else {
            client.PostRequest("http://127.0.0.1:5000/api/v1/get-correction-instruction", OnCorrectionInstructionsRecieved, null, null, false);
        }
    }
    
    /*
     * Removes the glow from the object when instruction is cor 
     */
    void DisableVirtualInteraction() {
        virtualInteractionEmissionControl.RemoveEmission();
        Destroy(virtualInteractionEmissionControl);
        virtualInteractionGameObject = null;
    }

    /*
    * Callback method that recieves object mentioned in the instruction step, 
    * uses the name to find the object and add a highlight to it.
    * - Uses EmissionControl to enable highlighting
    * @param: string gameObject
    * @returns void
    */
    void OnGameObjectRecieved(string gameObject){
        // Debug.Log("[FeedbackManager] GameObjectRecieved: " + gameObject.ToLower()); 
        virtualInteractionGameObject = GameObject.Find(gameObject.ToLower()); 
        // Debug.Log("Object: " + virtualInteractionGameObject.name);
        virtualInteractionGameObject.AddComponent<EmissionControl>(); //null pointer exception 
        EmissionControl EC = virtualInteractionGameObject.GetComponent<EmissionControl>();
        virtualInteractionEmissionControl = EC;
    }

    /* 
     * Callback method that recieves and saves new correction task instructions and winning conditions from webserver. 
     * - Notifies listeners that the task instructions has been recieved.  
     * @param: string response
     * @returns: void
     */
    public void OnCorrectionInstructionsRecieved(string response){
        Debug.Log("[FEEDBACKMANAGER] Correction Instruction: " + response); 

        if (response != null){
            RunManager runManager = FindObjectOfType<RunManager>();
            if (runManager.getEnablePictogram()) {
                ImageController imageController = FindObjectOfType<ImageController>();
                client.PostRequest("http://127.0.0.1:5000/api/v1/instruction_image_support", imageController.OnImagesRecieved, null, null, false);
            }
            CorrectionInstructionRecieved(response); 
        } else {
            Debug.LogError("[FEEDBACKMANAGER] Failed to get task instruction");
        }
    }

    public float GetFeedbackCounter(){
        return feedbackCounter; 
    }

}