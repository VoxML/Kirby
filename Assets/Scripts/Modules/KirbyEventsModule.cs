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
        // TODO: eventManager.NonexistentEntityError += NameOfUnknownObjectEventHandler
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FIND(object[] args)
    {

    }

    // TODO: Add NameOfUnknownObjectEventHandler event handler
    // see example: https://docs.microsoft.com/en-us/dotnet/api/system.eventhandler?view=netcore-3.1
}
