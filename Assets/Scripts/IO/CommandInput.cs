using System;
using System.Collections.Generic;
using UnityEngine;

using VoxSimPlatform.Agent;

public class CommandInput : MonoBehaviour
{
    RedisPublisher redisPublisher;

    public InputController inputController;

    // Start is called before the first frame update
    void Start()
    {
        inputController = GameObject.Find("IOController").GetComponent<InputController>();
        if (inputController == null)
        {
            Debug.LogWarning("CommandInput.Start: Could not find InputController.  Expect errors!");
        }

        inputController.InputReceived += PostMessage;

        redisPublisher = gameObject.GetComponent<RedisPublisher>();
        if (redisPublisher == null)
        {
            Debug.LogWarning("CommandInput.Start: Could not find RedisPublisher.  Expect errors!");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void PostMessage(object sender, EventArgs e)
    {
        string message = ((InputEventArgs)e).InputString;

        if (message != DataStore.GetStringValue("user:speech")) {
            // take the input message, turn it into a Redis command and post it
            Debug.Log(string.Format("Got input \"{0}\"", message));

            string command = string.Format("rpush {0} \"{1}\"", string.Format("{0}/{1}",
                redisPublisher.namespacePrefix, redisPublisher.cmdKey), message);
            Debug.Log(string.Format("Posting message {0} to Redis", command));
            redisPublisher.WriteCommand(command);
        }
    }

    public void PostMessage(string message)
    {
        Debug.Log(string.Format("Got input \"{0}\"", message));

        string command = string.Format("rpush {0} \"{1}\"", string.Format("{0}/{1}",
                redisPublisher.namespacePrefix, redisPublisher.cmdKey), message);
        Debug.Log(string.Format("Posting message {0} to Redis", command));
        redisPublisher.WriteCommand(command);
    }
}