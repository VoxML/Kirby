/*
This script parses natural language input into forms VoxSim can work with

Reads:      user:speech (StringValue, transcribed speech)

Writes:     
*/

using UnityEngine;
using System;
using System.Linq;
using System.Text.RegularExpressions;

using VoxSimPlatform.Agent;

public class NLUModule : ModuleBase
{
    public InputController inputController;

    // Use this for initialization
    void Start()
    {
        base.Start();
        DataStore.Subscribe("user:speech", ParseLanguageInput);
    }

    // Update is called once per frame
    void Update()
    {

    }

    // callback when user:speech changes
    void ParseLanguageInput(string key, DataStore.IValue value)
    {
        if (!DataStore.GetBoolValue("kirby:isListening"))
        {
            return;
        }

        inputController.inputString = DataStore.GetStringValue(key);
    }
}
