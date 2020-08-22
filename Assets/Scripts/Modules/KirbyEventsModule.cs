using UnityEngine;
using System.Collections;
using System;

using VoxSimPlatform.Core;

public class KirbyEventsModule : ModuleBase
{
    EventManager eventManager;

    // TODO: declare commandInput variable (type CommandInput)

    // TODO: decalre worldKnowledge variable (type KirbyWorldKnowledge)

    // Use this for initialization
    void Start()
    {
        // get game object named "BehaviorController"
        GameObject behaviorController = GameObject.Find("BehaviorController");
        // set eventManager = EventManager component on BehaviorController
        eventManager = behaviorController.GetComponent<EventManager>();

        // TODO: get CommandInput component on "KirbyManager" object

        // TODO: get KirbyWorldKnowledge component on "KirbyWorldKnowledge" object

        // create a DataStore subscriber for the key "user:event:intent" to trigger "PromptEvent"
        DataStore.Subscribe("user:event:intent", PromptEvent);
        // add event handler delegate
        eventManager.NonexistentEntityError += StartLooking;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PromptEvent(string key, DataStore.IValue value)
    {
        // get the value of key and store it in a string variable
        string v = DataStore.GetStringValue(key);

        // if that variable is not null of empty
        if (!string.IsNullOrEmpty(v))
        {
            // prompt the event
            eventManager.InsertEvent(string.Empty, 0);
            eventManager.InsertEvent(v, 1);
        }

    }

    void FIND(object[] args)
    {

    }

    void StartLooking(object sender, EventArgs e)
    {
        Debug.Log("Made it to StartLooking");

        // TODO: extract the "to find" content from the event string
        // get the string value of "user:event:intent" and store in a variable V
        // get the top predicate of your event string variable
        // - use the GetTopPredicate method in the GlobalHelper class
        // - need VoxSimPlatform.Global C# using statement
        // if top predicate equals "find":
        // trim "find(" and the final ")" from the event string
        // store the remaining string in the worldKnowledge variable declared above

        // TODO: post message "patrol" on commandInput (see NLUModule.cs for usage)
    }

   
}

