/*
This script manages Kirby's speech output

Reads:      kirby:speech (StringValue, Kirby's speech output)

Writes:               
*/

using UnityEngine;
using System;

using VoxSimPlatform.Agent;

public class KirbySpeechModule : ModuleBase
{
    AgentOutputController agentOutput;
    AgentTextController agentText;
    AgentVoiceController agentVoice;

    void Start()
    {
        base.Start();

        // TODO: in VoxSim - link AgentTextController and AgentVoiceController
        agentOutput = GameObject.Find("RoboStandIn").GetComponentInChildren<AgentOutputController>();
        agentText = GameObject.Find("RoboStandIn").GetComponentInChildren<AgentTextController>();
        agentVoice = GameObject.Find("RoboStandIn").GetComponentInChildren<AgentVoiceController>();

        DataStore.Subscribe("kirby:speech", Speak);
    }

    // Update is called once per frame
    void Update()
    {
    }

    // callback when kirby:speech changes
    void Speak(string key, DataStore.IValue value)
    {
        if (!string.IsNullOrEmpty(DataStore.GetStringValue(key)))
        {
            agentOutput.outputString = DataStore.GetStringValue(key);
            agentText.outputString = agentOutput.outputString;
            agentVoice.Speak(agentOutput.outputString);
        }
    }
}
