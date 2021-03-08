using UnityEngine;
using System.Collections;
using System;
using VoxSimPlatform.Global;

using VoxSimPlatform.Core;
using System.Collections.Generic;
using VoxSimPlatform.Vox;

public class KirbyEventsModule : ModuleBase
{
    EventManager eventManager;

    CommandInput commandInput;

    KirbyWorldKnowledge worldKnowledge;

    // Use this for initialization
    void Start()
    {
        GameObject behaviorController = GameObject.Find("BehaviorController");
        eventManager = behaviorController.GetComponent<EventManager>();

        GameObject kirbyManager = GameObject.Find("KirbyManager");
        commandInput = kirbyManager.GetComponent<CommandInput>();

        GameObject kirbyWorldKnowledge = GameObject.Find("KirbyWorldKnowledge");
        worldKnowledge = kirbyWorldKnowledge.GetComponent<KirbyWorldKnowledge>();

        // Subscribe to "user:event:intent" to trigger "PromptEvent"
        DataStore.Subscribe("user:event:intent", PromptEvent);

        // In the case of a NonexistentEntityError, we will start looking for the entity
        eventManager.NonexistentEntityError += StartLooking;

        // Subscribe to "kirby:patrol:finished" to trigger UpdateExploredFlag
        DataStore.Subscribe("kirby:patrol:finished", UpdateExploredFlag);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PromptEvent(string key, DataStore.IValue value)
    {
        
        string v = DataStore.GetStringValue(key);

        if (!string.IsNullOrEmpty(v))
        {
            eventManager.InsertEvent(string.Empty, 0);
            eventManager.InsertEvent(v, 1);
        }
    }

    public void UpdateExploredFlag(string key, DataStore.IValue value)
    {
        bool patrolled = DataStore.GetBoolValue(key);
        if (patrolled)
        {
            worldKnowledge.fullyExplored = true;
            Debug.Log("Set fully Explored to " + worldKnowledge.fullyExplored);
        }
    }

    public void FIND(object[] args)
    {
        // If we already know about an object that matches what we're looking for
        if (args[0] is GameObject && args[0] != null)
        {
            // Cast to GameObject
            GameObject o = (GameObject)args[0];
            // Extract the shape and color of the known object
            string color = worldKnowledge.GetVoxAttributes(o)[0];
            string shape = worldKnowledge.GetVoxPredicate(o);
            // This prose representation of the target will be used in the dialog
            string target = "a " + color + " " + shape;
            // Update isFinding flag to trigger transition to FindingState in dialog PDA
            DataStore.SetValue("kirby:isFinding", new DataStore.BoolValue(true), this, string.Empty);
            // Store prose representatio in :target key
            DataStore.SetValue("kirby:target", new DataStore.StringValue(target), this, string.Empty);
            // Publish feedback from Kirby that an object is known
            DataStore.SetStringValue("kirby:speech", new DataStore.StringValue("I know about " + target), this, string.Empty);

            // Get the coordinates of the known object
            Vector3 offset = DataStore.GetVector3Value("kirby:position") - o.transform.position;
            offset = new Vector3(offset.x, 0.0f, offset.z);
            offset = offset.normalized * .125f;

            Vector3 position = o.transform.position + offset;
            List<string> coords = new List<string>();
            coords.Add(position.z.ToString());
            coords.Add((-position.x).ToString());

            // Publish a go to command, to the location of object we found that matches
            commandInput.inputController.inputString = string.Format("go to {0} {1}", coords[0], coords[1]);
            commandInput.PostMessage(commandInput.inputController.inputString);
        }
        else
        {
            Debug.LogError("found object is null");
        }
    }

    void StartLooking(object sender, EventArgs e)
    {

        // Get the representation of the user's input
        string V = DataStore.GetStringValue("user:event:intent");
        // Get the top predicate from the representation
        string topPred = GlobalHelper.GetTopPredicate(V);
        // If we are dealing with a find command
        if (Equals(topPred, "find"))
        {
            // Strip the find predicate from the string
            string trimmed = V.Remove(0, 5);
            trimmed = trimmed.Remove(trimmed.Length - 1, 1);
            // What remains is what we are searching for, still in predicate form
            worldKnowledge.toFind = trimmed;
            // Create a prose-y representation by removing ( )
            string english = trimmed.Replace("(", " ");
            english = english.Replace(")", " ");
            // Store this in the :target key
            DataStore.SetValue("kirby:target", new DataStore.StringValue(english.Trim()), this, string.Empty);
        }
        // Set :isFinding flag to trigger Dialog PDA transition
        DataStore.SetValue("kirby:isFinding", new DataStore.BoolValue(true), this, string.Empty);
        // Set flag for when the object has been found to false
        DataStore.SetValue("kirby:locatedObject", new DataStore.BoolValue(false), this, string.Empty);
        // Publish a patrol command to have Kirby start looking
        commandInput.inputController.inputString = "patrol 2 20 5 ";
        commandInput.PostMessage(commandInput.inputController.inputString);
        
    }
}

