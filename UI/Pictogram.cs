using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class ImageController : MonoBehaviour
{
    public Canvas canvas; // Assign your Canvas in the Inspector
    private List<GameObject> images = new List<GameObject>();
    private float imageWidth = 150f;

    void Start()
    {
     
    }

   public void OnImagesRecieved(string response){

        Debug.Log("[PICTOGRAM] Images: " + response);
        string rgxPattern = "[^a-zA-Z\\s]"; //remove signs besides letters and spaces
        string parsed_response = response.ToLower();
        parsed_response = Regex.Replace(parsed_response, rgxPattern, "");

        string[] images = parsed_response.Split(" "); 
        RemoveAllImages();

        foreach (string image in images){
            Debug.Log("[PICTOGRAM]: Adding: " + image);
            AddImage("Pictogram/"+image);
        }

    } 

    public void AddImage(string imageName)
    {
        Object file = Resources.Load(imageName); 
        if (file != null){
            GameObject imgObject = new GameObject("Image");
            imgObject.transform.SetParent(canvas.transform, false); // Set the canvas as parent

            Image image = imgObject.AddComponent<Image>();
            Sprite sprite = Resources.Load<Sprite>(imageName); // Load the sprite from Resources
            image.sprite = sprite; // Set the sprite to the image

            RectTransform rectTransform = imgObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1); // Top left corner
            rectTransform.anchorMax = new Vector2(0, 1); // Top left corner
            rectTransform.pivot = new Vector2(0, 1); // Top left corner
            rectTransform.anchoredPosition = new Vector2(images.Count * imageWidth, 0); // Position at the top left corner of the canvas
            rectTransform.sizeDelta = new Vector2(imageWidth, imageWidth); // Set the size of the image

            images.Add(imgObject);
        } else {
            Debug.Log("[PICTOGRAM] Image is null");
        }
    }

    public void RemoveAllImages()
    {
        while (images.Count > 0)
        {
            GameObject lastImage = images[images.Count - 1];
            images.RemoveAt(images.Count - 1);
            Destroy(lastImage);
        }
    }
}
