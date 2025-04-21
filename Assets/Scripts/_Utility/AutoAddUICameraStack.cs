using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class AutoAddUICameraStack : MonoBehaviour
{
    [SerializeField] private Camera mCamera;
    private UniversalAdditionalCameraData cameraData;
    private void OnEnable()
    { 
        cameraData = Camera.main.GetUniversalAdditionalCameraData();
        if (!cameraData.cameraStack.Contains(mCamera))
        {
            cameraData.cameraStack.Add(mCamera);
        }
    }

    private void OnDisable()
    {
        if (Camera.main)
        {
            cameraData = Camera.main.GetUniversalAdditionalCameraData();
            if (cameraData.cameraStack.Contains(mCamera))
            {
                cameraData.cameraStack.Remove(mCamera);
            }
        }

    }
}
