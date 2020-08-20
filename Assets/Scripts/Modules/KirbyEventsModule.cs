using UnityEngine;
using System.Collections;
using System;

using VoxSimPlatform.Core;

public class KirbyEventsModule : ModuleBase
{
    EventManager eventManager;

    // Use this for initialization
    void Start()
    {
        // TODO: get game object named "BehaviorController"
        GameObject behaviorController = GameObject.Find("BehaviorController");
        // TODO: set eventManager = EventManager component on BehaviorController
        eventManager = behaviorController.GetComponent<EventManager>();
        // TODO: create a DataStore subscriber for the key "user:event:intent" to trigger "PromptEvent"
        //  (check out DialogueInteractionModule for an example of how to subscribe to keys in the DataStore
        DataStore.Subscribe("user:event:intent", PromptEvent);
        // TODO:
        eventManager.NonexistentEntityError += StartLooking;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PromptEvent(string key, DataStore.IValue value)
    {
        // TODO: get the value of key and store it in a string variable
        //  (check out KirbySpeechModule.Speak to see an example of getting a key value)
        string v = DataStore.GetStringValue(key);

        // TODO:
        //  if that variable is not null of empty
        if (!string.IsNullOrEmpty(v))
        {
            eventManager.InsertEvent(string.Empty, 0);
            eventManager.InsertEvent(v, 1);
        }

    }

    void FIND(object[] args)
    {

    }

    // TODO: Add NameOfUnknownObjectEventHandler event handler
    // see example: https://docs.microsoft.com/en-us/dotnet/api/system.eventhandler?view=netcore-3.1
    void StartLooking(object sender, EventArgs e)
    {
        Debug.Log("Made it to StartLooking");
    }

   
}

