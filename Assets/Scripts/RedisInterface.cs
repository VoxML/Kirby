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

    bool sentAuthentication = false;

    // Start is called before the first frame update
    void Start()
    {
        commBridge = GameObject.Find("CommunicationsBridge");
        redisSocket = (RedisSocket)commBridge.GetComponent<CommunicationsBridge>().FindSocketConnectionByLabel("Redis");

        if (redisSocket != null)
        {
            redisSocket.UpdateReceived += ReceivedResponse;

            // try authentication
            if (!sentAuthentication)
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
            WriteCommand("get foo");
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            // set json val
            SampleJson jsonObj = new SampleJson();
            jsonObj.key = "value";

            string json = JsonUtility.ToJson(jsonObj);
            WriteCommand(string.Format("json.set json . '{0}'",json));
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            // get json val
            WriteCommand("json.get json");
        }
    }

    void WriteCommand(string messageToSend)
    {
        string bulkString = string.Format("${0}\r\n{1}\r\n", messageToSend.Length.ToString(), messageToSend);
        Debug.Log(string.Format("Writing bulk string command \"{0}\" to Redis", bulkString.Replace("\r","\\r").Replace("\n", "\\n")));
        byte[] bytes = Encoding.ASCII.GetBytes(bulkString).ToArray<byte>();
        redisSocket.Write(bytes);
    }

    void ReceivedResponse(object sender, EventArgs e)
    {
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
                response = raw.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)[2];
                Debug.Log(string.Format("Got bulk string response of size {0} from Redis: {1}", size, response));
                MapUpdate update = JsonConvert.DeserializeObject<MapUpdate>(response);
                //object json = JsonUtility.FromJson(response,typeof(JsonUpdate));
                //JsonUpdate update = (JsonUpdate)json;
                Debug.Log(string.Format("Value of \"id\" in jsonObj = {0}", update.id));
                Debug.Log(string.Format("Value of \"width\" in jsonObj = {0}", update.width));
                Debug.Log(string.Format("Value of \"height\" in jsonObj = {0}", update.height));
                Debug.Log(string.Format("Value of \"resolution\" in jsonObj = {0}", update.resolution));
                Debug.Log(string.Format("Value of \"data\" in jsonObj = {0}", string.Format("[{0}]", string.Join(",", 
                    update.data.Select(l => string.Format("[{0}]", string.Join(",", l.Select(ll => ll.ToString()))))))));

                    //string.Join(",", 
                    //string.Format("[{0}]",update.data.Select(l => string.Join(",",l.Select(ll => ll.ToString())))).ToString())));
                break;

            // arrays
            case '*':
                break;

            default:
                break;
        }
    }
}
