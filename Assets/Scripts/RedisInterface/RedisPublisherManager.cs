using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using VoxSimPlatform.Network;

public class RedisPublisherManager : MonoBehaviour
{
    CommunicationsBridge commBridge;
    RedisSubscriber subscriber;

    OutputDisplay outputDisplay;

    int numAuthenticatedPublishers;

    public Dictionary<string, RedisPublisher> publishers;

    public string namespacePrefix;

    // keys
    // no other field in this class should end in "Key"
    //  this will cause problems in generating the "psubscribe"
    //  command in the subscriber
    public string mapKey;
    public string roboKey;
    public string fiducialKey;
    public string resetKey;
    public string cmdKey;
    public string logKey;

    // Start is called before the first frame update
    void Start()
    {
        //TODO: route this through VoxSim OutputController
        outputDisplay = GameObject.Find("OutputDisplay").GetComponent<OutputDisplay>();
        commBridge = GameObject.Find("CommunicationsBridge").GetComponent<CommunicationsBridge>();

        subscriber = gameObject.GetComponent<RedisSubscriber>();

        publishers = new Dictionary<string, RedisPublisher>();

        numAuthenticatedPublishers = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SubscribedToNotifications()
    {
        Debug.Log("RedisPublisherManager: picked up message SubscribedToNotifications");

        outputDisplay.SetText("Creating publishers...", TextDisplayMode.Persistent);
        CreatePublishers();
    }

    public void PublisherAuthenticated()
    {
        Debug.Log("RedisPublisherManager: picked up message PublisherAuthenticated");
        numAuthenticatedPublishers++;

        if (numAuthenticatedPublishers == publishers.Count)
        {
            BroadcastMessage("AllPublishersAuthenticated");
        }
    }

    public void AllPublishersAuthenticated()
    {
        Debug.Log("RedisPublisherManager: picked up message AllPublishersAuthenticated");

        //List<RedisIOClient> obsoletePublishers = commBridge.GetComponents<RedisIOClient>().Where(s => s.socketName == "RedisPublisher").ToList();
        //for (int i = 0; i < obsoletePublishers.Count; i++)
        //{
        //    Destroy(obsoletePublishers[i]);
        //}

        //RedisSocket redisSocket = (RedisSocket)commBridge.FindSocketConnectionByLabel("RedisPublisher");
        //redisSocket.Close();

        TriggerResetBridge();
    }

    public void CreatePublishers()
    {
        // get all keys defined (all field names that end in "Key")
        List<string> keyVarNames = this.GetType().GetFields().Where(f => f.Name.EndsWith("Key")).Select(f => f.Name).ToList();

        foreach (string key in keyVarNames)
        {
            string keyName = (string)this.GetType().GetField(key).GetValue(this);
            if (!string.IsNullOrEmpty(keyName))
            {
                Debug.Log(string.Format("Creating RedisIOClient for key \"{0}\"", keyName));

                RedisPublisher publisher = gameObject.AddComponent<RedisPublisher>();
                publisher.publisherKey = keyName;

                publishers[publisher.publisherKey] = publisher;
            }
        }

        Debug.Log(string.Format("Created {0} publishers", publishers.Count));
    }

    public void TriggerResetBridge()
    {
        if (!string.IsNullOrEmpty(resetKey))
        { 
            outputDisplay.SetText("Flushing database...", TextDisplayMode.Persistent);

            // halt subscriber processing while bridge is reset
            subscriber.processing = false;
            publishers[resetKey].WriteCommand(string.Format("set {0} 1", string.Format("{0}/{1}", namespacePrefix, resetKey)));
        }
        else
        {
            outputDisplay.SetText(string.Empty);
            Debug.Log("RedisPublisherManager.TriggerResetBridge: No value for resetKey variable given!");
        }
    }
}
