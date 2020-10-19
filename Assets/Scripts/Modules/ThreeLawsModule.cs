using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreeLawsModule : ModuleBase
{
    public bool heavyMetalRobot;
    public AudioSource audioSource;

    const string THREE_LAWS_OF_ROBOTICS = "I must not injure a human or through inaction bring one to harm.  " +
        "I must obey all orders given unless it conflicts with the First Law.  " +
        "I must protect my own existence unless it contradicts the first two Laws.";

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        DataStore.Subscribe("user:speech", SpeakThreeLaws);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // callback when user:speech changes
    void SpeakThreeLaws(string key, DataStore.IValue value)
    {
        // "hey kirby" = Easter egg
        if (DataStore.GetStringValue(key).ToLower().StartsWith("hey kirby"))
        {
            if (DataStore.GetStringValue(key).ToLower() == "hey kirby what are the three laws")
            {
                if (heavyMetalRobot && audioSource.clip != null)
                {
                    audioSource.PlayOneShot(audioSource.clip);
                }
                else
                {
                    SetValue("kirby:speech", THREE_LAWS_OF_ROBOTICS, string.Empty);
                }
            }
        }
    }
}
