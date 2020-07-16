/*
This script parses natural language input into forms VoxSim can work with

Reads:      user:speech (StringValue, transcribed speech)

Writes:     
*/

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Timers;

using VoxSimPlatform.Agent;

public class NLUModule : ModuleBase
{
    public GameObject kirbyManager;
    public OutputController speechInputDisplay;

    CommandInput commandInput;

    Timer clearSpeechTimer;
    public int clearSpeechTime;

    bool clearSpeech = false;

    public bool testingMode;

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
            speechInputDisplay.outputString = string.Empty;
            clearSpeech = false;
        }
    }

    void OnGUI()
    {
        Event e = Event.current;
        if (e.keyCode == KeyCode.Return)
        {
            if (speechInputDisplay.outputString != string.Empty)
            {
                Debug.Log(string.Format("Setting user:speech to \"{0}\"", speechInputDisplay.outputString.Trim()));
                SetValue("user:speech", speechInputDisplay.outputString.Trim(), string.Empty);
                speechInputDisplay.outputString = string.Empty;
            }
        }
    }

    // callback when user:speech changes
    void ParseLanguageInput(string key, DataStore.IValue value)
    {
        if (!string.IsNullOrEmpty(DataStore.GetStringValue(key)))
        {
            // testingMode turns off the requirement that kirby:isAttendingSpeech
            //  be true to proceed (so you can test speech commands without
            //  having to be hooked up to the whole system
            if (!testingMode)
            {
                if (!DataStore.GetBoolValue("kirby:isAttendingSpeech"))
                {
                    return;
                }
            }

            if (DataStore.GetBoolValue("user:isMuted"))
            {
                return;
            }

            speechInputDisplay.outputString = DataStore.GetStringValue(key);

            string commands = MapLanguageToCommands(DataStore.GetStringValue(key));

            if (commands != string.Empty) {
                commandInput.inputController.inputString = commands;
                commandInput.PostMessage(commandInput.inputController.inputString);
                clearSpeechTimer.Enabled = true;
            }
        }
    }

    void ClearSpeech(object sender, ElapsedEventArgs e)
    {
        clearSpeechTimer.Enabled = false;
        clearSpeechTimer.Interval = clearSpeechTime;
        clearSpeech = true;
    }

    string MapLanguageToCommands(string rawInput)
    {
        string output = string.Empty;
        string input = rawInput.ToLower();

        if ((input == "go here") || (input == "go there"))
        {
            output = GoToCommand(input);
        }
        else if (input == "go to that one")
        {
            output = GoToThatCommand(input);
        }
        else if (input == "go forward")
        {
            output = GoForwardCommand(input);
        }
        else if ((input == "turn left") || (input == "turn right"))
        {
            output = TurnCommand(input);
        }
        else if ((input == "patrol") || (input == "explore"))
        {
            output = PatrolCommand(input);
        }
        else if ((input == "cancel") || (input == "cancel all"))
        {
            output = CancelCommand(input);
        }

        Debug.Log(string.Format("Mapped \"{0}\" to \"{1}\"", input, output));
        return output;
    }

    string GoToCommand(string input)
    {
        string command = string.Empty;

        if (!string.IsNullOrEmpty(DataStore.GetStringValue("user:lastPointedAt:name")))
        {
            // go to object
        }
        else if (DataStore.GetVector3Value("user:lastPointedAt:position") != default)
        {
            // go to location
            Vector3 position = DataStore.GetVector3Value("user:lastPointedAt:position");
            List<string> coords = new List<string>();
            coords.Add(position.z.ToString());
            coords.Add((-position.x).ToString());
            command = string.Format("go to {0} {1}", coords[0], coords[1]);
        }

        return command;
    }

    string GoToThatCommand(string input)
    {
        string command = string.Empty;

        if (!string.IsNullOrEmpty(DataStore.GetStringValue("user:lastPointedAt:name")))
        {
            // go to object

            // get object
            GameObject targetObj = GameObject.Find(DataStore.GetStringValue("user:lastPointedAt:name"));

            // get offset from Kirby to object
            Vector3 offset = targetObj.transform.position-DataStore.GetVector3Value("kirby:position");
            offset = new Vector3(offset.x, 0.0f, offset.z);
            offset = offset.normalized * .125f;

            Vector3 position = targetObj.transform.position+offset;
            List<string> coords = new List<string>();
            coords.Add(position.z.ToString());
            coords.Add((-position.x).ToString());
            command = string.Format("go to {0} {1}", coords[0], coords[1]);
        }

        return command;
    }

    string GoForwardCommand(string input)
    {
        string command = input;

        return command;
    }

    string TurnCommand(string input)
    {
        string command = input;

        return command;
    }

    string PatrolCommand(string input)
    {
        string command = "patrol";

        return command;
    }

    string CancelCommand(string input)
    {
        string command = input;

        return command;
    }
}
