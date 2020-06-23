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

public class DialogueInteractionModule : ModuleBase
{
    MapUpdater mapUpdater;

    // Use this for initialization
    void Start()
    {
        base.Start();

        mapUpdater = GameObject.Find("KirbyManager").GetComponent<MapUpdater>();

        if (mapUpdater == null)
        {
            Debug.LogWarning("DialogueInteractionModule.Start: Could not find MapUpdater.  Expect errors!");
        }
        else
        {
            mapUpdater.MapInited += StartAttending;
        }
        
        DataStore.Subscribe("user:isEngaged", CheckEngagement);
        DataStore.Subscribe("user:intent:isServoLeft", CheckServoLeft);
        DataStore.Subscribe("user:intent:isServoRight", CheckServoRight);
        DataStore.Subscribe("user:intent:isServoBack", CheckServoBack);
        DataStore.Subscribe("user:intent:isPosack", CheckPosack);
        DataStore.Subscribe("user:intent:isNegack", CheckNegack);
        DataStore.Subscribe("user:intent:isNevermind", CheckNevermind);
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    // callback from MapUpdated when map inited for first time
    void StartAttending(object sender, EventArgs e)
    {
        SetValue("kirby:isAttending:speech", true, string.Empty);
        SetValue("kirby:isAttending:gesture", true, string.Empty);

        if (DataStore.GetBoolValue("user:isEngaged"))
        {
            SetValue("kirby:speech", "I'm ready to go.", string.Empty);
        }
    }

    // callback when user:isEngaged changes
    void CheckEngagement(string key, DataStore.IValue value)
    {
        if (DataStore.GetBoolValue(key))
        {
            if (DataStore.GetBoolValue("kirby:isAttending:speech") &&
                DataStore.GetBoolValue("kirby:isAttending:gesture"))
            { 
                SetValue("kirby:speech", "Hello. I'm ready to go.", string.Empty);
            }
            else
            {
                SetValue("kirby:speech", "Hello. Hold on, please.", string.Empty);
            }
        }
        else
        {
            SetValue("kirby:speech", "Goodbye.", string.Empty);
        }
    }

    // callback when user:intent:isServoLeft changes
    void CheckServoLeft(string key, DataStore.IValue value)
    {
    }

    // callback when user:intent:isServoRight changes
    void CheckServoRight(string key, DataStore.IValue value)
    {
    }

    // callback when user:intent:isServoBack changes
    void CheckServoBack(string key, DataStore.IValue value)
    {
    }

    // callback when user:intent:isPosack changes
    void CheckPosack(string key, DataStore.IValue value)
    {
        Debug.Log(string.Format("{0}: {1}", System.Reflection.MethodBase.GetCurrentMethod().Name, DataStore.GetBoolValue(key)));
    }

    // callback when user:intent:isNegack changes
    void CheckNegack(string key, DataStore.IValue value)
    {
        Debug.Log(string.Format("{0}: {1}", System.Reflection.MethodBase.GetCurrentMethod().Name, DataStore.GetBoolValue(key)));
    }

    // callback when user:intent:isNevermind changes
    void CheckNevermind(string key, DataStore.IValue value)
    {
        Debug.Log(string.Format("{0}: {1}", System.Reflection.MethodBase.GetCurrentMethod().Name, DataStore.GetBoolValue(key)));
    }
}
