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

    // Start is called before the first frame update
    void Start()
    {
        commBridge = GameObject.Find("CommunicationsBridge");
        redisSocket = (RedisSocket)commBridge.GetComponent<CommunicationsBridge>().FindSocketConnectionByLabel("Redis");

        if (redisSocket != null)
        {
            redisSocket.UpdateReceived += ReceivedResponse;
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
        byte[] bytes = Encoding.ASCII.GetBytes("$").Concat(
                Encoding.ASCII.GetBytes(messageToSend.Length.ToString())).Concat(
                Encoding.ASCII.GetBytes("\r\n").Concat(
                Encoding.ASCII.GetBytes(messageToSend)).Concat(
                Encoding.ASCII.GetBytes("\r\n"))).ToArray<byte>();
        redisSocket.Write(bytes);
    }

    void ReceivedResponse(object sender, EventArgs e)
    {
        Debug.Log(string.Format("Got response from Redis: {0}", ((RedisEventArgs)e).Content));
    }
}
