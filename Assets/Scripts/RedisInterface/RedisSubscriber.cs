using UnityEngine;
using System;
using System.Linq;

using VoxSimPlatform.Network;

public class RedisSubscriber : RedisInterface
{
    RedisPublisher publisher;

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

        publisher = gameObject.GetComponent<RedisPublisher>();
    }

    // Update is called once per frame
    void Update()
    {

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
                /*
                    $8
                    pmessage
                    $10
                    __key* __:*
                    $19
                    __keyspace@0__:json
                    $3
                    set
                    *4
                    $8
                    pmessage
                    $10
                    __key* __:*
                    $18
                    __keyevent@0__:set
                    $4
                    json
                */
                response = raw.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Last();
                Debug.Log(string.Format("RedisPublisher: Got bulk string response from Redis: {0}", response));

                string key = response;
                if (usingRejson)
                {
                    publisher.WriteCommand(string.Format("json.get {0}", key));
                }
                else
                {
                    publisher.WriteCommand(string.Format("get {0}", key));
                }
                break;

            // arrays
            case '*':
                break;

            default:
                break;
        }
    }
}
