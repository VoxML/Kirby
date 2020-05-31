/*
This script manages the dialogue state between Kirby and the user

Reads:      

Writes:     kirby:isListening (BoolValue, whether or not Kirby is listening
                to human speech
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
            mapUpdater.MapInited += StartListening;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    // callback from MapUpdated when map inited for first time
    void StartListening(object sender, EventArgs e)
    {
        SetValue("kirby:isListening", true, string.Empty);
    }
}
