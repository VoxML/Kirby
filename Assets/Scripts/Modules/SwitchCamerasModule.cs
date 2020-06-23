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

public class SwitchCamerasModule : ModuleBase
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

        DataStore.Subscribe("user:intent:isPushLeft", SwipeLeft);
        DataStore.Subscribe("user:intent:isPushRight", SwipeRight);
    }

    // Update is called once per frame
    void Update()
    {

    }

    // callback when user:intent:isPushLeft changes
    void SwipeLeft(string key, DataStore.IValue value)
    {
        if (DataStore.GetBoolValue(key))
        {
            cameraManager.SwitchCameraViews();
        }
    }

    // callback when user:intent:isPushRight changes
    void SwipeRight(string key, DataStore.IValue value)
    {
        if (DataStore.GetBoolValue(key))
        {
            cameraManager.SwitchCameraViews();
        }
    }
}
