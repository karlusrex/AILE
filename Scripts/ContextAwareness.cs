using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System;
using Unity.VisualScripting;

/* Responsible for formatting context data */
public class ContextAwareness : MonoBehaviour
{
    GameObject player;
    Dictionary<GameObject, List<GameObject>> gameobjectRelation = new Dictionary<GameObject, List<GameObject>>(); //maps gameobject to object below and above
    GameObject[] pickupable;
    GameObject[] interactable;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        pickupable = GameObject.FindGameObjectsWithTag("Pickupable"); //get items with "pickupable" tag 
        interactable = GameObject.FindGameObjectsWithTag("Interactable"); //get items with "interactable" tag

        foreach(GameObject obj in pickupable){
            PlacementDetector pd = obj.GetComponent<PlacementDetector>(); //get script attached to the gameobject
            pd.GetRelationalObjects(OnGameObjectRecieved);
        }
        
    }

    private void OnGameObjectRecieved(GameObject obj, List<GameObject> relationalObjects){ //object below, object above, inside of object

        if (gameobjectRelation.ContainsKey(obj)){
            gameobjectRelation[obj] = relationalObjects;
        } else {
            gameobjectRelation.Add(obj, relationalObjects);
        }
    }

    /* 
        Generates context in a human like way.
    */ 
    public string getContext(){ 
        Dictionary<string, List<GameObject>> layerToObjects = new Dictionary<string, List<GameObject>>(); //contains the data
        
        /* Create context text */
        StringBuilder sb = new StringBuilder(); 

        //get type of objects 
        foreach(GameObject obj in pickupable){
            int layer = obj.layer; 
            string type = LayerMask.LayerToName(layer); 

            if (layerToObjects.ContainsKey(type)){
                layerToObjects[type].Add(obj);
            } else  {
                layerToObjects.Add(type, new List<GameObject> {obj});
            }
        }

        //List all pickupables in the context
        foreach (KeyValuePair<string, List<GameObject>> keyValue in layerToObjects){
            sb.Append("There are ").Append(keyValue.Value.Count).Append(" ").Append(keyValue.Key.ToLower()).Append(".");
            sb.Append("These ").Append(keyValue.Key.ToLower()).Append(" are available: "); 

            foreach (GameObject obj in keyValue.Value){
                sb.Append(obj.name.ToLower()); 
                sb.Append(", "); 
            }

            sb.Remove(sb.Length - 2, 1);
            sb.Append(".");
        }

        sb.Append("There is also a ");

        //List all interactable in the context
        foreach(GameObject obj in interactable){
            sb.Append(obj.name.ToLower()); 
            sb.Append(" and a "); 
        }

        sb.Remove(sb.Length - 7, 7);
        sb.Append(" that the player can interact with");
        sb.Append(".");

        
        /* Specificy if user is holding object */
        PlayerPickUpDrop pickUp = FindObjectOfType<PlayerPickUpDrop>();
        string playerHoldingObject = null; 

        if (pickUp.objectGrabbable != null)
        {
            sb.Append("The player is currently holding the ");
            playerHoldingObject = pickUp.objectGrabbable.gameObject.name; //get name of gameobject
            sb.Append(playerHoldingObject); //name of object user if holding
            sb.Append("."); 
            
        } else { //add information that player is not holding an object 
            sb.AppendLine("Player is not holding a object.");
        }

        //List relationships between objects 
        foreach(GameObject obj in gameobjectRelation.Keys){
            
            List<GameObject> otherGameobjects = gameobjectRelation[obj]; //below, above, in front, behind, right, left, enclosing

            if ( obj.name != playerHoldingObject){ 
                sb.AppendLine();
                sb.Append(obj.name);

                if(otherGameobjects[0] != null){ 
                    sb.Append(" is on top of ");
                    sb.Append(otherGameobjects[0].name); //gameobject below 
                    sb.Append(".");
                } 
                
                if (otherGameobjects[1] != null){
                    sb.Append(" and is directly beneath "); 
                    sb.Append(otherGameobjects[1].name); //the gameobject above
                    sb.Append(".");
                }

                //Do we need this? 
                if (otherGameobjects[2] != null){
                    sb.Append(" and is in front of "); 
                    sb.Append(otherGameobjects[2].name); //the gameobject behind
                    sb.Append(".");
                }

                if (otherGameobjects[3] != null){
                    sb.Append(" and is behind "); 
                    sb.Append(otherGameobjects[3].name); //the gameobject in front
                    sb.Append(".");
                }

                if (otherGameobjects[4] != null){
                    sb.Append(" and is to the left of "); 
                    sb.Append(otherGameobjects[4].name); //the gameobject to the right
                    sb.Append(".");
                }

                if (otherGameobjects[5] != null){
                    sb.Append(" and is to the right of "); 
                    sb.Append(otherGameobjects[5].name); //the gameobject to the left
                    sb.Append(".");
                } 

                if (otherGameobjects[6] != null){
                    sb.Append(" is inside "); 
                    sb.Append(otherGameobjects[6].name); //gameobject that encloses this object
                    sb.Append(".");
                }
            }
        }
        Debug.Log("[CONTEXT]: " + sb.ToString());
        return sb.ToString();
    }
   
}
