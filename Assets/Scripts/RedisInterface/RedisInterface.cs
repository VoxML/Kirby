using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Newtonsoft.Json;
using VoxSimPlatform.Network;

public enum RedisMessageType
{
    Array,
    BulkString
};

public class RedisInterface : MonoBehaviour
{
    protected GameObject commBridge;

    protected RedisSocket redisSocket;

    public bool authenticated = false;

    public bool usingRejson = false;

    [SerializeField]
    bool useSizeHeader;
    public bool UseSizeHeader
    {
        get { return useSizeHeader; }
        set
        {
            useSizeHeader = value;
            OnUseSizeHeaderChanged(useSizeHeader);
        }
    }

    [SerializeField]
    bool verboseDebugOutput;
    public bool VerboseDebugOutput
    {
        get { return verboseDebugOutput; }
        set
        {
            verboseDebugOutput = value;
            OnVerboseDebugOutputChanged(verboseDebugOutput);
        }
    }

    // Start is called before the first frame update
    public virtual void Start()
    {
        commBridge = GameObject.Find("CommunicationsBridge");
    }

    // Update is called once per frame
    public virtual void Update()
    {

    }

    public virtual void WriteArrayCommand(string messageToSend)
    {
        // take a message to send to Redis, turn it into a properly formatted array, and send it
        Debug.Log(string.Format("Formatting string \"{0}\"", messageToSend));
        Regex re = new Regex("(?=.+\") (?<!\".+)| (?!.+\")");
        string[] msgStrings = re.Split(messageToSend);

        List<string> bulkedMsgStrings = new List<string>();
        for (int i = 0; i < msgStrings.Length; i++)
        {
            bulkedMsgStrings.Add(string.Format("${0}\r\n{1}\r\n", msgStrings[i].Length-msgStrings[i].Count<char>(c => c == '\"'), msgStrings[i]));
        }

        string bulkedMsg = string.Join(string.Empty, bulkedMsgStrings).Replace("\"","");

        string arrayString = string.Format("*{0}\r\n{1}", msgStrings.Length, bulkedMsg);

        Debug.Log(string.Format("Writing array command \"{0}\" to Redis", arrayString.Replace("\r", "\\r").Replace("\n", "\\n")));
        byte[] bytes = Encoding.ASCII.GetBytes(arrayString).ToArray<byte>();
        redisSocket.Write(bytes);
    }

    public virtual void WriteBulkStringCommand(string messageToSend)
    {
        // take a message to send to Redis, turn it into a properly formatted bulk string, and send it
        string bulkString = string.Format("${0}\r\n{1}\r\n", messageToSend.Length.ToString(), messageToSend);
        Debug.Log(string.Format("Writing bulk string command \"{0}\" to Redis", bulkString.Replace("\r","\\r").Replace("\n", "\\n")));
        byte[] bytes = Encoding.ASCII.GetBytes(bulkString).ToArray<byte>();
        redisSocket.Write(bytes);
    }

    public virtual void WriteAuthentication(string messageToSend)
    {
        WriteArrayCommand(messageToSend);
        //string bulkString = string.Format("${0}\r\n{1}\r\n", messageToSend.Length.ToString(), messageToSend);
        //Debug.Log(string.Format("Writing bulk string command \"{0}\" to Redis", bulkString.Replace("\r", "\\r").Replace("\n", "\\n")));
        //byte[] bytes = Encoding.ASCII.GetBytes(bulkString).ToArray<byte>();
        //redisSocket.Write(bytes);
    }

    protected char GetResponseType(string raw)
    {
        char type = '\0';
        Debug.Log(string.Format("Raw content from Redis: {0}", raw));

        // split on new lines
        // if first line is "unknown command" response -- throw it out
        //  type is first char of second line
        string first = raw.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();

        if (first.Contains("unknown command"))
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

    void OnUseSizeHeaderChanged(bool val)
    {
        redisSocket.UseSizeHeader = val;
    }

    void OnVerboseDebugOutputChanged(bool val)
    {
        redisSocket.VerboseDebugOutput = val;
    }
}
