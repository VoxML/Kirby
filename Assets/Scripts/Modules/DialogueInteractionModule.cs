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
    }

    // callback when user:isEngaged changes
    void CheckEngagement(string key, DataStore.IValue value)
    {
        if (DataStore.GetBoolValue(key))
        {
            SetValue("kirby:speech", "Hello.", string.Empty);
        }
        else
        {
            SetValue("kirby:speech", "Goodbye.", string.Empty);
        }
    }
}
