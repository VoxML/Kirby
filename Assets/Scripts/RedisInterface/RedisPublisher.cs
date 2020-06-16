using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;
using VoxSimPlatform.Network;

public class RedisPublisher : RedisInterface
{
    RedisEventArgs lastEvent = null;

    RedisPublisherManager manager;
    RedisSubscriber subscriber;

    MapUpdater mapUpdater;
    RoboUpdater roboUpdater;
    FiducialUpdater fidUpdater;
    LogUpdater logUpdater;

    OutputDisplay outputDisplay;
    
    public string publisherKey;

    List<string> validReceiveCommands = new List<string>()
    {
        "get",
        "lpop"
    };

    // Start is called before the first frame update
    void Start()
    {
        base.Start();

        Debug.Log(string.Format("Creating {0}RedisPublisher", publisherKey));

        //TODO: route this through VoxSim OutputController
        outputDisplay = GameObject.Find("OutputDisplay").GetComponent<OutputDisplay>();

        redisSocket = (RedisSocket)commBridge.GetComponent<CommunicationsBridge>().
            FindSocketConnectionByLabel("RedisPublisher");

        string address = redisSocket.Address;
        int port = redisSocket.Port;

        // close and reopen to get dedicated connecton
        if (redisSocket != null)
        {
            //commBridge.GetComponent<CommunicationsBridge>().SocketConnections.RemoveAt(
            //    commBridge.GetComponent<CommunicationsBridge>().SocketConnections.IndexOf(redisSocket));
            //redisSocket.Close();
        }

        redisSocket = (RedisSocket)commBridge.GetComponent<CommunicationsBridge>().
            ConnectSocket(address, port, typeof(RedisSocket));
        redisSocket.Label = string.Format("{0}RedisPublisher", publisherKey);
        commBridge.GetComponent<CommunicationsBridge>().SocketConnections.Add(redisSocket);

        // add socket's IOClientType component to CommunicationsBridge
        commBridge.AddComponent(redisSocket.IOClientType);

        if (redisSocket != null)
        {
            redisSocket.UpdateReceived += ReceivedMessage;
        }

        manager = gameObject.GetComponent<RedisPublisherManager>();
        subscriber = gameObject.GetComponent<RedisSubscriber>();

        mapUpdater = gameObject.GetComponent<MapUpdater>();
        roboUpdater = gameObject.GetComponent<RoboUpdater>();
        fidUpdater = gameObject.GetComponent<FiducialUpdater>();
        logUpdater = gameObject.GetComponent<LogUpdater>();

        if (!authenticated)
        {
            WriteCommand("auth ROSlab134");
        }
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
        string requestKey = string.Empty;

        if (lastEvent != null)
        {
            Debug.Log(string.Format("RedisPublisher({0}): Received response to: {1}", publisherKey, lastEvent.Content));
        }

        string response = string.Empty;
        switch (type)
        {
            // simple strings
            case '+':
                response = raw.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)[1].TrimStart('+');
                Debug.Log(string.Format("RedisPublisher({0}): Got simple string response from Redis: {1}", publisherKey, response));

                if (!authenticated)
                {
                    if (string.Equals(response, "OK"))
                    {
                        authenticated = true;
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
                    Debug.Log(string.Format("RedisPublisher({0}): Got bulk string response from Redis (responding to \"{1}\", size {2}): {3}",
                        publisherKey, lastEvent.Content, size, response));

                    requestKey = lastEvent.Content.Split()[1];

                    if (!string.IsNullOrEmpty(manager.mapKey) && (requestKey == string.Format("{0}/{1}", manager.namespacePrefix, manager.mapKey)))
                    {
                        MapUpdate mapUpdate = JsonConvert.DeserializeObject<MapUpdate>(response);

                        if (MapUpdate.Validate(mapUpdate))
                        {
                            mapUpdate.Log();
                            mapUpdater.UpdateMap(mapUpdate);
                        }
                    }
                    else if (!string.IsNullOrEmpty(manager.logKey) && requestKey == string.Format("{0}/{1}", manager.namespacePrefix, manager.logKey))
                    {
                        LogUpdate logUpdate = JsonConvert.DeserializeObject<LogUpdate>(response);
                        //Debug.Log("there is a logUpdate");
                        if (LogUpdate.Validate(logUpdate))
                        {
                            logUpdate.Log();
                            //Debug.Log("logUpdate.Log()");
                            logUpdater.UpdateLog(logUpdate);
                            //Debug.Log("logUpdater.UpdateLog(logUpdate)");

                        }
                        //Debug.Log("it's invalidLogUpdate");
                    }
                    else if (!string.IsNullOrEmpty(manager.roboKey) && (requestKey == string.Format("{0}/{1}", manager.namespacePrefix, manager.roboKey)))
                    {
                        RoboUpdate roboUpdate = JsonConvert.DeserializeObject<RoboUpdate>(response);

                        if (RoboUpdate.Validate(roboUpdate))
                        {
                            roboUpdate.Log();
                            roboUpdater.UpdateRobot(roboUpdate);
                        }
                    }
                    else if (!string.IsNullOrEmpty(manager.fiducialKey) && (requestKey == string.Format("{0}/{1}", manager.namespacePrefix, manager.fiducialKey)))
                    {
                        FiducialUpdate fidUpdate = JsonConvert.DeserializeObject<FiducialUpdate>(response);

                        if (FiducialUpdate.Validate(fidUpdate))
                        {
                            fidUpdate.Log();
                            fidUpdater.UpdateFiducial(fidUpdate);
                        }
                    }
                    else if (!string.IsNullOrEmpty(manager.resetKey) && (requestKey == string.Format("{0}/{1}", manager.namespacePrefix, manager.resetKey)))
                    {
                        if (Convert.ToInt32(response) == 0)
                        {
                            outputDisplay.SetText("Databased flushed.");
                            BroadcastMessage("DatabaseFlushed", SendMessageOptions.DontRequireReceiver);
                        }
                    }
                    else if (!string.IsNullOrEmpty(manager.cmdKey) && (requestKey == string.Format("{0}/{1}", manager.namespacePrefix, manager.cmdKey)))
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
