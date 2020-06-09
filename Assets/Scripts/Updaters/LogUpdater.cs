using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

using VoxSimPlatform.Global;
using TMPro;

public class LogUpdater : MonoBehaviour
{

    LogUpdate log;
    KirbySpeechModule speech;

    // Use this for initialization
    void Start()
    {
        log = new LogUpdate();
        //publisher = gameObject.GetComponent<RedisPublisher>();
        speech = gameObject.GetComponent<KirbySpeechModule>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateLog(LogUpdate update)
    {
        DataStore.SetStringValue("kirby:speech", new DataStore.StringValue(update.message), speech, string.Empty);
    }
}
