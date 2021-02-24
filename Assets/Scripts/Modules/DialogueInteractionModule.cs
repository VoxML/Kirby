﻿/*
This script manages the dialogue state between Kirby and the user

Reads:      

Writes:     kirby:isAttending:speech (BoolValue, whether or not Kirby is attending
                to human speech
            kirby:isAttending:gesture (BoolValue, whether or not Kirby is attending
                to human gesture               
*/

using UnityEngine;
using System;

using VoxSimPlatform.Agent.CharacterLogic;
using VoxSimPlatform.Interaction;

public class DialogueInteractionModule : ModuleBase
{
    MapUpdater mapUpdater;
    KirbySpeechModule speech;
    
    public DialogueStateMachine stateMachine;


    // Use this for initialization
    void Start()
    {
        base.Start();

        stateMachine.scenarioController = gameObject.GetComponent<SingleAgentInteraction>();

        mapUpdater = GameObject.Find("KirbyManager").GetComponent<MapUpdater>();
        speech = GameObject.Find("KirbyManager").GetComponent<KirbySpeechModule>();

        if (mapUpdater == null)
        {
            Debug.LogWarning("DialogueInteractionModule.Start: Could not find MapUpdater.  Expect errors!");
        }
        else
        {
            mapUpdater.MapInited += StartAttending;
        }

        // here we listen for all changes
        DataStore.instance.onValueChanged.AddListener(ValueChanged);

        DataStore.Subscribe("user:isEngaged", CheckEngagement);
        DataStore.Subscribe("user:intent:isServoLeft", CheckServoLeft);
        DataStore.Subscribe("user:intent:isServoRight", CheckServoRight);
        DataStore.Subscribe("user:intent:isServoBack", CheckServoBack);
        DataStore.Subscribe("user:intent:isPosack", CheckPosack);
        DataStore.Subscribe("user:intent:isNegack", CheckNegack);
        DataStore.Subscribe("user:intent:isNevermind", CheckNevermind);
        DataStore.Subscribe("user:lastPointedAt:name", PointedAtObject);
        DataStore.Subscribe("user:lastPointedAt:position", PointedAtLocation);
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    protected override void ValueChanged(string key)
    {
        // in this method we listen for all key changes

        // rewrite the stack with the new blackboard state
        //  this only triggers a state change in a few cases
        //  (see DialogueStateMachine.cs)
        stateMachine.RewriteStack(new PDAStackOperation(PDAStackOperation.PDAStackOperationType.Rewrite,
            stateMachine.GenerateStackSymbol(DataStore.instance)));
    }

    // callback from MapUpdated when map inited for first time
    void StartAttending(object sender, EventArgs e)
    {
        SetValue("kirby:isAttending:speech", true, string.Empty);
        SetValue("kirby:isAttending:gesture", true, string.Empty);

        if (DataStore.GetBoolValue("user:isInteracting"))
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
            if (DataStore.GetBoolValue("user:isInteracting"))
            {
                SetValue("kirby:speech", "Goodbye.", string.Empty);
            }
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
    }

    // callback when user:intent:isNegack changes
    void CheckNegack(string key, DataStore.IValue value)
    {
    }

    // callback when user:intent:isNevermind changes
    void CheckNevermind(string key, DataStore.IValue value)
    {
    }

    // callback when user:lastPointedAt:name changes
    void PointedAtObject(string key, DataStore.IValue value)
    {
        if (DataStore.GetBoolValue("kirby:isAttending:gesture"))
        {
            SetValue("user:intent:object", DataStore.GetVector3Value(key), string.Empty);
        }
    }

    // callback when user:lastPointedAt:location changes
    void PointedAtLocation(string key, DataStore.IValue value)
    {
        if (DataStore.GetBoolValue("kirby:isAttending:gesture"))
        {
            SetValue("user:intent:location", DataStore.GetVector3Value(key), string.Empty);
        }
    }

    public void Ready(object content)
    {
        // do anything that needs to happen when we first enter the ready state
        SetValue("user:isInteracting", true, string.Empty);
    }

    public void ModularInteractionLoop(object content)
    {
        // do anything that needs to happen when we first enter the main
        //  interaction loop here
    }

    public void PatrollingLoop(object content)
    {
        // do anything that needs to happen when we first enter the patrolling
        //  loop here
    }

    public void QuestionAnsweringLoop(object content)
    {
        // do anything that needs to happen when we first enter the question
        //  answering loop here
    }

    public void CleanUp(object content)
    {
        // say goodbye!
        //  (and end the interaction, forget learned affordances)
        SetValue("kirby:speech", "Goodbye!", string.Empty);
        SetValue("user:isInteracting", false, string.Empty);
    }

    public void FilterLogFeedback(LogUpdate update)
    {
        Debug.Log("_________________________MADE IT TO DIM___________________");
        Debug.Log(update.code);
        Debug.Log(stateMachine.CurrentState.Name);
        string output = "";
        if (stateMachine.CurrentState.Name.Equals("ModularInteractionLoop"))
        {
            switch (update.code)
            {
                case "INVALID":
                    if (DataStore.GetBoolValue("kirby:isPatrolling"))
                    {
                        output = "";
                    }
                    else
                    {
                        output = "I don't understand.";
                    } 
                    break;

                case "PAUSED":
                    output = "Okay, I'll wait.";
                    break;

                case "CANCELLED_GOAL":
                    output = "Okay, cancelling that.";
                    break;

                case "CANCELLED_ALL":
                    output = "Okay, I cancelled everything.";
                    break;

                case "FORWARD":
                    output = "Sure!";
                    break;

                case "GO_TO":
                    output = "Okay!";
                    break;

                case "ESTIMATE_ROTATION":
                    output = "Okay, turning";
                    break;

                case "GO_BACK":
                    output = "Okay, I'll go back to where I was.";
                    break;

                case "SUCCESS_FORWARD":
                    output = "Made it!";
                    break;

                case "SUCCESS_GO_TO":
                    output = "Okay, I'm here.";
                    break;

                case "SUCCESS_VERIFY_ROTATION":
                    output = "Finished.";
                    break;

                case "SUCCESS_GO_BACK":
                    output = "Okay, I made it back.";
                    break;

                case "UNREACHABLE":
                    output = "I can't get there.";
                    break;

                case "STRAYED":
                    output = "Help, I moved but I couldn't make it to my goal.";
                    break;

                case "HELP":
                    output = "Do you want me to keep going from here or go back to " +
                        "where I was?";
                    break;

                case "PATROL":
                    output = "I'll explore.";
                    break;

                case "PLAN_LOOP":
                    output = "I'm planning where to explore next.";
                    break;

                case "STOP_PATROL":
                    output = "Okay, I'll stop exploring.";
                    break;

                case "FINISH_PATROL":
                    output = "I've explored as much as I can.";
                    DataStore.SetValue("kirby:patrol:finished", new DataStore.BoolValue(true), speech, string.Empty);
                    break;

                case "DEBUG":
                    output = update.message;
                    break;

                case "QUEUE":
                    output = update.message;
                    break;
            }
        
        }
        else if (stateMachine.CurrentState.Name.Equals("FindingLoop"))
        {
            switch (update.code)
            {
                case "PATROL":
                    output = "Okay, I'll look for " + DataStore.GetStringValue("kirby:target");
                    output = "";
                    break;

                case "STOP_PATROL":
                    if (DataStore.GetBoolValue("kirby:locatedObject"))
                    {
                        output = "Found it.";
                    }
                    else
                    {
                        output = "Ok. I will stop looking.";
                        DataStore.SetValue("kirby:isFinding", new DataStore.BoolValue(false), this, string.Empty);
                    }
                    break;

                case "SUCCESS_GO_TO":
                    output = "Here's " + DataStore.GetStringValue("kirby:target");
                    DataStore.SetStringValue("kirby:speech", new DataStore.StringValue(output), speech, string.Empty);
                    output = "";
                    DataStore.SetValue("kirby:isFinding", new DataStore.BoolValue(false), this, string.Empty);
                    break;

                case "QUEUE":
                    output = "I'm still looking.";
                    break;
            }
        }
        else if (stateMachine.CurrentState.Name.Equals("PatrollingLoop"))
        {
            switch (update.code)
            {
                case "PATROL":
                    output = "I'll explore.";
                    break;

                case "STOP_PATROL":
                    output = "Ok, I'll stop exploring";
                    break;

                case "PAUSED":
                    output = "Ok, I'll wait.";
                    break;

                case "QUEUE":
                    output = "I'm exploring.";
                    break;
            }
        }
            if (!output.Equals(""))
        {
            DataStore.SetStringValue("kirby:speech", new DataStore.StringValue(output), speech, string.Empty);
        }
        

    }

}
