using UnityEngine;
using System;
using System.Linq;
using System.Text;

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
        if (Input.GetKeyDown(KeyCode.A))
        {
            // ping
            WriteCommand("ping");
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            // set foo bar
            WriteCommand("set foo bar");
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
