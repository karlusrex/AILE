using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using System;
using UnityEditor;
using Unity.VisualScripting;
using System.Text;

/* https://www.youtube.com/watch?v=2IhzPTS4av4 */


public class PlayerPickUpDrop : MonoBehaviour
{
    [SerializeField] private Transform playerCameraTransform;
    [SerializeField] private Transform objectGrabPointTransform;
    public ObjectGrabbable objectGrabbable;
    private bool supportedMode; 

    /* Two objects to be selected */
    private GameObject selectedPickUpObject; //can only be grabbable
    private GameObject targetObject; //can be either interacteble or grabbable - where select object is placed 

    private GameObject latestHitObject; //last object that was hit by raycast
    private GameObject priorSelectObject; //prior selected object - used if it needs to move to original position
    private Vector3 fromPos; //moved objects prior position 
    private Quaternion rotation; //moved objects prior rotation
    private EventManager eventManager; 
    private Client client;
    private RunManager runManager;
    private FeedbackManager feedbackManager; 
    private GameObject[] pickupable;
    private GameObject[] interactable;
    private GameObject[] raycastAble; // merge into this thing
    private float pickupDistance = 5.0f;
    

    void Start(){
        runManager   = FindObjectOfType<RunManager>(); 
        supportedMode     = runManager.isSupportedMode(); 
        interactable = GameObject.FindGameObjectsWithTag("Interactable"); //get items with "interactable" tag
        pickupable   = GameObject.FindGameObjectsWithTag("Pickupable"); //get items with "pickupable" tag 
        raycastAble  = pickupable.Concat(interactable).ToArray(); //merge
    }

    public void SetUp() {
        eventManager    = FindObjectOfType<EventManager>(); 
        feedbackManager = FindObjectOfType<FeedbackManager>();

        eventManager.EventInteraction.AddListener(Reset); //on false evaluation 
        eventManager.EventReset.AddListener(MoveObject); //on true evaluation 
    }
    
    public GameObject[] GetRaycastable(){
        return (GameObject[])raycastAble.Clone(); 
    }

    private void Update(){
        HandleGlow();

        if (!Input.GetKeyDown(KeyCode.E)) {
            return;
        }

        if (supportedMode){
            if (latestHitObject != null && selectedPickUpObject != null && targetObject == null) { // Player holds object
                if (latestHitObject != selectedPickUpObject){ //Drop the selected object upon hit object if not same object
                    DropGrabbedObject();
                }
            } else if (latestHitObject != null && targetObject == null && selectedPickUpObject == null){ //player does not hold an object 
                // this means the player has selected the object and should be sent to the evaluator 
                if (pickupable.Contains(latestHitObject)){
                    objectGrabbable = latestHitObject.GetComponent<ObjectGrabbable>();
                    objectGrabbable.GrabEasyMode(); //triggers event that EventManger listens to, that makes request to evaluation
                    selectedPickUpObject = latestHitObject;
                    priorSelectObject = null;
                }
            }
            
        } else { //not supported mode
            if (objectGrabbable != null) {
                DropGrabbedObject(); 
                return;
            }
            GrabObject();
        }
    }

    private void GrabObject(){
        float pickUpDistance = 5f;
        float raycastRadius = 0.1f; 

        if (Physics.SphereCast(playerCameraTransform.position, raycastRadius , playerCameraTransform.forward, out RaycastHit raycastHit, pickUpDistance))
        {
            if (raycastHit.transform.TryGetComponent(out objectGrabbable))
            {
                objectGrabbable.Grab(objectGrabPointTransform);
            }
        } 

    }

    private void DropGrabbedObject(){
        print("[INTERACTION] drop time: " + DateTime.Now.ToString("HH:mm:ss:fff"));
        if (supportedMode){
            targetObject = latestHitObject;
            objectGrabbable.DropEasyMode(); 
        } else {
            objectGrabbable.Drop();
        }
    
        objectGrabbable = null;

        //needs to be executed after object is set to null because of the context
        client = FindObjectOfType<Client>(); 
        eventManager = FindObjectOfType<EventManager>(); 
        runManager = FindObjectOfType<RunManager>();
        if (!runManager.getIntroductionMode()){
            if (supportedMode){
                MoveObject(); 
            }
            client.PostRequest("http://127.0.0.1:5000/api/v1/evaluation", eventManager.OnEvaluationRecieved, null, null, false); 
        }
    }

    private void MoveObject(){

        if (targetObject == null || selectedPickUpObject == null) { return; }
        
        fromPos = selectedPickUpObject.transform.position; 
        rotation = selectedPickUpObject.transform.rotation; 
        priorSelectObject = selectedPickUpObject; 

        Collider targetCollider = targetObject.GetComponent<Collider>();
        Vector3 targetColliderPos = targetCollider.bounds.center; 
        Vector3 newPos = targetColliderPos + Vector3.up * (targetCollider.bounds.extents.y + selectedPickUpObject.GetComponent<Collider>().bounds.extents.y); 
        selectedPickUpObject.transform.position = newPos;
        targetObject = null; 
        selectedPickUpObject = null; 
    }

    private void Reset(){

        if (priorSelectObject != null){ //Move back prior selected object to original position if correction has been made 
            priorSelectObject.transform.position = fromPos; //move back object
            priorSelectObject.transform.rotation = rotation;
            selectedPickUpObject = priorSelectObject; 
            objectGrabbable = selectedPickUpObject.GetComponent<ObjectGrabbable>();
            priorSelectObject = null;
            AchivedUI.Instance.AddUI(selectedPickUpObject);
        } else if (selectedPickUpObject != null && targetObject == null){ //if user has picked up wrong object 
            AchivedUI.Instance.RemoveUI(selectedPickUpObject);
            selectedPickUpObject = null; 
            objectGrabbable = null;
        }

    }

    /*
    * Responsible for managing the glow of the object the player has selected. 
    * Used by Update method. 
    */ 
    private void HandleGlow(){
        if (Physics.Raycast(playerCameraTransform.position, playerCameraTransform.forward, out RaycastHit hit, pickupDistance)) { //hit object
            GameObject hitObject = hit.collider.gameObject;

            if (raycastAble.Contains(hitObject)) { //if object is among the raycastabled ones that can be selected 

                if (latestHitObject == null) { //add glow
                    latestHitObject = hitObject;
                   
                    latestHitObject.AddComponent<EmissionControl>();

                  
                    if (latestHitObject.name == "Stack of plates"){
                        foreach (Transform child in latestHitObject.transform){
                            child.gameObject.AddComponent<EmissionControl>();
                        }
                    }

                } else if (latestHitObject != hit.collider.gameObject) { //remove glow
                    latestHitObject.GetComponent<EmissionControl>();
                    EmissionControl EC = latestHitObject.GetComponent<EmissionControl>();
                    EC.RemoveEmission(); 

                    if (latestHitObject.name == "Stack of plates"){
                        foreach (Transform child in latestHitObject.transform){
                            EmissionControl ec = child.gameObject.GetComponent<EmissionControl>();
                            ec.RemoveEmission();
                        }
                    }
                    latestHitObject = null;
                }
            }
        } else { // miss 
            //remove glow from latest hit object if its not selected
            if (latestHitObject != null && (latestHitObject != selectedPickUpObject)) {
                latestHitObject.GetComponent<EmissionControl>();
                EmissionControl EC = latestHitObject.GetComponent<EmissionControl>();
                EC.RemoveEmission();
                latestHitObject = null;
            }
        }
    }
}
