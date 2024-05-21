using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class RunManager : MonoBehaviour
{

    public bool enablePictogram = false; 
    public bool enableContinuousInstructions = false; 
    public bool introductionMode = true;
    private Client client;
    public bool supportedMode; 
    public bool semiManualMode; 
    public bool manualMode; 
    public bool isCubeScene; 

    public UnityEvent<string> IntroductionIsOver = new UnityEvent<string>();

    // Start is called before the first frame update
    void Start()
    {
        gameObject.AddComponent<Client>();
        gameObject.AddComponent<Microph>();
        GameObject audioManager = GameObject.Find("AudioManager");
        audioManager.AddComponent<AudioPlayback>();
        gameObject.AddComponent<EventManager>();
        gameObject.AddComponent<Timer>();
        gameObject.AddComponent<FeedbackManager>();
        gameObject.AddComponent<Callback>();

        client = FindObjectOfType<Client>(); 
        Callback callback = FindObjectOfType<Callback>();
        string start;

        PlayerPickUpDrop playerPickUpDrop = FindObjectOfType<PlayerPickUpDrop>();
        playerPickUpDrop.SetUp();

        if (isCubeScene){
            start = "The player is tasked with constructing a tower using all the available cubes within the context provided. The tower should be assembled on the table on top of the grey placement marker.";
        } else {
            start = "The player is tasked with sorting dinnerware and fruits into the right place. The items should be arranged according to their type, with similar items grouped together.";
        }
        
        client.PostRequest("http://127.0.0.1:5000/api/v1/get-instructions-from-ollama", callback.OnTaskInstructionsRecieved, null, start, false);
    
    }

    public bool isSupportedMode(){
        return supportedMode; 
    }

    public bool isSemiManualMode(){
        return semiManualMode; 
    }

    public bool isManualMode() {
        return manualMode;
    }

    public bool getEnablePictogram(){
        return enablePictogram; 
    }

    public bool getEnableContinuousInstructions() { 
        return enableContinuousInstructions; 
    }

    public bool getIntroductionMode() { 
        return introductionMode; 
    }

    // callback method for introduction
    public void OverallInstructions(string instructions) {
        string introduction = "Welcome to the task. You will be given a set of instructions to follow. Please follow the instructions carefully. If you have any questions, feel free to ask. Let's get started. ";
        string confirmation = " Do you understand the task?";
        string allInstructions = introduction + instructions + confirmation;
        client.AllTaskInstructionsFetchedMethod(allInstructions);
    }

    // Only used while introductionMode is true
    public void CheckIfIntroductionConversationIsOver(string response) {
        string end_conversation = "Good luck!";

        // check if response contains end_conversation
        if (response.Contains(end_conversation)) {
            IntroductionIsOver.Invoke(response); //Notify that introduction is over
            introductionMode = false;
        } else {
            client.AllTaskInstructionsFetchedMethod(response); // only play the response
        }

    }

}
