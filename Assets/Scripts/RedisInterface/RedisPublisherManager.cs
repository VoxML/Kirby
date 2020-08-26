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

    List<string> publisherKeyVarNames;
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

        publisherKeyVarNames = this.GetType().GetFields().Where(f => f.Name.EndsWith("Key") &&
            (string)this.GetType().GetField(f.Name).GetValue(this) != string.Empty).Select(f => f.Name).ToList();
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
        CreatePublisher(publisherKeyVarNames[numAuthenticatedPublishers]);
    }

    public void PublisherAuthenticated()
    {
        Debug.Log("RedisPublisherManager: picked up message PublisherAuthenticated");
        numAuthenticatedPublishers++;

        if (numAuthenticatedPublishers == publisherKeyVarNames.Count)
        {
            BroadcastMessage("AllPublishersAuthenticated");
        }
        else
        {
            CreatePublisher(publisherKeyVarNames[numAuthenticatedPublishers]);
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

    public void CreatePublisher(string keyVarName)
    {
        if (!string.IsNullOrEmpty(keyVarName))
        {
            string keyName = (string)this.GetType().GetField(keyVarName).GetValue(this);

            Debug.Log(string.Format("Creating RedisPublisher for key \"{0}\"", keyName));

            RedisPublisher publisher = gameObject.AddComponent<RedisPublisher>();
            publisher.publisherKey = keyName;

            publishers[publisher.publisherKey] = publisher;
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
            publishers[resetKey].WriteCommand(string.Format("set {0} \"1\"", string.Format("{0}/{1}", namespacePrefix, resetKey)));
        }
        else
        {
            outputDisplay.SetText(string.Empty);
            Debug.Log("RedisPublisherManager.TriggerResetBridge: No value for resetKey variable given!");
        }
    }
}
