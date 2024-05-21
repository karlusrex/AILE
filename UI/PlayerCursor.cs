using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCursor : MonoBehaviour
{
    [SerializeField] private Texture2D cursorTexture;
    private Vector3 cursorHotSpot;

    private void Start()
    {
        Cursor.SetCursor(cursorTexture, cursorHotSpot, CursorMode.Auto); 

    }
    
}
