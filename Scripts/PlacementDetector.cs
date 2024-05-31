using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.Assertions.Must;

/* 
Responsible for detect object below and above. 
*/
public class PlacementDetector : MonoBehaviour
{
    private GameObject inside; 
    private float rayLength = 0.20f; 
    bool onTheSide = false;
    private Dictionary<int, GameObject> sideToObject = new Dictionary<int, GameObject>();  // hashmap storing number and objects

    public void VerticalRaycast(Vector3 transformStartPosition, Vector3 transformDirectionVector, float transformationAngle) {
        {
        RaycastHit hit;

        if (Physics.Raycast(transformStartPosition, transformDirectionVector, out hit, rayLength))
            if (transformationAngle > 75 && transformationAngle < 105) {
                onTheSide = true;
                
                // disable on keep original rotation
                if (Vector3.Angle(Vector3.forward, transformDirectionVector) < 45) {
                    sideToObject.Add(3, hit.collider.gameObject);
                } else if (Vector3.Angle(Vector3.back, transformDirectionVector) < 45) {
                    sideToObject.Add(2, hit.collider.gameObject);
                } else if (Vector3.Angle(Vector3.right, transformDirectionVector) < 45) {
                    sideToObject.Add(4, hit.collider.gameObject);
                } else if (Vector3.Angle(Vector3.left, transformDirectionVector) < 45) {
                    sideToObject.Add(5, hit.collider.gameObject);
                }

            } else {
                if (Vector3.Angle(Vector3.up, transformDirectionVector) < 45) {
                    onTheSide = false;
                    sideToObject.Add(1, hit.collider.gameObject);
                } else if (Vector3.Angle(Vector3.down, transformDirectionVector) < 45) {
                    onTheSide = false;
                    sideToObject.Add(0, hit.collider.gameObject);
                }
            }
        }
    }

    public void HorizontalRaycast(Vector3 transformStartPosition, Vector3 transformDirectionVector, float transformationAngle) {
        {
        RaycastHit hit;
        if (Physics.Raycast(transformStartPosition, transformDirectionVector, out hit, rayLength)) 
            if (onTheSide && transformationAngle < 45) { // on the side and facing down
                sideToObject.Add(0, hit.collider.gameObject);
            } else if (onTheSide && transformationAngle > 135) { // on the side and facing up
                sideToObject.Add(1, hit.collider.gameObject);
            } else {
                // disable on keep original rotation
                if (Vector3.Angle(Vector3.forward, transformDirectionVector) < 45) {
                    sideToObject.Add(2, hit.collider.gameObject);
                } else if (Vector3.Angle(Vector3.back, transformDirectionVector) < 45) {
                    sideToObject.Add(3, hit.collider.gameObject);
                } else if (Vector3.Angle(Vector3.right, transformDirectionVector) < 45) {
                    sideToObject.Add(4, hit.collider.gameObject);
                } else if (Vector3.Angle(Vector3.left, transformDirectionVector) < 45) {
                    sideToObject.Add(5, hit.collider.gameObject);
                }
            }
        }
    }
    

    IEnumerator GetObjectRelations(System.Action<GameObject, List<GameObject>> callback){
        while (true) {
            List<GameObject> objects = new List<GameObject>();

            if (inside == null){ //object is not inside other object
                Collider playerCollider = GetComponent<Collider>();
                Vector3 playerSize = playerCollider.bounds.size;

                Vector3 localTop =      new Vector3(0,                          playerSize.y + 0.01f,   0                         );
                Vector3 localForward =  new Vector3(0,                          playerSize.y/2,         playerSize.z / 2  + 0.01f );
                Vector3 localBack =     new Vector3(0,                          playerSize.y/2,         -playerSize.z / 2 - 0.01f );
                Vector3 localRight =    new Vector3(playerSize.x / 2 + 0.01f,   playerSize.y/2,         0                         );
                Vector3 localLeft =     new Vector3(-playerSize.x / 2 - 0.01f,  playerSize.y/2,         0                         );

                Vector3 worldTop =      transform.TransformPoint(localTop);
                Vector3 worldForward =  transform.TransformPoint(localForward);
                Vector3 worldBack =     transform.TransformPoint(localBack);
                Vector3 worldRight =    transform.TransformPoint(localRight);
                Vector3 worldLeft =     transform.TransformPoint(localLeft);
                Vector3 worldDown =     transform.TransformPoint(new Vector3(0, 0.01f, 0));

                // ======================== DEBUGGING PURPOSES ============================== // 
                /*
                Debug.DrawRay(worldTop,     transform.up       * rayLength * 2,   Color.red);
                Debug.DrawRay(worldDown,    -transform.up      * rayLength * 2,   Color.green);
                Debug.DrawRay(worldForward, transform.forward  * rayLength * 2,   Color.blue);
                Debug.DrawRay(worldBack,    -transform.forward * rayLength * 2,   Color.yellow);
                Debug.DrawRay(worldRight,   transform.right    * rayLength * 2,   Color.magenta);
                Debug.DrawRay(worldLeft,    -transform.right   * rayLength * 2,   Color.cyan);
                */ 
                // ======================== DEBUGGING PURPOSES ============================== // 

                float angleDown =        Vector3.Angle(Vector3.down,    -transform.up);
                float angleUp =          Vector3.Angle(Vector3.up,      transform.up);
                float angleForwardDown = Vector3.Angle(Vector3.down, transform.forward);
                float angleBackDown =    Vector3.Angle(Vector3.down, -transform.forward);
                float angleRightDown =   Vector3.Angle(Vector3.down, transform.right);
                float angleLeftDown =    Vector3.Angle(Vector3.down, -transform.right);

                VerticalRaycast(worldDown,      -transform.up,      angleDown);
                VerticalRaycast(worldTop,       transform.up,       angleUp);
                HorizontalRaycast(worldForward, transform.forward,  angleForwardDown);
                HorizontalRaycast(worldBack,    -transform.forward, angleBackDown);
                HorizontalRaycast(worldRight,   transform.right,    angleRightDown);
                HorizontalRaycast(worldLeft,    -transform.right,   angleLeftDown);

                // go through dictionary and add to list from 0 - 6
                for (int i = 0; i < 7; i++) {
                    if (sideToObject.ContainsKey(i)) {
                        objects.Add(sideToObject[i]);
                    } else {
                        objects.Add(null);
                    }
                }

            } else { //object is inside other gameobject

                // add null except nr 6 that is the object that encloses on index 6 
                for (int i = 0; i < 7; i++) {
                    if (i < 6){
                        objects.Add(null); 
                    } else {
                        objects.Add(inside);
                    }
                }
            }


            callback(gameObject, objects);

            // clear dictionary
            sideToObject.Clear();
            yield return new WaitForSeconds(.1f);
        }
    }

    public void GetRelationalObjects(System.Action<GameObject, List<GameObject>> callback){
        StartCoroutine(GetObjectRelations(callback));
    }

}