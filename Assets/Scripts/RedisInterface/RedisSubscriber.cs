using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using VoxSimPlatform.Network;

public class RedisSubscriber : RedisInterface
{
    RedisPublisherManager manager;

    OutputDisplay outputDisplay;

    public string password { private get; set; }

    public RedisMessageType messageType;

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
            redisSocket.UseSizeHeader = UseSizeHeader;
            redisSocket.VerboseDebugOutput = VerboseDebugOutput;

            // try authentication
            if (!authenticated)
            {
                outputDisplay.SetText("Authenticating subscriber...", TextDisplayMode.Persistent);
                WriteAuthentication(string.Format("auth {0}", password));
            }

            redisSocket.UpdateReceived += ReceivedUpdate;
        }

        manager = gameObject.GetComponent<RedisPublisherManager>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SubscriberAuthenticated()
    {
        Debug.Log("RedisSubscriber: picked up message SubscriberAuthenticated");
        outputDisplay.SetText("Subscribing to notifications...");

        // get all keys defined in the publisher manager (all field names that end in "Key")
        List<string> keyNames = manager.GetType().GetFields().Where(f => f.Name.EndsWith("Key") &&
            (string)manager.GetType().GetField(f.Name).GetValue(manager) != string.Empty).Select(f => f.Name).ToList();

        // generate a single psubscribe commmand (psubscribe '__key*__:<ns>/<key1>' '__key*__:<ns>/<key2>'...)
        if (messageType == RedisMessageType.Array)
        {
            WriteArrayCommand(string.Format("psubscribe {0}", string.Format(string.Join(" ",
                keyNames.Select(k => string.Format("\'__key*__:{0}/{1}\'", manager.namespacePrefix,
                (string)manager.GetType().GetField(k).GetValue(manager)))))));
        }
        else if (messageType == RedisMessageType.BulkString)
        {
            WriteBulkStringCommand(string.Format("psubscribe {0}", string.Format(string.Join(" ",
                keyNames.Select(k => string.Format("\'__key*__:{0}/{1}\'", manager.namespacePrefix,
                (string)manager.GetType().GetField(k).GetValue(manager)))))));
        }
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
        List<string> blocks = new List<string>();

        string response = string.Empty;
        switch (type)
        {
            // simple strings
            case '+':
                response = raw.TrimStart('+').Trim();
                //response = raw.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)[1].TrimStart('+');
                Debug.Log(string.Format("RedisSubscriber: Got simple string update from Redis: {0}", response));

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
                    $29
                    __key*__:Kirby/Kirby_Feedback
                    $35
                    __keyspace@0__:Kirby/Kirby_Feedback
                    $5
                    rpush   <--

                    $8
                    pmessage
                    $29
                    __key*__:Kirby/Kirby_Feedback
                    $35
                    __keyspace@0__:Kirby/Kirby_Feedback
                    $4
                    lpop    <--
                    *4
                    $8
                    pmessage
                    $29
                    __key*__:Kirby/Kirby_Feedback
                    $35
                    __keyspace@0__:Kirby/Kirby_Feedback
                    $3
                    del     <--

                    $8
                    pmessage
                    $29
                    __key*__:Kirby/Kirby_Feedback
                    $35
                    __keyspace@0__:Kirby/Kirby_Feedback
                    $5
                    rpush   <--
                    *4
                    $8
                    pmessage
                    $19
                    __key*__:Kirby/Odom
                    $25
                    __keyspace@0__:Kirby/Odom
                    $3
                    set     <--
                    *4
                    $8
                    pmessage
                    $29
                    __key*__:Kirby/Kirby_Feedback
                    $35
                    __keyspace@0__:Kirby/Kirby_Feedback
                    $4
                    lpop    <--

                */


                blocks = raw.Split(new string[] { "*" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                foreach (string block in blocks)
                {
                    List<string> lines = block.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    string longKey = string.Empty;
                    string shortKey = string.Empty;
                    string cmd = string.Empty;

                    if (lines.FindIndex(l => l.StartsWith("psubscribe")) != -1)
                    {
                        if (!subscribed)
                        {
                            subscribed = true;
                            outputDisplay.SetText("Subscribed to notifications.");
                            BroadcastMessage("SubscribedToNotifications", SendMessageOptions.DontRequireReceiver);
                            return;
                        }
                    }
                    else if (lines.FindIndex(l => l.StartsWith("__keyspace")) != -1)
                    {
                        longKey = lines[lines.FindIndex(l => l.StartsWith("__keyspace"))].Split(':')[1];
                        shortKey = longKey.Replace(string.Format("{0}/", manager.namespacePrefix), string.Empty).Trim();
                        cmd = lines.Last();
                        Debug.Log(string.Format("RedisSubscriber: Got bulk string update from Redis: key: {0}, command: {1}", longKey, cmd));
                    }

                    // changes to resetKey might mean we have to start listening
                    if (processing || longKey == string.Format("{0}/{1}", manager.namespacePrefix, manager.resetKey))
                    {
                        if (!ignoreResetNotification)
                        {
                            if (longKey != string.Format("{0}/{1}", manager.namespacePrefix, manager.cmdKey))
                            {
                                if (usingRejson)
                                {
                                    switch (cmd)
                                    {
                                        case "set":
                                            manager.publishers[shortKey].WriteCommand(string.Format("json.get \"{0}\"", longKey));
                                            break;

                                        case "rpush":
                                            manager.publishers[shortKey].WriteCommand(string.Format("json.lpop \"{0}\"", longKey));
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
                                            manager.publishers[shortKey].WriteCommand(string.Format("get \"{0}\"", longKey));
                                            break;

                                        case "rpush":
                                            manager.publishers[shortKey].WriteCommand(string.Format("lpop \"{0}\"", longKey));
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
                }
                break;

            // arrays
            case '*':
                blocks = raw.Split(new string[] { "*" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                foreach (string block in blocks)
                {
                    List<string> lines = block.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    string longKey = string.Empty;
                    string shortKey = string.Empty;
                    string cmd = string.Empty;

                    if (lines.FindIndex(l => l.StartsWith("psubscribe")) != -1)
                    {
                        if (!subscribed)
                        {
                            subscribed = true;
                            outputDisplay.SetText("Subscribed to notifications.");
                            BroadcastMessage("SubscribedToNotifications", SendMessageOptions.DontRequireReceiver);
                            return;
                        }
                    }
                    else if (lines.FindIndex(l => l.StartsWith("__keyspace")) != -1)
                    {
                        longKey = lines[lines.FindIndex(l => l.StartsWith("__keyspace"))].Split(':')[1];
                        shortKey = longKey.Replace(string.Format("{0}/", manager.namespacePrefix), string.Empty).Trim();
                        cmd = lines.Last();
                        Debug.Log(string.Format("RedisSubscriber: Got bulk string update from Redis: key: {0}, command: {1}", longKey, cmd));
                    }

                    // changes to resetKey might mean we have to start listening
                    if (processing || longKey == string.Format("{0}/{1}", manager.namespacePrefix, manager.resetKey))
                    {
                        if (!ignoreResetNotification)
                        {
                            if (longKey != string.Format("{0}/{1}", manager.namespacePrefix, manager.cmdKey))
                            {
                                if (usingRejson)
                                {
                                    switch (cmd)
                                    {
                                        case "set":
                                            manager.publishers[shortKey].WriteCommand(string.Format("json.get \"{0}\"", longKey));
                                            break;

                                        case "rpush":
                                            manager.publishers[shortKey].WriteCommand(string.Format("json.lpop \"{0}\"", longKey));
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
                                            manager.publishers[shortKey].WriteCommand(string.Format("get \"{0}\"", longKey));
                                            break;

                                        case "rpush":
                                            manager.publishers[shortKey].WriteCommand(string.Format("lpop \"{0}\"", longKey));
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
                }
                break;

            default:
                break;
        }
    }
}
