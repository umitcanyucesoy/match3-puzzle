using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLevelSize : MonoBehaviour
{
    private void Start()
    {
        SetupCamera();
    }

    private void SetupCamera()
    {
        Camera.main.transform.position = new Vector3(((Board.instance.width - 1) / 2), (Board.instance.height - 1) / 2,
            Camera.main.transform.position.z);
        
        var aspectRatio = Camera.main.aspect;
        
        var verticalSize = Board.instance.height / 2f + Board.instance.borderSize;
        var horizontalSize = (Board.instance.width / 2f + Board.instance.borderSize) / aspectRatio;
        
        Camera.main.orthographicSize = (verticalSize > horizontalSize) ? verticalSize : horizontalSize; 
    }
}
