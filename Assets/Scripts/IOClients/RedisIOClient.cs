using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using VoxSimPlatform.Network;

public class RedisIOClient : MonoBehaviour
{
    CommunicationsBridge commBridge;

    RedisSocket _redisSocket;
    public RedisSocket RedisSocket
    {
        get { return _redisSocket; }
        set { _redisSocket = value; }
    }

    public string socketName = string.Empty;

    // Use this for initialization
    void Start()
    {
        commBridge = GameObject.Find("CommunicationsBridge").GetComponent<CommunicationsBridge>();

        List<string> clientNames = commBridge.GetComponents<RedisIOClient>().Select(s => s.socketName).ToList();

        List<string> exclusions = commBridge.SocketConnections.FindAll(s => s.IOClientType == typeof(RedisIOClient) && 
            clientNames.Contains(s.Label)).Select(s => s.Label).ToList();
        
        _redisSocket = (RedisSocket)commBridge.FindSocketConnectionByType(typeof(RedisIOClient), exclusions);

        if (_redisSocket != null)
        {
            socketName = _redisSocket.Label;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_redisSocket != null)
        {
            string redisUrl = string.Format("{0}:{1}", _redisSocket.Address, _redisSocket.Port);
            if (_redisSocket.IsConnected())
            {
                if (commBridge.tryAgainSockets.ContainsKey(redisUrl))
                {
                    if (commBridge.tryAgainSockets[redisUrl] == typeof(RedisSocket))
                    {
                        _redisSocket = (RedisSocket)commBridge.FindSocketConnectionByType(typeof(RedisIOClient));
                    }
                }

                string inputFromRedis = _redisSocket.GetMessage();
                if (inputFromRedis != "")
                {
                    Debug.Log(string.Format("Received message: {0}",inputFromRedis));
                    Debug.Log(_redisSocket.HowManyLeft() + " messages left.");
                    _redisSocket.OnUpdateReceived(this, new RedisEventArgs(RedisEventType.Response,inputFromRedis));
                }
            }
            else
            {
                if (!commBridge.tryAgainSockets.ContainsKey(redisUrl))
                {
                    commBridge.tryAgainSockets.Add(redisUrl, _redisSocket.GetType());
                }
            }
        }
    }
}
