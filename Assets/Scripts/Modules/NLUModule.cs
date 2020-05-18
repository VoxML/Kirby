/*
This script parses natural language input into forms VoxSim can work with

Reads:      user:speech (StringValue, transcribed speech)

Writes:     
*/

using UnityEngine;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Timers;

using VoxSimPlatform.Agent;

public class NLUModule : ModuleBase
{
    public GameObject kirbyManager;

    CommandInput commandInput;

    Timer clearSpeechTimer;
    public int clearSpeechTime;

    bool clearSpeech = false;

    // Use this for initialization
    void Start()
    {
        base.Start();
        DataStore.Subscribe("user:speech", ParseLanguageInput);

        commandInput = kirbyManager.GetComponent<CommandInput>();

        clearSpeechTimer = new Timer(clearSpeechTime);
        clearSpeechTimer.Enabled = false;
        clearSpeechTimer.Elapsed += ClearSpeech;
    }

    // Update is called once per frame
    void Update()
    {
        if (clearSpeech)
        {
            SetValue("user:speech", string.Empty, string.Empty);
            commandInput.inputController.inputString = string.Empty;
            clearSpeech = false;
        }
    }

    // callback when user:speech changes
    void ParseLanguageInput(string key, DataStore.IValue value)
    {
        if (!string.IsNullOrEmpty(DataStore.GetStringValue(key)))
        {
            if (!DataStore.GetBoolValue("kirby:isListening"))
            {
                return;
            }

            if (DataStore.GetBoolValue("user:isMuted"))
            {
                return;
            }

            commandInput.inputController.inputString = DataStore.GetStringValue(key);
            commandInput.PostMessage(commandInput.inputController.inputString);
            clearSpeechTimer.Enabled = true;
        }
    }

    void ClearSpeech(object sender, ElapsedEventArgs e)
    {
        clearSpeechTimer.Enabled = false;
        clearSpeechTimer.Interval = clearSpeechTime;
        clearSpeech = true;
    }
}
