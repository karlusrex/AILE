// Get request example from unity 
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Security.Cryptography.X509Certificates;
using UnityEngine.UIElements;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine.Events;
using System.Text.RegularExpressions;
using System.Text;

public class Client : MonoBehaviour
{
    private string response;
    private ContextAwareness contextAwareness; 
    public string[] task_instructions; //task instructions for the user
    public string [] winning_conditions; //winning coniditions of the task instructions 
    private int current_task_step = 0; //current step in the task instruction
    public int getCurrentTaskStep() { return current_task_step; }
    public event System.Action<string> AllTaskInstructionsFetched; 
    public string url = "http://127.0.0.1:5000/api/v1/";
    private Callback callback;

    private List<string> imageNames = new List<string>(); 

    void Start()
    {
        contextAwareness = FindObjectOfType<ContextAwareness>();
        callback = FindObjectOfType<Callback>();

        //Load available image names
        Sprite[] sprites = Resources.LoadAll<Sprite>("Pictogram");
        List<string> names = new List<string>();
        
        foreach(Sprite sprite in sprites){
            names.Add(sprite.name);
        }
    }

    // GetSpeechToText, GetTaskSupportIdle
    public void GetRequest(string uri, Action<string> callback)
    {
        StartCoroutine(HTTPGetRequest(uri, callback));
    }

    /*
     * @param: string uri - the URI to make the request to
     * @param: Action<string> callback - the callback method to forward the response to
     * @param: string text - anything that requires text, previously txt, question etc. 
     * @param: bool isAudio - if the request is an audio file (Special Case)
     */
    // Replaces GenerateImages, GenerateOverallTaskGoal, GenerateNewCorrectionInstruction, 
    // GenerateGetRightObjectFromInstruction, RecieveTaskInstructions
    // SPECIAL CASE: TextToAudioFile
    public void PostRequest(string uri, Action<string> callback1, Action<AudioClip> callback2, string text, bool isAudio)
    {
        StartCoroutine(HTTPPostRequest(uri, callback1, callback2, text, isAudio));
    }

    /*
     * Performs a HTTP GET request to specified URI. 
     * @param: string uri - the URI to make the request to
     * @param: Action<string> callback - the callback method to forward the response to
     * @returns: void
     */
    IEnumerator HTTPGetRequest(string uri, Action<string> callback)
    {

        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            yield return webRequest.SendWebRequest();
            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    callback(null);
                    break;
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    callback(null);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    response = webRequest.downloadHandler.text;
                    Debug.LogError("Response: " + response);
                    callback(null);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log("[CLIENT] Get Request: " + pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                    response = webRequest.downloadHandler.text; //recieve text response
                    callback(response); //forward response to callback method
                    break;
            }
        }
    }

    /*
     * Performs HTTP POST request to specified URI.
     * @param: string uri - the URI to make the request to
     * @param: Action<string> callback - the callback method to forward the response to
     * @param: string text - the text to send in the request
     * @param: bool isAudio - if the request is an audio file (Special Case)
     */
    IEnumerator HTTPPostRequest(string uri, Action<string> callback1, Action<AudioClip> callback2, string text, bool isAudio)
    {
        yield return new WaitForSeconds(1.0f);
        WWWForm form = new WWWForm();
        string optional_string; // occur 119, 247
        
        // set to empty string if null, else text
        if (text == null) { optional_string = "";} 
        else { optional_string = text; }

        string image_words              = string.Join(", " , imageNames); // 381
        string task_instruction         = task_instructions != null ? task_instructions[current_task_step] : ""; // 463
        string task_instruction_all     = task_instructions != null ? string.Join(", " , task_instructions) : ""; // 464
        string task_winning_condition   = winning_conditions != null ? winning_conditions[current_task_step] : ""; // 465
        string virtual_context          = contextAwareness.getContext(); // 164, 294, 338

        form.AddField("optional_string",        optional_string); // txt, 
        form.AddField("image_words",            image_words);
        form.AddField("task_instruction",       task_instruction);
        form.AddField("task_instruction_all",   task_instruction_all);
        form.AddField("task_winning_condition", task_winning_condition);
        form.AddField("virtual_context",        virtual_context);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(uri, form))
        {
            if (isAudio) {
                webRequest.downloadHandler = new DownloadHandlerAudioClip(uri, AudioType.MPEG);
            }
            yield return webRequest.SendWebRequest();

            string[] pages  = uri.Split('/');
            int page        = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    callback1(null);
                    break;
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    callback1(null);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    callback1(null);
                    break;
                case UnityWebRequest.Result.Success:
                    if (!isAudio) {
                        response = webRequest.downloadHandler.text;
                        Debug.Log("[CLIENT]" + response);
                        callback1(response);
                    } else {
                        AudioClip audioClip = DownloadHandlerAudioClip.GetContent(webRequest);
                        callback2(audioClip); 
                    }
                    break;
                default:
                    Debug.LogError("Unknown error");
                    break; 
            }
        }
    }
    
    // ======================================== CALLBACKS ============================================ //

    public string RecieveNextInstruction(){ // CHECK
        if (current_task_step < task_instructions.Length - 1){ 
            return task_instructions[++current_task_step];
        } else {
            PostRequest(url + "get-overall-task-goal", callback.OnLastEvaluation, null, null, false); // NEW
            // GenerateOverallTaskGoal("http://127.0.0.1:5000/api/v1/get-overall-task-goal", OnLastEvaluation);
            return "I'm going to check if you have completed the task. Please wait a moment.";    
        }
    }

    public void Reset() {
        current_task_step = 0;
        AllTaskInstructionsFetched(task_instructions[current_task_step]);

    }

    public void SetInstructions(string[] instructions){
        task_instructions = instructions;
    }

    public void SetConditions(string[] conditions){
        winning_conditions = conditions;
    }

    public void AllTaskInstructionsFetchedMethod(string instruction){
        AllTaskInstructionsFetched(instruction);
    }

    public string GetInstruction(){
        return task_instructions[current_task_step];
    }
}