using UnityEngine;
using System;
using System.Linq;
using System.Text;

using Newtonsoft.Json;
using VoxSimPlatform.Network;

public class RedisInterface : MonoBehaviour
{
    protected GameObject commBridge;

    protected RedisSocket redisSocket;

    public bool authenticated = false;

    public bool usingRejson = false;

    // Start is called before the first frame update
    public virtual void Start()
    {
        commBridge = GameObject.Find("CommunicationsBridge");
    }

    // Update is called once per frame
    public virtual void Update()
    {

    }

    public virtual void WriteCommand(string messageToSend)
    {
        // take a message to sent to Redis, turn it into a properly formatted bulk string, and send it
        string bulkString = string.Format("${0}\r\n{1}\r\n", messageToSend.Length.ToString(), messageToSend);
        Debug.Log(string.Format("Writing bulk string command \"{0}\" to Redis", bulkString.Replace("\r","\\r").Replace("\n", "\\n")));
        byte[] bytes = Encoding.ASCII.GetBytes(bulkString).ToArray<byte>();
        redisSocket.Write(bytes);
    }

    protected char GetResponseType(string raw)
    {
        char type = '\0';
        Debug.Log(string.Format("Raw content from Redis: {0}", raw));

        // split on new lines
        // if first line is "unknown command" response -- throw it out
        //  type is first char of second line
        string first = raw.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();

        if (first.StartsWith("unknown command"))
        {
            string[] content = raw.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (content.Length > 1)
            {
                if (content[1].Length > 0)
                { 
                    type = raw.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)[1][0];
                }
            }
        }
        else
        {
            if (first.Length > 0)
            { 
                type = first[0];
            }
        }

        return type;
    }
}
