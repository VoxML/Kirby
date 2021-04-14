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

using VoxSimPlatform.Agent.CharacterLogic;
using VoxSimPlatform.Interaction;

public class DialogueInteractionModule : ModuleBase
{
    MapUpdater mapUpdater;
    KirbySpeechModule speech;
    
    public DialogueStateMachine stateMachine;

    KirbyWorldKnowledge worldKnowledge;
    KirbyEventsModule eventsModule;


    // Use this for initialization
    void Start()
    {
        base.Start();

        stateMachine.scenarioController = gameObject.GetComponent<SingleAgentInteraction>();

        mapUpdater = GameObject.Find("KirbyManager").GetComponent<MapUpdater>();
        speech = GameObject.Find("KirbyManager").GetComponent<KirbySpeechModule>();
        Debug.Log("Events Module" + eventsModule);

        GameObject kirbyWorldKnowledge = GameObject.Find("KirbyWorldKnowledge");
        worldKnowledge = kirbyWorldKnowledge.GetComponent<KirbyWorldKnowledge>();

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
        Debug.Log("Handling feedback " + update.code);
        Debug.Log("Current Dialog State: " + stateMachine.CurrentState.Name);
        string output = "";
        if (update.code.Equals("INVALID"))
        {
            return;
        }
        if (stateMachine.CurrentState.Name.Equals("ModularInteractionLoop"))
        {
            switch (update.code)
            {
                case "INVALID":
                    // There is sometimes noise when Kirby is/starts patrolling,
                    // don't send feedback from Kirby in this case
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
                    DataStore.SetValue("kirby:isPatrolling", new DataStore.BoolValue(true), this, string.Empty);
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
            Debug.Log("Finding " + update.code);
            Debug.Log("Finding events: " + (DataStore.GetBoolValue("kirby:lookingForMore")));
            switch (update.code)
            {
                case "PATROL":
                    Debug.Log("Finding seen as patrol");
                    //if (!eventsModule.lookForMore)
                    //{
                    //    
                    //}
                    //output = "Ok I'll look for " + DataStore.GetStringValue("kirby:target");
                    if (DataStore.GetBoolValue("kirby:lookingForMore"))
                    {
                        // if we are looking for more Kirby has already sent feedback about this. 
                        output = "";
                        Debug.Log("I think we're looking for more");
                    }
                    else
                    {
                        // If Kirby is finding and starts patrolling, he is looking for something
                        output = "Okay, I'll look for" + DataStore.GetStringValue("kirby:target");
                        Debug.Log("I'm trying to talk");
                    }
                    Debug.Log("Patrol output: " + output);
                    break;

                case "STOP_PATROL":
                    // If this command was published because we found the target object,
                    // and this isn't a case where we're looking for two objects
                    if (DataStore.GetBoolValue("kirby:locatedObject") && !DataStore.GetBoolValue("kirby:lookingForMore"))
                    {
                        output = "Found it.";
                    }
                    // If this command was published because the user wants to stop looking
                    else if (!DataStore.GetBoolValue("kirby:disambiguating"))
                    {
                        output = "Ok. I will stop looking.";
                        DataStore.SetValue("kirby:isFinding", new DataStore.BoolValue(false), this, string.Empty);
                    }
                    else
                    {
                        output = "";
                    }
                    worldKnowledge.toFind = "";
                    break;

                case "FINISH_PATROL":
                    String target = DataStore.GetStringValue("kirby:target");
                    String color = worldKnowledge.ExtractColorFromToFind();
                    String shape = worldKnowledge.ExtractShapeFromToFind();
                    int matches = worldKnowledge.CountKnownMatches(shape, color);
                    if (target.Contains("all"))
                    {
                        //string target = DataStore.GetStringValue("kirby:target").Split(' ')[1] + " " + DataStore.GetStringValue("kirby:target").Split(' ')[2];
                        if (matches == 0)
                        {
                            output = "I could not find any " + color + " " + shape + "s";
                        }
                        else if (matches == 1)
                        {
                            output = "I finished looking. There is " + matches + " " + color + " " + shape;
                        }
                        else
                        {
                            output = "I finished looking. There are " + matches + " " + color + " " + shape + "s";
                        }
                    }
                    else if (target.Contains("two"))
                    {
                        if (matches == 0)
                        {
                            output = "I looked everywhere, but I could not find " + target;
                        }
                        else if (matches == 1)
                        {
                            output = "I looked everywhere. I could only find one " + color + " " + shape;
                        }
                    }
                    else
                    {
                        output = "I could not find " + DataStore.GetStringValue("kirby:target");
                    }
                    DataStore.SetValue("kirby:patrol:finished", new DataStore.BoolValue(true), speech, string.Empty);
                    DataStore.SetStringValue("kirby:speech", new DataStore.StringValue(output), speech, string.Empty);
                    output = "";
                    DataStore.SetValue("kirby:isFinding", new DataStore.BoolValue(false), this, string.Empty);
                    worldKnowledge.toFind = "";
                    DataStore.SetValue("kirby:lookingForMore", new DataStore.BoolValue(false), this, string.Empty);
                    break;

                // Kirby goes to an object if he finds the target
                case "SUCCESS_GO_TO":
                    // Publish success message while still in FindingLooop
                    if (!DataStore.GetBoolValue("kirby:lookingForMore"))
                    {
                        output = "Here's " + DataStore.GetStringValue("kirby:target");
                        DataStore.SetStringValue("kirby:speech", new DataStore.StringValue(output), speech, string.Empty);
                        output = "";
                    }
                    // Then transition out of Finding, don't publish more feedback
                    DataStore.SetValue("kirby:isFinding", new DataStore.BoolValue(false), this, string.Empty);
                    DataStore.SetValue("kirby:lookingForMore", new DataStore.BoolValue(false), this, string.Empty);
                    DataStore.SetValue("kirby:disambiguating", new DataStore.BoolValue(false), this, string.Empty);
                    break;

                case "GO_TO":
                    if (DataStore.GetBoolValue("kirby:disambiguating"))
                    {
                        output = "Ok!";
                    }
                    break;

                // This won't work while patrolling, will work during go-to
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
                    DataStore.SetValue("kirby:isPatrolling", new DataStore.BoolValue(false), this, string.Empty);
                    break;

                case "PAUSED":
                    output = "Ok, I'll wait.";
                    break;

                case "QUEUE":
                    output = "I'm exploring.";
                    break;

                case "PLAN_LOOP":
                    output = "I'm planning where to explore next.";
                    break;

                case "FINISH_PATROL":
                    output = "I've explored as much as I can.";
                    DataStore.SetValue("kirby:patrol:finished", new DataStore.BoolValue(true), speech, string.Empty);
                    DataStore.SetStringValue("kirby:speech", new DataStore.StringValue(output), speech, string.Empty);
                    output = "";
                    DataStore.SetValue("kirby:isPatrolling", new DataStore.BoolValue(false), this, string.Empty);
                    break;
            }
        }
            if (!output.Equals(""))
        {
            DataStore.SetStringValue("kirby:speech", new DataStore.StringValue(output), speech, string.Empty);
        }
        

    }

}
