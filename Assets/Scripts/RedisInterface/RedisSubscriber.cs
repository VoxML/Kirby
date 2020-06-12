using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using VoxSimPlatform.Network;

public class RedisSubscriber : RedisInterface
{
    RedisPublisher publisher;

    OutputDisplay outputDisplay;

    public bool subscribed = false;

    public bool ignoreResetNotification = true;

    public bool processing = false;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();

        //TODO: route this through VoxSim OutputController
        outputDisplay = GameObject.Find("OutputDisplay").GetComponent<OutputDisplay>();

        redisSocket = (RedisSocket)commBridge.GetComponent<CommunicationsBridge>().FindSocketConnectionByLabel("RedisSubscriber");

        if (redisSocket != null)
        {
            // try authentication
            if (!authenticated)
            {
                outputDisplay.SetText("Authenticating subscriber...", TextDisplayMode.Persistent);
                WriteCommand("auth ROSlab134");
            }

            redisSocket.UpdateReceived += ReceivedUpdate;
        }

        publisher = gameObject.GetComponent<RedisPublisher>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SubscriberAuthenticated()
    {
        Debug.Log("RedisSubscriber: picked up message SubscriberAuthenticated");
        outputDisplay.SetText("Subscribing to notifications...");

        // get all keys defined in the publisher (all field names that end in "Key")
        List<string> keyNames = publisher.GetType().GetFields().Where(f => f.Name.EndsWith("Key")).Select(f => f.Name).ToList();

        // generate a single psubscribe commmand (psubscribe '__key*__:<ns>/<key1>' '__key*__:<ns>/<key2>'...)
        WriteCommand(string.Format("psubscribe {0}", string.Format(string.Join(" ",
            keyNames.Select(k => string.Format("\'__key*__:{0}/{1}\'", publisher.namespacePrefix,
            (string)publisher.GetType().GetField(k).GetValue(publisher)))))));
    }

    public void DatabaseFlushed()
    {
        Debug.Log("RedisSubscriber: picked up message DatabaseFlushed");
        processing = true;
    }

    public void ReceivedUpdate(object sender, EventArgs e)
    {
        string raw = ((RedisEventArgs)e).Content;
        char type = GetResponseType(((RedisEventArgs)e).Content);

        string response = string.Empty;
        switch (type)
        {
            // simple strings
            case '+':
                response = raw.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)[1].TrimStart('+');
                Debug.Log(string.Format("RedisSubscriber: Got simple string response from Redis: {0}", response));

                if (!authenticated)
                {
                    if (string.Equals(response, "OK"))
                    {
                        authenticated = true;
                        outputDisplay.SetText("Subscriber authenticated.");
                        BroadcastMessage("SubscriberAuthenticated", SendMessageOptions.DontRequireReceiver);
                    }
                }
                break;

            // errors
            case '-':
                break;

            // integers
            case ':':
                break;

            // bulk strings
            case '$':
                /*
                    $8
                    pmessage
                    $10
                    __key* __:*
                    $19
                    __keyspace@0__:json
                    $3
                    set
                    *4
                    $8
                    pmessage
                    $10
                    __key* __:*
                    $18
                    __keyevent@0__:set
                    $4
                    json
                */
                List<string> lines = raw.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                string key = string.Empty;
                string cmd = string.Empty;

                if (lines.FindIndex(l => l.StartsWith("__keyspace")) != -1)
                {
                    key = lines[lines.FindIndex(l => l.StartsWith("__keyspace"))].Split(':')[1];
                    cmd = lines.Last();
                    Debug.Log(string.Format("RedisSubscriber: Got bulk string response from Redis: key: {0}, command: {1}", key, cmd));
                }

                // changes to resetKey might mean we have to start listening
                if (processing || key == string.Format("{0}/{1}", publisher.namespacePrefix, publisher.resetKey))
                {
                    if (!ignoreResetNotification)
                    {
                        if (key != string.Format("{0}/{1}", publisher.namespacePrefix, publisher.cmdKey))
                        {
                            if (usingRejson)
                            {
                                switch (cmd)
                                {
                                    case "set":
                                        publisher.WriteCommand(string.Format("json.get {0}", key));
                                        break;

                                    case "rpush":
                                        publisher.WriteCommand(string.Format("json.lpop {0}", key));
                                        break;

                                    default:
                                        break;
                                }
                            }
                            else
                            {
                                switch (cmd)
                                {
                                    case "set":
                                        publisher.WriteCommand(string.Format("get {0}", key));
                                        break;

                                    case "rpush":
                                        publisher.WriteCommand(string.Format("lpop {0}", key));
                                        break;

                                    default:
                                        break;
                                }
                            }
                        }
                    }
                    else
                    {
                        ignoreResetNotification = false;
                    }
                }
                break;

            // arrays
            case '*':
                if (!subscribed)
                {
                    subscribed = true;
                    outputDisplay.SetText("Subscribed to notifications.");
                    BroadcastMessage("SubscribedToNotifications", SendMessageOptions.DontRequireReceiver);
                }
                break;

            default:
                break;
        }
    }
}
