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

    public string password { private get; set; }

    public RedisMessageType messageType;

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
        
        redisSocket = (RedisSocket)commBridge.GetComponent<CommunicationsBridge>().
            ConnectSocket(address, port, typeof(RedisSocket));
        redisSocket.Label = string.Format("{0}RedisPublisher", publisherKey);
        redisSocket.UseSizeHeader = UseSizeHeader;
        redisSocket.VerboseDebugOutput = VerboseDebugOutput;
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
            WriteAuthentication(string.Format("auth {0}", password));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            WriteCommand("set foo \"bar\"");
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            WriteCommand("get \"foo\"");
        }
    }

    public void WriteCommand(string messageToSend)
    {
        if (messageType == RedisMessageType.Array)
        {
            WriteArrayCommand(messageToSend);
        }
        else if (messageType == RedisMessageType.BulkString)
        {
            WriteBulkStringCommand(messageToSend);
        }
    }

    public void WriteArrayCommand(string messageToSend)
    {
        //if (lastEvent == null)
        //{
        lastEvent = new RedisEventArgs(RedisEventType.Command, messageToSend);
        //}

        base.WriteArrayCommand(messageToSend);
    }

    public void WriteBulkStringCommand(string messageToSend)
    {
        //if (lastEvent == null)
        //{
        lastEvent = new RedisEventArgs(RedisEventType.Command, messageToSend);
        //}

        base.WriteBulkStringCommand(messageToSend);
    }

    public void ReceivedMessage(object sender, EventArgs e)
    {
        string raw = ((RedisEventArgs)e).Content;
        char type = GetResponseType(raw);
        string requestKey = string.Empty;

        if (lastEvent != null)
        {
            Debug.Log(string.Format("RedisPublisher({0}): Received response to \"{1}\": {2}", publisherKey, lastEvent.Content, raw));
        }

        string response = string.Empty;
        switch (type)
        {
            // simple strings
            case '+':
                response = raw.TrimStart('+').Trim();
                //response = raw.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)[1].TrimStart('+');
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
                response = raw.TrimStart(':').Trim();
                Debug.Log(string.Format("RedisPublisher({0}): Got integer response from Redis: {1}", publisherKey, response));
                break;

            // bulk strings
            case '$':
                string size = raw.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)[0].TrimStart('$');

                if (raw.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Length > 1)
                {
                    if (!IsValidResponseEvent(lastEvent))
                    {
                        return;
                    }

                    response = raw.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)[1];
                    Debug.Log(string.Format("RedisPublisher({0}): Got bulk string response from Redis (responding to \"{1}\", size {2}): {3}",
                        publisherKey, lastEvent.Content, size, response));

                    requestKey = lastEvent.Content.Split()[1].Replace("\"",string.Empty);

                    if (!string.IsNullOrEmpty(manager.mapKey) && (requestKey == string.Format("{0}/{1}", manager.namespacePrefix, manager.mapKey)))
                    {
                        try
                        {
                            MapUpdate mapUpdate = JsonConvert.DeserializeObject<MapUpdate>(response);

                            if (MapUpdate.Validate(mapUpdate))
                            {
                                mapUpdate.Log();
                                mapUpdater.UpdateMap(mapUpdate);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarningFormat("RedisPublisher.ReceivedMessage: {0}", ex.Message);
                        }
                    }
                    else if (!string.IsNullOrEmpty(manager.logKey) && requestKey == string.Format("{0}/{1}", manager.namespacePrefix, manager.logKey))
                    {
                        try
                        {
                            LogUpdate logUpdate = JsonConvert.DeserializeObject<LogUpdate>(response);
                            if (LogUpdate.Validate(logUpdate))
                            {
                                logUpdate.Log();
                                logUpdater.UpdateLog(logUpdate);

                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarningFormat("RedisPublisher.ReceivedMessage: {0}", ex.Message);
                        }
                    }
                    else if (!string.IsNullOrEmpty(manager.roboKey) && (requestKey == string.Format("{0}/{1}", manager.namespacePrefix, manager.roboKey)))
                    {
                        try
                        {
                            RoboUpdate roboUpdate = JsonConvert.DeserializeObject<RoboUpdate>(response);
                            if (RoboUpdate.Validate(roboUpdate))
                            {
                                roboUpdate.Log();
                                roboUpdater.UpdateRobot(roboUpdate);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarningFormat("RedisPublisher.ReceivedMessage: {0}", ex.Message);
                        }
                    }
                    else if (!string.IsNullOrEmpty(manager.fiducialKey) && (requestKey == string.Format("{0}/{1}", manager.namespacePrefix, manager.fiducialKey)))
                    {
                        try
                        {
                            FiducialUpdate fidUpdate = JsonConvert.DeserializeObject<FiducialUpdate>(response);
                            if (FiducialUpdate.Validate(fidUpdate))
                            {
                                fidUpdate.Log();
                                fidUpdater.UpdateFiducial(fidUpdate);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarningFormat("RedisPublisher.ReceivedMessage: {0}", ex.Message);
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
                Debug.Log(string.Format("Untyped response: lastEvent = {0}", lastEvent.Content));

                requestKey = lastEvent.Content.Split()[1];

                if (!IsValidResponseEvent(lastEvent))
                {
                    return;
                }

                if (!string.IsNullOrEmpty(manager.mapKey) && (requestKey == string.Format("{0}/{1}", manager.namespacePrefix, manager.mapKey)))
                {
                    // Map is rpush (a list)
                    if (raw.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Length > 1)
                    {
                        response = raw.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)[1];
                        Debug.Log(string.Format("RedisPublisher({0}): Got untyped response from Redis (responding to \"{1}\"): {2}",
                            publisherKey, lastEvent.Content, response));

                        MapUpdate mapUpdate = JsonConvert.DeserializeObject<MapUpdate>(response);

                        if (MapUpdate.Validate(mapUpdate))
                        {
                            mapUpdate.Log();
                            mapUpdater.UpdateMap(mapUpdate);
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(manager.logKey) && requestKey == string.Format("{0}/{1}", manager.namespacePrefix, manager.logKey))
                {
                    // Kirby_Feedback is rpush (a list)
                    if (raw.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Length > 1)
                    {
                        response = raw.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)[1];
                        Debug.Log(string.Format("RedisPublisher({0}): Got untyped response from Redis (responding to \"{1}\"): {2}",
                            publisherKey, lastEvent.Content, response));

                        LogUpdate logUpdate = JsonConvert.DeserializeObject<LogUpdate>(response);
                        if (LogUpdate.Validate(logUpdate))
                        {
                            logUpdate.Log();
                            logUpdater.UpdateLog(logUpdate);

                        }
                    }
                }
                else if (!string.IsNullOrEmpty(manager.roboKey) && (requestKey == string.Format("{0}/{1}", manager.namespacePrefix, manager.roboKey)))
                {
                    // Odom is set
                    response = raw.Trim();
                    Debug.Log(string.Format("RedisPublisher({0}): Got untyped response from Redis (responding to \"{1}\"): {2}",
                        publisherKey, lastEvent.Content, response));

                    RoboUpdate roboUpdate;

                    try
                    {
                        roboUpdate = JsonConvert.DeserializeObject<RoboUpdate>(response);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarningFormat(ex.Message);
                        return;
                    }

                    if (RoboUpdate.Validate(roboUpdate))
                    {
                        roboUpdate.Log();
                        roboUpdater.UpdateRobot(roboUpdate);
                    }
                }
                else if (!string.IsNullOrEmpty(manager.fiducialKey) && (requestKey == string.Format("{0}/{1}", manager.namespacePrefix, manager.fiducialKey)))
                {
                    // Fiducials is set
                    response = raw.Trim();
                    Debug.Log(string.Format("RedisPublisher({0}): Got untyped response from Redis (responding to \"{1}\"): {2}",
                        publisherKey, lastEvent.Content, response));
                        
                    FiducialUpdate fidUpdate;

                    try
                    {
                        fidUpdate = JsonConvert.DeserializeObject<FiducialUpdate>(response);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarningFormat(ex.Message);
                        return;
                    }

                    if (FiducialUpdate.Validate(fidUpdate))
                    {
                        fidUpdate.Log();
                        fidUpdater.UpdateFiducial(fidUpdate);
                    }
                }
                else if (!string.IsNullOrEmpty(manager.resetKey) && (requestKey == string.Format("{0}/{1}", manager.namespacePrefix, manager.resetKey)))
                {
                    // Bridge_Reset is set
                    response = raw.Trim();
                    Debug.Log(string.Format("RedisPublisher({0}): Got untyped response from Redis (responding to \"{1}\"): {2}",
                        publisherKey, lastEvent.Content, response));

                    try
                    {
                        if (Convert.ToInt32(response) == 0)
                        {
                            outputDisplay.SetText("Databased flushed.");
                            BroadcastMessage("DatabaseFlushed", SendMessageOptions.DontRequireReceiver);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarningFormat(ex.Message);
                    }
                    
                }
                else if (!string.IsNullOrEmpty(manager.cmdKey) && (requestKey == string.Format("{0}/{1}", manager.namespacePrefix, manager.cmdKey)))
                {
                }

                break;
        }

        //lastEvent = null;
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
