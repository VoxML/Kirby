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

    public DialogueStateMachine stateMachine;

    // Use this for initialization
    void Start()
    {
        base.Start();

        stateMachine.scenarioController = gameObject.GetComponent<SingleAgentInteraction>();

        mapUpdater = GameObject.Find("KirbyManager").GetComponent<MapUpdater>();

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
}
