﻿using UnityEngine;
using System.Collections;
using System;
using VoxSimPlatform.Global;

using VoxSimPlatform.Core;

public class KirbyEventsModule : ModuleBase
{
    EventManager eventManager;

    // declare commandInput variable
    CommandInput commandInput;

    // declare worldKnowledge variable
    KirbyWorldKnowledge worldKnowledge;

    // Use this for initialization
    void Start()
    {
        // get game object named "BehaviorController"
        GameObject behaviorController = GameObject.Find("BehaviorController");
        // set eventManager = EventManager component on BehaviorController
        eventManager = behaviorController.GetComponent<EventManager>();

        // get CommandInput component on "KirbyManager" object
        GameObject kirbyManager = GameObject.Find("KirbyManager");
        commandInput = kirbyManager.GetComponent<CommandInput>();

        // get KirbyWorldKnowledge component on "KirbyWorldKnowledge" object
        GameObject kirbyWorldKnowledge = GameObject.Find("KirbyWorldKnowledge");
        worldKnowledge = kirbyWorldKnowledge.GetComponent<KirbyWorldKnowledge>();

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

        // extract the "to find" content from the event string
        // get the string value of "user:event:intent" and store in a variable V
        // get the top predicate of your event string variable
        // - use the GetTopPredicate method in the GlobalHelper class
        // - need VoxSimPlatform.Global C# using statement
        // if top predicate equals "find":
        // trim "find(" and the final ")" from the event string
        // store the remaining string in the worldKnowledge variable declared above
        string V = DataStore.GetStringValue("user:event:intent");
        string topPred = GlobalHelper.GetTopPredicate(V);
        Debug.Log(V);
        Debug.Log(topPred);
        Debug.Log(Equals(topPred, "find"));
        if (Equals(topPred, "find"))
        {
            string trimmed = V.Remove(0, 5);
            trimmed = trimmed.Remove(trimmed.Length - 1, 1);
            worldKnowledge.toFind = trimmed;
            Debug.Log("trimmed " + worldKnowledge.toFind);
            Debug.Log(GlobalHelper.GetTopPredicate(trimmed));

           


        }
        // post message "patrol" on commandInput (see NLUModule.cs for usage)
        commandInput.inputController.inputString = "patrol";
        commandInput.PostMessage(commandInput.inputController.inputString);
        Debug.Log("sent command?");
        
    }
}

