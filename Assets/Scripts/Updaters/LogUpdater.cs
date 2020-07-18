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
        string output = "";
        switch (update.code)
        {
            case "READY":
                output = "I have nothing to do.";
                break;

            case "INVALID":
                output = "I don't understand.";
                break;
            
            case "PAUSED":
                output = "Okay, I'll wait.";
                break;
            
            case "RESTARTING":
                output = "Okay, let's go.";
                break;
            
            case "CANCELLED_GOAL":
                output = "Okay, cancelling that.";
                break;
            
            case "CANCELLED_ALL":
                output = "Okay, I cancelled everything.";
                break;
            
            case "FORWARD":
                output = "Sure!";
                break;
            
            case "GO_TO":
                output = "Okay!";
                break;
            
            case "ESTIMATE_ROTATION":
                output = "Okay, turning";
                break;
            
            case "VERIFY_ROTATION":
                output = "";
                break;
            
            case "GO_BACK":
                output = "Okay, I'll go back to where I was.";
                break;
            
            case "SUCCESS_FORWARD":
                output = "Made it!";
                break;
            
            case "SUCCESS_GO_TO":
                output = "Okay, I'm here.";
                break;
            
            case "SUCCESS_ESTIMATE_ROTATION":
                output = "";
                break;
            
            case "SUCCESS_VERIFY_ROTATION":
                output = "I finished.";
                break;
            
            case "SUCCESS_GO_BACK":
                output = "Okay, I made it back.";
                break;
            
            case "UNREACHABLE":
                output = "I can't get there.";
                break;
            
            case "STRAYED":
                output = "Help, I moved but I couldn't make it to my goal.";
                break;
            
            case "HELP":
                output = "Do you want me to keep going from here or go back to " +
                    "where I was?";
                break;
            
            case "PATROL":
                output = "I'll explore.";
                break;
            
            case "PLAN_LOOP":
                output = "I'm planning where to explore next.";
                break;
            
            case "COMPLETED_LOOP":
                output = "I've done some exploration.";
                break;
            
            case "STOP_PATROL":
                output = "Okay, I'll stop exploring.";
                break;
            
            case "FINISH_PATROL":
                output = "I've explored as much as I can.";
                break;
            
            case "DEBUG":
                output = update.message;
                break;
            
            case "QUEUE":
                output = update.message;
                break;
        }
        DataStore.SetStringValue("kirby:speech", new DataStore.StringValue(output), speech, string.Empty);
    }
}
