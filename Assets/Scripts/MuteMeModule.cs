/*
    Put this script on the same GameObject as the AudioSource
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuteMeModule : ModuleBase
{
    public bool mutedByDefault;

    [Tooltip("Press and hold this key to unmute")]
    public KeyCode muteToggle;
    
    public OutputDisplay outputDisplay;


    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        DataStore.Subscribe("user:isSpeaking", CheckUserSpeaking);
    }

    // Update is called once per frame
    void Update()
    {
        if (mutedByDefault)
        {
            if (Input.GetKey(muteToggle))
            {
                SetValue("user:isMuted", false, string.Empty);
                outputDisplay.Clear();
            }
            else
            {
                SetValue("user:isMuted", true, string.Empty);
            }
        }
    }

    // callback when user:isSpeaking changes
    void CheckUserSpeaking(string key, DataStore.IValue value)
    {
        // user is speaking
        if (DataStore.GetBoolValue(key))
        {
            if (DataStore.GetBoolValue("kirby:isListening") &&
                DataStore.GetBoolValue("user:isMuted"))
            {
                outputDisplay.SetText(string.Format("You are muted. Hold \"{0}\" to unmute.",muteToggle.ToString()), TextDisplayMode.Persistent);
            }
        }
        else
        {
            outputDisplay.Clear();
        }
    }
}
