using UnityEngine;
using System;
using System.Linq;
using System.Text;

using Newtonsoft.Json;
using VoxSimPlatform.Network;

public class RedisInterface : MonoBehaviour
{
    GameObject commBridge;
    RedisSocket redisSocket;

    bool authenticated = false;

    RedisEventArgs lastEvent = null;

    /* KEYS */
    public string mapKey;
    public string roboKey;
    public string fiducialKey;
    public string cmdKey;

    public bool usingRejson = false;

    // Start is called before the first frame update
    void Start()
    {
        commBridge = GameObject.Find("CommunicationsBridge");
        redisSocket = (RedisSocket)commBridge.GetComponent<CommunicationsBridge>().FindSocketConnectionByLabel("Redis");

        if (redisSocket != null)
        {
            redisSocket.UpdateReceived += ReceivedResponse;

            // try authentication
            if (!authenticated)
            {
                WriteCommand("auth ROSlab134");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
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
            WriteCommand(string.Format("get {0}",cmdKey));
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

        string bulkString = string.Format("${0}\r\n{1}\r\n", messageToSend.Length.ToString(), messageToSend);
        Debug.Log(string.Format("Writing bulk string command \"{0}\" to Redis", bulkString.Replace("\r","\\r").Replace("\n", "\\n")));
        byte[] bytes = Encoding.ASCII.GetBytes(bulkString).ToArray<byte>();
        redisSocket.Write(bytes);
    }

    void ReceivedResponse(object sender, EventArgs e)
    {
        
        if (lastEvent != null)
        {
            if (lastEvent.Type == RedisEventType.Command)
            { 
                Debug.Log(string.Format("Received response to: {0}", lastEvent.Content));
            }
        }

        string raw = ((RedisEventArgs)e).Content;
        Debug.Log(string.Format("Raw response from Redis: {0}", raw));

        // split on new lines
        // first line is "unknown command" response -- throw it out
        // type is first char of second line
        char type = raw.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)[1][0];
        string response = string.Empty;

        switch (type)
        {
            // simple strings
            case '+':
                response = raw.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)[1].TrimStart('+');
                Debug.Log(string.Format("Got simple string response from Redis: {0}", response));

                if (!authenticated)
                {
                    if (string.Equals(response,"OK"))
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
                    Debug.Log(string.Format("Got bulk string response from Redis (responding to \"{0}\", size {1}): {2}",
                        lastEvent.Content, size, response));

                    if (lastEvent.Content.StartsWith("get"))
                    {
                        string key = lastEvent.Content.Split()[1];

                        if (!string.IsNullOrEmpty(mapKey) && (key == mapKey))
                        {
                            MapUpdate update = JsonConvert.DeserializeObject<MapUpdate>(response);
                            Debug.Log(string.Format("Value of \"id\" in jsonObj = {0}", update.id));
                            Debug.Log(string.Format("Value of \"width\" in jsonObj = {0}", update.width));
                            Debug.Log(string.Format("Value of \"height\" in jsonObj = {0}", update.height));
                            Debug.Log(string.Format("Value of \"resolution\" in jsonObj = {0}", update.resolution));
                            Debug.Log(string.Format("Value of \"data\" in jsonObj = {0}", string.Format("[{0}]", string.Join(",",
                                update.data.Select(l => string.Format("[{0}]", string.Join(",", l.Select(ll => ll.ToString()))))))));
                        }
                        else if (!string.IsNullOrEmpty(roboKey) && (key == roboKey))
                        {
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
