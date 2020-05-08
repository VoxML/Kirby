using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;
using VoxSimPlatform.Network;

public class RedisPublisher : RedisInterface
{
    RedisEventArgs lastEvent = null;

    MapUpdater mapUpdater;
    RoboUpdater roboUpdater;

    // keys
    public string mapKey;
    public string roboKey;
    public string fiducialKey;
    public string cmdKey;

    List<string> validReceiveCommands = new List<string>()
    {
        "get",
        "lpop"
    };

    // Start is called before the first frame update
    void Start()
    {
        mapUpdater = gameObject.GetComponent<MapUpdater>();
        roboUpdater = gameObject.GetComponent<RoboUpdater>();

        base.Start();

        redisSocket = (RedisSocket)commBridge.GetComponent<CommunicationsBridge>().FindSocketConnectionByLabel("RedisPublisher");

        if (redisSocket != null)
        {
            // try authentication
            if (!authenticated)
            {
                WriteCommand("auth ROSlab134");
            }
        }

        redisSocket.UpdateReceived += ReceivedMessage;
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
                        BroadcastMessage("PublisherAuthenticated", SendMessageOptions.DontRequireReceiver);
                        authenticated = true;
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

                    string requestKey = lastEvent.Content.Split()[1];

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
}
