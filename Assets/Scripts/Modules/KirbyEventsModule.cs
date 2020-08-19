using UnityEngine;
using System.Collections;

using VoxSimPlatform.Core;

public class KirbyEventsModule : ModuleBase
{
    EventManager eventManager;

    // Use this for initialization
    void Start()
    {
        // TODO: get game object named "BehaviorController"
        // TODO: set eventManager = EventManager component on BehaviorController
        // TODO: create a DataStore subscriber for the key "user:event:intent" to trigger "PromptEvent"
        //  (check out DialogueInteractionModule for an example of how to subscribe to keys in the DataStore
        // TODO: eventManager.NonexistentEntityError += NameOfUnknownObjectEventHandler
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PromptEvent(string key, DataStore.IValue value)
    {
        // TODO: get the value of key and store it in a string variable
        //  (check out KirbySpeechModule.Speak to see an example of getting a key value)

        // TODO:
        //  if that variable is not null of empty
        //  eventManager.InsertEvent(string.Empty, 0)
        //  eventManager.InsertEvent(<your event string variable>, 1)
    }

    void FIND(object[] args)
    {

    }

    // TODO: Add NameOfUnknownObjectEventHandler event handler
    // see example: https://docs.microsoft.com/en-us/dotnet/api/system.eventhandler?view=netcore-3.1
}
