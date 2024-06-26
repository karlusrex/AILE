using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

/* https://www.youtube.com/watch?v=2IhzPTS4av4 */

public class ObjectGrabbable : MonoBehaviour
{
    private Rigidbody objRigidbody;
    private Transform objectGrabPointTransform;

    private GameObject OnTopOf; //the game object this object is on top of 
    private bool currentlyHolding = false; //if the object is currently being held
    private Transform target;


    // Event to notify when the object is picked up
    public delegate void ObjectPickedUpEventHandler(GameObject obj);
    public static event ObjectPickedUpEventHandler OnObjectPickedUp;
    

    // Event to notify when the object is dropped
    public delegate void ObjectDroppedEventHandler();
    public static event ObjectDroppedEventHandler OnObjectDropped;

    IEnumerator LookingAtTarget() {
        while (true) {
            if (currentlyHolding) {
                Vector3 lookAtPosition = new Vector3(target.position.x, transform.position.y, target.position.z);
                Quaternion targetRotation = Quaternion.LookRotation(lookAtPosition - transform.position);
                float rotationSpeed = 100f; // Adjust this value to change the speed of rotation
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
            yield return new WaitForSeconds(0.03f);
        }
    }

    private void Start() {
        StartCoroutine(LookingAtTarget());
    }

    private void Awake()
    {
        objRigidbody = GetComponent<Rigidbody>(); 
        target = GameObject.Find("Player").transform;
    }

    public void Grab(Transform objectGrabPointTransform)
    {
        this.objectGrabPointTransform = objectGrabPointTransform;
        objRigidbody.useGravity = false;
        objRigidbody.isKinematic = true;
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        
        if (boxCollider != null) // if box collider is not null, disable it
        {
            boxCollider.enabled = false;
        }
        
        if (meshCollider != null) // if mesh collider is not null, disable it
        {
            meshCollider.enabled = false;
        }
        
        currentlyHolding = true;
        OnObjectPickedUp?.Invoke(gameObject); // Trigger the pick up event
    }

    public void GrabEasyMode(){ 
        OnObjectPickedUp?.Invoke(gameObject); // only trigger the pick up event
        AchivedUI.Instance.AddUI(gameObject); 
    }

    private void FixedUpdate()
    {
        if (objectGrabPointTransform != null)
        {
            float lerpSpeed = 10f;
            Vector3 newPos = Vector3.Lerp(transform.position, objectGrabPointTransform.position, Time.deltaTime * lerpSpeed);
            objRigidbody.MovePosition(newPos);
        }
    }

    public void Drop()
    {

        /*Check if semi manual mode mode is activated*/
        RunManager runManager = FindObjectOfType<RunManager>(); 

        if (runManager.isSemiManualMode()){
            GameObject closestObject = GetClosestObject();

            Debug.Log(closestObject); 

            if (closestObject != null){
                Collider targetCollider = closestObject.GetComponent<Collider>();
                Vector3 targetColliderPos = targetCollider.bounds.center; 
                Vector3 newPos = targetColliderPos + Vector3.up * (targetCollider.bounds.extents.y + gameObject.GetComponent<Collider>().bounds.extents.y + 0.05f); 
                Rigidbody rb = gameObject.GetComponent<Rigidbody>();
                rb.velocity = Vector3.zero; 
                gameObject.transform.position = newPos;
            }
        }

        this.objectGrabPointTransform = null;
        objRigidbody.useGravity = true;
        objRigidbody.isKinematic = false;
        OnObjectDropped?.Invoke(); // Trigger the drop event

        BoxCollider boxCollider = GetComponent<BoxCollider>();
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        
        if (boxCollider != null) // if box collider is not null, enable it
        {
            boxCollider.enabled = true;
        }
        
        if (meshCollider != null)// if mesh collider is not null, enable it
        {
            meshCollider.enabled = true;
        }
        currentlyHolding = false;
    }

    public void DropEasyMode(){ 
        AchivedUI.Instance.RemoveUI(gameObject);
        OnObjectDropped?.Invoke(); // Trigger the drop event
    }

    public void SetOnTopOf(GameObject obj)
    {
        OnTopOf = obj;
    }

    public GameObject GetOnTopOf()
    {
        if (OnTopOf == null)
            return null; 
        return OnTopOf; 
    }

    // credit: https://forum.unity.com/threads/how-to-find-the-nearest-object.360952/
    public GameObject GetClosestObject()
    {
        PlayerPickUpDrop playerPickUpDrop = FindObjectOfType<PlayerPickUpDrop>();
        GameObject[] MyListOfObjects = playerPickUpDrop.GetRaycastable();
        float closest = 0.5f; //add max range here
        GameObject closestObject = null;

        for (int i = 0; i < MyListOfObjects.Length; i++)  //list of gameObjects to search through
        {
            float dist = Vector3.Distance(MyListOfObjects[ i ].transform.position, gameObject.transform.position);
            if (dist < closest && MyListOfObjects[i] != gameObject)
            {
            closest = dist;
            closestObject = MyListOfObjects[ i ];
            }
        }
        return closestObject;
    }

    
}
