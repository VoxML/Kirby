using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;
using VoxSimPlatform.Network;

public class RedisPublisher : RedisInterface
{
    RedisEventArgs lastEvent = null;

    RedisSubscriber subscriber;

    MapUpdater mapUpdater;
    RoboUpdater roboUpdater;

    OutputDisplay outputDisplay;

    // keys
    public string mapKey;
    public string roboKey;
    public string fiducialKey;
    public string resetKey;
    public string cmdKey;

    List<string> validReceiveCommands = new List<string>()
    {
        "get",
        "lpop"
    };

    // Start is called before the first frame update
    void Start()
    {
        base.Start();

        //TODO: route this through VoxSim OutputController
        outputDisplay = GameObject.Find("OutputDisplay").GetComponent<OutputDisplay>();

        redisSocket = (RedisSocket)commBridge.GetComponent<CommunicationsBridge>().FindSocketConnectionByLabel("RedisPublisher");

        if (redisSocket != null)
        {
            // try authentication
            if (!authenticated)
            {
                outputDisplay.SetText("Authenticating publisher...", TextDisplayMode.Persistent);
                WriteCommand("auth ROSlab134");
            }
        }

        redisSocket.UpdateReceived += ReceivedMessage;

        subscriber = gameObject.GetComponent<RedisSubscriber>();

        mapUpdater = gameObject.GetComponent<MapUpdater>();
        roboUpdater = gameObject.GetComponent<RoboUpdater>();
    }

    // Update is called once per frame
    void Update()
    {
    /* 
        // to set set json val
        JsonKeyValue jsonObj = new JsonKeyValue();
        jsonObj.key = "value";

        string json = JsonUtility.ToJson(jsonObj);

        if (usingRejson)
        {
            WriteCommand(string.Format("json.set {0} . '{1}'", mapKey, json));
        }
        else
        {
            WriteCommand(string.Format("set {0} '{1}'", mapKey, json));
        }
        
        // to get json val
        if (usingRejson)
        {
            WriteCommand(string.Format("json.get {0}", mapKey));
        }
        else
        {
            WriteCommand(string.Format("get {0}", mapKey));
        }
    */
    }

    public void SubscriberAuthenticated()
    {
        Debug.Log("RedisPublisher: picked up message SubscriberAuthenticated");

        ResetBridge();
    }

    public void WriteCommand(string messageToSend)
    {
        if (lastEvent == null)
        {
            lastEvent = new RedisEventArgs(RedisEventType.Command, messageToSend);
        }

        base.WriteCommand(messageToSend);
    }

    public void ReceivedMessage(object sender, EventArgs e)
    {
        string raw = ((RedisEventArgs)e).Content;
        char type = GetResponseType(((RedisEventArgs)e).Content);
        string requestKey = string.Empty;

        if (lastEvent != null)
        {
            Debug.Log(string.Format("RedisPublisher: Received response to: {0}", lastEvent.Content));
        }

        string response = string.Empty;
        switch (type)
        {
            // simple strings
            case '+':
                response = raw.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)[1].TrimStart('+');
                Debug.Log(string.Format("RedisPublisher: Got simple string response from Redis: {0}", response));

                if (!authenticated)
                {
                    if (string.Equals(response, "OK"))
                    {
                        authenticated = true;
                        outputDisplay.SetText("Publisher authenticated.");
                        BroadcastMessage("PublisherAuthenticated", SendMessageOptions.DontRequireReceiver);
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
                string size = raw.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)[1].TrimStart('$');

                if (raw.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Length > 2)
                {
                    if (!IsValidResponseEvent(lastEvent))
                    {
                        return;
                    }

                    response = raw.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)[2];
                    Debug.Log(string.Format("RedisPublisher: Got bulk string response from Redis (responding to \"{0}\", size {1}): {2}",
                        lastEvent.Content, size, response));

                    requestKey = lastEvent.Content.Split()[1];

                    if (!string.IsNullOrEmpty(mapKey) && (requestKey == mapKey))
                    {
                        MapUpdate mapUpdate = JsonConvert.DeserializeObject<MapUpdate>(response);

                        if (MapUpdate.Validate(mapUpdate))
                        {
                            mapUpdate.Log();
                            mapUpdater.UpdateMap(mapUpdate);
                        }
                    }
                    else if (!string.IsNullOrEmpty(roboKey) && (requestKey == roboKey))
                    {
                        RoboUpdate roboUpdate = JsonConvert.DeserializeObject<RoboUpdate>(response);

                        if (RoboUpdate.Validate(roboUpdate))
                        {
                            roboUpdate.Log();
                            roboUpdater.UpdateRobot(roboUpdate);
                        }
                    }
                    else if (!string.IsNullOrEmpty(fiducialKey) && (requestKey == fiducialKey))
                    {
                    }
                    else if (!string.IsNullOrEmpty(resetKey) && (requestKey == resetKey))
                    {
                        if (Convert.ToInt32(response) == 0)
                        {
                            outputDisplay.SetText("Databased flushed.");
                            BroadcastMessage("DatabaseFlushed", SendMessageOptions.DontRequireReceiver);
                        }
                    }
                    else if (!string.IsNullOrEmpty(cmdKey) && (requestKey == cmdKey))
                    {
                    }
                }

                break;

            // arrays
            case '*':
                break;

            default:
                break;
        }

        lastEvent = null;
    }

    bool IsValidResponseEvent(RedisEventArgs ev)
    {
        bool valid = true;

        valid &= ((ev != null) && (ev.Type == RedisEventType.Command));

        if (valid)
        {
            if (usingRejson)
            {
                valid &= validReceiveCommands.Any(f => ev.Content.StartsWith("json." + f));
            }
            else
            {
                valid &= validReceiveCommands.Any(f => ev.Content.StartsWith(f));
            }
        }

        return valid;
    }

    public void ResetBridge()
    {
        outputDisplay.SetText("Flushing database...", TextDisplayMode.Persistent);

        // halt subscriber processing while bridge is reset
        subscriber.processing = false;
        WriteCommand(string.Format("set {0} 1", resetKey));
    }
}
