using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class AutoAddUICameraStack : MonoBehaviour
{
    [SerializeField] private Camera camera;
    private UniversalAdditionalCameraData cameraData;
    private void OnEnable()
    { 
        cameraData = Camera.main.GetUniversalAdditionalCameraData();
        if (!cameraData.cameraStack.Contains(camera))
        {
            cameraData.cameraStack.Add(camera);
        }
    }

    private void OnDisable()
    {
        if (Camera.main)
        {
            cameraData = Camera.main.GetUniversalAdditionalCameraData();
            if (cameraData.cameraStack.Contains(camera))
            {
                cameraData.cameraStack.Remove(camera);
            }
        }

    }
}
