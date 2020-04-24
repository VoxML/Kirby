using UnityEngine;
using System;

using VoxSimPlatform.Network;

public class RedisSubscriber : RedisInterface
{
    // Start is called before the first frame update
    void Start()
    {
        base.Start();

        redisSocket = (RedisSocket)commBridge.GetComponent<CommunicationsBridge>().FindSocketConnectionByLabel("RedisSubscriber");

        if (redisSocket != null)
        {
            // try authentication
            if (!authenticated)
            {
                WriteCommand("auth ROSlab134");
            }
        }

        redisSocket.UpdateReceived += ReceivedUpdate;

    }

    // Update is called once per frame
    void Update()
    {

    }

    void PublisherAuthenticated()
    {
        //if (!authenticated)
        //{
        //    WriteCommand("auth ROSlab134");
        //}
    }

    public void ReceivedUpdate(object sender, EventArgs e)
    {
        string raw = ((RedisEventArgs)e).Content;
        char type = GetResponseType(((RedisEventArgs)e).Content);

        string response = string.Empty;
        switch (type)
        {
            // simple strings
            case '+':
                response = raw.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)[1].TrimStart('+');
                Debug.Log(string.Format("RedisSubscriber: Got simple string response from Redis: {0}", response));

                if (!authenticated)
                {
                    if (string.Equals(response, "OK"))
                    {
                        authenticated = true;
                        WriteCommand("psubscribe \'__key*__:*\'");
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
                break;

            // arrays
            case '*':
                break;

            default:
                break;
        }
    }
}
