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

using VoxSimPlatform.Agent;

public class MoveCameraModule : ModuleBase
{
    CameraManager cameraManager;

    // Use this for initialization
    void Start()
    {
        base.Start();

        cameraManager = GameObject.Find("CameraManager").GetComponent<CameraManager>();
        if (cameraManager == null)
        {
            Debug.LogError("SwitchCamerasModule.Start: Could not find CameraManager.  Expect errors!");
        }

        DataStore.Subscribe("user:intent:isClaw", EnterCameraMoveMode);
        DataStore.Subscribe("user:intent:isPosack", ExitCameraMoveMode);
        DataStore.Subscribe("user:intent:isServoLeft", RotateLeft);
        DataStore.Subscribe("user:intent:isServoRight", RotateRight);
        DataStore.Subscribe("user:intent:isServoBack", RotateBack);
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
        if (DataStore.GetBoolValue(key))
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
        }
    }

    // callback when user:intent:isServoRight changes
    void RotateRight(string key, DataStore.IValue value)
    {
        if (DataStore.GetBoolValue("user:isMovingCamera"))
        {
        }
    }

    // callback when user:intent:isServoBack changes
    void RotateBack(string key, DataStore.IValue value)
    {
        if (DataStore.GetBoolValue("user:isMovingCamera"))
        {
        }
    }
}
