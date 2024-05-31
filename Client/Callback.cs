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

public class Callback : MonoBehaviour {
    private AudioPlayback audioPlayback; 
    private Client client; 
    private ImageController imageController;

    private RunManager runManager;
    public string url = "http://127.0.0.1:5000/api/v1/";

    void Start(){
        audioPlayback = FindObjectOfType<AudioPlayback>();
        client = FindObjectOfType<Client>();
        imageController = FindObjectOfType<ImageController>();
        runManager = FindObjectOfType<RunManager>();
    }
    
    public void OnLastEvaluation(string response) { 
        client.PostRequest(url + "evaluation", OnLastEvaluationRecieved, null, response, false);
    }

    public void OnLastEvaluationRecieved(string response) { 

        if (response == "true"){
            audioPlayback.OnInstructionsRecieved("Congratulations, you have completed the task."); 
        } else {
            client.Reset(); //Replay instructions from beginning
        }
    }

    public void OnTaskInstructionsRecieved(string response) { 
        if (response != null){

            string [] temp = response.Split('\n'); //split at new line 
            List<string> list_instructions = new List<string>();

            foreach (string s in temp) {
                if (s != "" || s != null){
                    list_instructions.Add(s); 
                }
            }

            string [] instructions            = list_instructions.ToArray();
            string [] task_instructions       = new string[instructions.Length]; 
            string [] winning_conditions      = new string[instructions.Length]; 

            Debug.Log("[CALLBACK] Instructions Recieved: " + response);


            for (int i = 0; i < instructions.Length; i++){
                string taskStep = instructions[i]; 

                if (taskStep != "\n" && taskStep != null && taskStep != "" && char.IsDigit(taskStep[0])){ //if not empty line and only if it begins with a number
                    string [] firstSubstrings   = taskStep.Split(":"); 
                    string [] secondSubstrings  = firstSubstrings[1].Split("Winning condition"); // [Instruction, Winning condition]
                    string instruction          = secondSubstrings[0]; // [Pick up the RedCube. ]
                    string winning_condition    = firstSubstrings[2];

                    task_instructions[i] = instruction;
                    winning_conditions[i] = winning_condition;
                }
            }

            // set in client 
            client.SetInstructions(task_instructions);
            client.SetConditions(winning_conditions);
            
            // Get brief task description for introduction in RunManager
            client.PostRequest(url + "task-brief", runManager.OverallInstructions, null, null, false);

        } else {
            Debug.LogError("[CALLBACK] Failed to get task instruction");
        }
    }
}
