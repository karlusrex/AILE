using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchivedUI : MonoBehaviour
{

    
    public Canvas canvas; 
    private GameObject imgObject; 
    public int imageSize; 
    private static AchivedUI instance; // Static reference to the instance

     /* Property to access the instance from other scripts */
    public static AchivedUI Instance
    {
        get
        {
            if (instance == null) // If the instance doesn't exist, find it in the scene
            {
                instance = FindObjectOfType<AchivedUI>();

                if (instance == null)// If it still doesn't exist, create a new GameObject and add the script
                {
                    GameObject singletonObject = new GameObject("Achived UI singleton");
                    instance = singletonObject.AddComponent<AchivedUI>();
                }
            }
            return instance;
        }
    }

    public void AddUI(GameObject obj){
        string imageName =  "Pictogram/hold"; 
        imgObject = new GameObject("Image");

        try{
            imgObject.transform.SetParent(canvas.transform, false); 
            Image img = imgObject.AddComponent<Image>();
            Sprite imageSprite = Resources.Load<Sprite>(imageName); 
            img.sprite = imageSprite; 

            // Set canvas size relative to the object 
            RectTransform rectTransform = imgObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(imageSize, imageSize); // Adjust multiplier as needed

            // Calculate the position of the UI element relative to the object
            Vector3 offset = new Vector3(0.1f, 0.2f, 0.0f); 
            Vector3 targetPosition = obj.transform.position + offset;

            // Set the position of the UI element
            imgObject.transform.position = targetPosition;
        } catch(Exception e){
            Debug.Log(e.Message);    
        }
    }


    public void RemoveUI(GameObject obj){

          // Check if there is a UI instance
        if (imgObject != null)
        {
            Destroy(imgObject);
            imgObject = null; // Set the reference to null to indicate that the UI instance has been removed 
        }
    }
}
