/*
This script manages the dialogue state between Kirby and the user

Reads:      

Writes:     kirby:isAttending:speech (BoolValue, whether or not Kirby is attending
                to human speech
            kirby:isAttending:gesture (BoolValue, whether or not Kirby is attending
                to human gesture               
*/

using UnityEngine;
using System;
using System.Linq;
using System.Text.RegularExpressions;

using VoxSimPlatform.Core;

public class MoveCameraModule : ModuleBase
{
    GameObject camera;

    // Use this for initialization
    void Start()
    {
        base.Start();

        camera = GameObject.FindGameObjectWithTag("MainCamera");
        if (camera == null)
        {
            Debug.LogError("MoveCameraModule.Start: Could not find main camera.  Expect errors!");
        }

        DataStore.Subscribe("user:intent:isClaw", EnterCameraMoveMode);
        DataStore.Subscribe("user:intent:isPosack", ExitCameraMoveMode);
        DataStore.Subscribe("user:intent:isServoLeft", RotateLeft);
        DataStore.Subscribe("user:intent:isServoRight", RotateRight);
        DataStore.Subscribe("user:intent:isServoBack", RotateBack);
        DataStore.Subscribe("user:intent:isNevermind", ReturnToHome);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // callback when user:intent:isClaw changes
    void EnterCameraMoveMode(string key, DataStore.IValue value)
    {
        if (DataStore.GetBoolValue(key))
        {
            SetValue("user:isMovingCamera", true, string.Empty);
            Debug.Log("Entering camera move mode");
        }
    }

    // callback when user:intent:isPosack changes
    void ExitCameraMoveMode(string key, DataStore.IValue value)
    {
        if (!DataStore.GetBoolValue(key))
        {
            SetValue("user:isMovingCamera", false, string.Empty);
            Debug.Log("Exiting camera move mode");
        }
    }

    // callback when user:intent:isServoLeft changes
    void RotateLeft(string key, DataStore.IValue value)
    {
        if (DataStore.GetBoolValue("user:isMovingCamera"))
        {
            Debug.Log("Rotating camera: Left");
        }
    }

    // callback when user:intent:isServoRight changes
    void RotateRight(string key, DataStore.IValue value)
    {
        if (DataStore.GetBoolValue("user:isMovingCamera"))
        {
            Debug.Log("Rotating camera: Right");
        }
    }

    // callback when user:intent:isServoBack changes
    void RotateBack(string key, DataStore.IValue value)
    {
        if (DataStore.GetBoolValue("user:isMovingCamera"))
        {
            Debug.Log("Rotating camera: Back");
        }
    }

    // callback when user:intent:isNevermind changes
    void ReturnToHome(string key, DataStore.IValue value)
    {
        if (DataStore.GetBoolValue("user:isMovingCamera"))
        {
            Debug.Log("Returning camera to home");
            GhostFreeRoamCamera cameraMovementScript = camera.GetComponent<GhostFreeRoamCamera>();

            camera.transform.position = cameraMovementScript.cameraPosOrigin;
            camera.transform.rotation = cameraMovementScript.cameraRotOrigin;
        }
    }
}
