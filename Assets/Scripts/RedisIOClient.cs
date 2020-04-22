using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using VoxSimPlatform.Network;

public class RedisIOClient : MonoBehaviour
{
    RedisSocket _redisSocket;
    public RedisSocket RedisSocket
    {
        get { return _redisSocket; }
        set { _redisSocket = value; }
    }

    CommunicationsBridge commBridge;

    // Use this for initialization
    void Start()
    {
        commBridge = GameObject.Find("CommunicationsBridge").GetComponent<CommunicationsBridge>();
        _redisSocket = (RedisSocket)commBridge.FindSocketConnectionByType(typeof(RedisIOClient));
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
