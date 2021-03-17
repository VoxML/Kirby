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

        DataStore.SetValue("kirby:lookingForMore", new DataStore.BoolValue(false), this, string.Empty);
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
        //Debug.Log("I FOUND: " + args.Length);
        Debug.Log("PREDICATE: " + DataStore.GetStringValue("user:event:intent"));
        String quant = ExtractDeterminers(DataStore.GetStringValue("user:event:intent"));
        if (quant.Contains("all") || quant.Contains("every"))
        {
            if (worldKnowledge.fullyExplored)
            {
                String status = "I have already fully searched. There are " + (args.Length - 1);
                DataStore.SetStringValue("kirby:speech", new DataStore.StringValue(status), this, string.Empty);
            }
            else
            {
                String status = "I know about " + (args.Length - 1) + " already. I will look for more.";
                DataStore.SetStringValue("kirby:speech", new DataStore.StringValue(status), this, string.Empty);
                DataStore.SetValue("kirby:lookingForMore", new DataStore.BoolValue(true), this, string.Empty);
                BeginExploration();
            }
        }
        else if (quant.Contains("two"))
        {
            if (args[1] == null)
            {
                if (worldKnowledge.fullyExplored)
                {
                    String status = "I have already looked everywhere and there is only one.";
                    DataStore.SetStringValue("kirby:speech", new DataStore.StringValue(status), this, string.Empty);  
                }
                else
                {
                    String status = "I already know about one. I will look for another.";
                    DataStore.SetStringValue("kirby:speech", new DataStore.StringValue(status), this, string.Empty);
                    DataStore.SetValue("kirby:lookingForMore", new DataStore.BoolValue(true), this, string.Empty);
                    BeginExploration();
                }
            }
            else
            {
                String status = "I have already found two.";
                Debug.Log("I alread found two");
                DataStore.SetStringValue("kirby:speech", new DataStore.StringValue(status), this, string.Empty);
            }
        }
        else
        {
            // If we already know about an object that matches what we're looking for
            if (args[0] is GameObject && args[0] != null)
            {

                //Debug.Log("I'm in FIND");
                //Debug.Log("I found: " + args.Length);
                //for (int i = 0; i < args.Length; i++)
                //{
                //    Debug.Log("I found: " + i);
                //    GameObject obj = (GameObject)args[i];
                //    string c = worldKnowledge.GetVoxAttributes(obj)[0];
                //    string s = worldKnowledge.GetVoxPredicate(obj);
                //    Debug.Log("I found: " + c + " " + s);
                //}
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
        
    }

    void StartLooking(object sender, EventArgs e)
    {
        
        // Get the representation of the user's input
        string V = DataStore.GetStringValue("user:event:intent");
        Debug.Log("PREDICATE: " + V);
        // Get the top predicate from the representation
        string topPred = GlobalHelper.GetTopPredicate(V);
        // If we are dealing with a find command
        if (Equals(topPred, "find"))
        {
            if (!worldKnowledge.fullyExplored)
            {
                BeginExploration();
            }
            else
            {
                DataStore.SetStringValue("kirby:speech", new DataStore.StringValue("I have looked and there are none."), this, string.Empty);
            }
           
        } 
    }

    void BeginExploration()
    {
        // Get the representation of the user's input
        string V = DataStore.GetStringValue("user:event:intent");
        // Strip the find predicate from the string
        string trimmed = V.Remove(0, 5);
        trimmed = trimmed.Remove(trimmed.Length - 1, 1);
        // What remains is what we are searching for, still in predicate form
        worldKnowledge.toFind = trimmed;
        // Create a prose-y representation by removing ( )
        string prose = trimmed.Replace("(", " ");
        prose = prose.Replace(")", " ");
        // Store this in the :target key
        if (prose.Contains("all") || prose.Contains("every"))
        {
            DataStore.SetValue("kirby:target", new DataStore.StringValue(prose.Trim() + "s"), this, string.Empty);
        }
        else
        {
            DataStore.SetValue("kirby:target", new DataStore.StringValue(prose.Trim()), this, string.Empty);
        }
        // Set :isFinding flag to trigger Dialog PDA transition
        DataStore.SetValue("kirby:isFinding", new DataStore.BoolValue(true), this, string.Empty);
        // Set flag for when the object has been found to false
        DataStore.SetValue("kirby:locatedObject", new DataStore.BoolValue(false), this, string.Empty);
        // Publish a patrol command to have Kirby start looking
        commandInput.inputController.inputString = "patrol 2 20 5 ";
        Debug.Log("Patrol before");
        //commandInput.inputController.inputString = "patrol";
        commandInput.PostMessage(commandInput.inputController.inputString);
        Debug.Log("Patrol after");
    }


    // Assumes it is getting a predicate string representation of the user input. 
    public String ExtractDeterminers(String userIntent)
    {
        char[] delimiters = { '(', ')' };
        string[] words = userIntent.Split(delimiters, System.StringSplitOptions.RemoveEmptyEntries);
        words[0] = "";  // remove find
        words[words.Length - 1] = "";   // remove color
        words[words.Length - 2] = "";   // remove shape
        return String.Join(" ", words);
    }
}

