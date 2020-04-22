using System;
using System.Collections.Generic;
using UnityEngine;

using VoxSimPlatform.Agent;

public class CommandInput : MonoBehaviour
{
    InputController inputController;
    RedisInterface redis;

    // Start is called before the first frame update
    void Start()
    {
        inputController = GameObject.Find("IOController").GetComponent<InputController>();
        if (inputController == null)
        {
            Debug.LogError("Could not find InputController!");
        }

        //inputController.directToEventManager must be false
        //if (inputController.directToEventManager)
        //{ 
        //    inputController.directToEventManager = false;
        //}

        inputController.InputReceived += PostMessage;

        redis = gameObject.GetComponent<RedisInterface>();
        if (inputController == null)
        {
            Debug.LogError("Could not find RedisInterface!");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void PostMessage(object sender, EventArgs e)
    {
        string message = ((InputEventArgs)e).InputString;
        Debug.Log(string.Format("Got input \"{0}\"", message));

        string command = string.Format("set cmd \"'{0}'\"", message);
        Debug.Log(string.Format("Posting message {0} to Redis", command));
        redis.WriteCommand(command);
    }
}