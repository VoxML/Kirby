using UnityEngine;
using System.Collections;

using System.IO;
using System.Xml.Serialization;

public class KirbyRedisParams
{
    public string Password;
    public string Namespace;
}

public class RedisConfig : MonoBehaviour
{
    public string configFile;

    RedisSubscriber subscriber;
    RedisPublisherManager pubManager;

    // Use this for initialization
    void Start()
    {
        subscriber = gameObject.GetComponent<RedisSubscriber>();
        pubManager = gameObject.GetComponent<RedisPublisherManager>();
        if (!string.IsNullOrEmpty(configFile))
        {
            KirbyRedisParams config = Load(configFile);
            if (config != null)
            {
                subscriber.password = config.Password;
                pubManager.password = config.Password;

                if (string.IsNullOrEmpty(pubManager.namespacePrefix))
                {
                    pubManager.namespacePrefix = config.Namespace;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public KirbyRedisParams Load(string path)
    {
        if (File.Exists(path))
        {
            XmlSerializer serializer = new XmlSerializer(typeof(KirbyRedisParams));
            using (var stream = new FileStream(path, FileMode.Open))
            {
                return serializer.Deserialize(stream) as KirbyRedisParams;
            }
        }
        else
        {
            Debug.LogWarningFormat("No config file {0} found!", path);
            return null;
        }

    }
}
