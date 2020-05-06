using UnityEngine;
using System;

using Newtonsoft.Json;
using VoxSimPlatform.Network;

public class RedisPublisher : RedisInterface
{
    RedisEventArgs lastEvent = null;

    MapUpdater mapUpdater;
    RoboUpdater roboUpdater;

    bool initedMap = false;

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

        redisSocket.UpdateReceived += ReceivedResponse;
    }

    // Update is called once per frame
    void Update()
    {
        if (authenticated && !initedMap)
        {
            if (usingRejson)
            {
                WriteCommand(string.Format("json.lpop {0}", mapKey));
            }
            else
            {
                WriteCommand(string.Format("lpop {0}", mapKey));
            }

            initedMap = true;
        }

        // below are tests that set preset commands to JSON
        // TODO: remove when no longer necessary

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            // ping
            WriteCommand("ping");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            // set foo bar
            WriteCommand("set foo bar");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            // get foo
            WriteCommand(string.Format("get {0}", cmdKey));
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            // set json val
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
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            // get json val
            if (usingRejson)
            {
                WriteCommand(string.Format("json.get {0}", mapKey));
            }
            else
            {
                WriteCommand(string.Format("get {0}", mapKey));
            }
        }
    }

    public void WriteCommand(string messageToSend)
    {
        if (lastEvent == null)
        {
            lastEvent = new RedisEventArgs(RedisEventType.Command, messageToSend);
        }

        base.WriteCommand(messageToSend);
    }

    public void ReceivedResponse(object sender, EventArgs e)
    {
        if (lastEvent != null)
        {
            if (lastEvent.Type == RedisEventType.Command)
            {
                Debug.Log(string.Format("RedisPublisher: Received response to: {0}", lastEvent.Content));
            }
        }

        string raw = ((RedisEventArgs)e).Content;
        char type = GetResponseType(((RedisEventArgs)e).Content);

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
                    response = raw.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)[2];
                    Debug.Log(string.Format("RedisPublisher: Got bulk string response from Redis (responding to \"{0}\", size {1}): {2}",
                        lastEvent.Content, size, response));

                    if (lastEvent.Content.StartsWith("get") || lastEvent.Content.StartsWith("lpop"))
                    {
                        string key = lastEvent.Content.Split()[1];

                        if (!string.IsNullOrEmpty(mapKey) && (key == mapKey))
                        {
                            MapUpdate mapUpdate = JsonConvert.DeserializeObject<MapUpdate>(response);
                            mapUpdate.Log();
                            mapUpdater.UpdateMap(mapUpdate);
                        }
                        else if (!string.IsNullOrEmpty(roboKey) && (key == roboKey))
                        {
                            RoboUpdate roboUpdate = JsonConvert.DeserializeObject<RoboUpdate>(response);
                            roboUpdate.Log();
                            roboUpdater.UpdateRobot(roboUpdate);
                        }
                        else if (!string.IsNullOrEmpty(fiducialKey) && (key == fiducialKey))
                        {
                        }
                        else if (!string.IsNullOrEmpty(cmdKey) && (key == cmdKey))
                        {
                        }
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
}
