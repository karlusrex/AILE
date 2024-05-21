using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
 * Attach to object to make it brighter
 */
public class EmissionControl : MonoBehaviour
{
    private Material material;
    private Color emissionColor;
    private RunManager runManager;
    void Start()
    {
        // Get the material from the Renderer component
        runManager = FindObjectOfType<RunManager>();
        
        if (runManager.isCubeScene) {
            material = GetComponent<Renderer>().material;

            // Get the base color of the material
            emissionColor = material.color;

            // Set the emission flag
            material.EnableKeyword("_EMISSION");
            
            // Set the emission color to the material color
            material.SetColor("_EmissionColor", emissionColor);
        } else {

            //loop trough every material and set emission 
            List <Material> materials = GetComponent<Renderer>().materials.ToList();

            foreach (Material material in materials) {
                // Set the emission flag
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", new Color(.3f, .3f, .3f, 0.5f)); // make the object brighter but not white
            }
        }
        
    }

    public void RemoveEmission() {

        if (runManager.isCubeScene){
            // Remove the emission flag
            material.DisableKeyword("_EMISSION");
        } else {
            //loop trough every material and set emission 
            List <Material> materials = GetComponent<Renderer>().materials.ToList();

            foreach (Material material in materials) {
                // Remove the emission flag
                material.DisableKeyword("_EMISSION");
            }

        }

        
    }
}
