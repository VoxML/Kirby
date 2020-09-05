using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxSimPlatform.Global;
using VoxSimPlatform.Vox;

public class KirbyWorldKnowledge : MonoBehaviour
{
    public Dictionary<GameObject, Vector3> objectDict;

    // declare "to find" variable
    public string toFind;
    CommandInput commandInput;

    // Start is called before the first frame update
    void Start()
    {
        // TODO: instantiate object dict (new Dictionary<,>)
        Debug.Log("ABOUT TO MAKE A DICTIONARY");
        objectDict = new Dictionary<GameObject, Vector3>();
        Debug.Log("I MADE A DICITONARY");
        Debug.Log("my new dict: " + objectDict);

        // get CommandInput component on "KirbyManager" object
        GameObject kirbyManager = GameObject.Find("KirbyManager");
        commandInput = kirbyManager.GetComponent<CommandInput>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // TODO: create object characteristic assessment method here
    //  (see FiducialUpdater)
    // if toFind is not null and the input object for this method
    //  matches the predicate-argument structure listed in toFind
    //  interrupt patrol and navigate to that object
    // we will need to declare and get things like CommandInput
    //  as we do in KirbyEventsModule
    public void CheckTargetLocated(GameObject fidObject)
    {
        Debug.Log("toFind is: " + toFind);
        toFind = "the(blue(block))";
        if (toFind != null)
        {
            // trim top predicate (a determiner) from the toFind string 
            string trimmed = toFind.Remove(0, toFind.IndexOf('(') + 1);
            // remove outer closing parenthesis 
            trimmed = trimmed.Remove(trimmed.Length - 1, 1);
            // top predicate should now be the color of the block
            string targetColor = GlobalHelper.GetTopPredicate(trimmed);
            Debug.Log("Target color: " + targetColor);

            // trim the outer predicate again - color
            trimmed = toFind.Remove(0, toFind.IndexOf('(') + 1);
            trimmed = trimmed.Remove(trimmed.Length - 1, 1);
            Debug.Log("beofre final trim: " + trimmed);
            trimmed = trimmed.Remove(0, trimmed.IndexOf('(') + 1);
            trimmed = trimmed.Remove(trimmed.Length - 1, 1);
            // only remaining predicate should be block 
            string targetShape = trimmed;
            Debug.Log("Target shape: " + targetShape);

            // get the Voxeme of the new fiducial/block
            Voxeme fidObjVox = fidObject.GetComponent<Voxeme>();
            Debug.Log("Voxeme : " + fidObjVox);
            Debug.Log("Attributes : " + fidObjVox.voxml.Attributes);
            Debug.Log("Attrs: " + fidObjVox.voxml.Attributes.Attrs);
            Debug.Log("Pred: " + fidObjVox.voxml.Lex.Pred);
            Debug.Log("Type: " + fidObjVox.voxml.Lex.Type);
            Debug.Log("Attrs[0] " + fidObjVox.voxml.Attributes.Attrs[0]);
            Debug.Log("The value is : " + fidObjVox.voxml.Attributes.Attrs[0].Value);
            Debug.Log("The shape is: " + fidObjVox.voxml.Lex.Pred);
            string locatedColor = fidObjVox.voxml.Attributes.Attrs[0].Value;
            
            Debug.Log("located color: " + locatedColor);
            string locatedShape = fidObjVox.voxml.Lex.Pred;
            Debug.Log("located shape: " + locatedShape);

            if (locatedColor.Equals(targetColor) && locatedShape.Equals(targetShape))
            {
                Debug.Log("Indeed we are equal");
                // attempt to post a 'stop patrol' command.
                // Is it necessary to set commandInput.inputController.inputString?
                commandInput.PostMessage("stop patrol");
                // try to get x, y coords of the fiducial
                double x = fidObject.transform.position[0];
                double y = fidObject.transform.position[1];
                // publish a go to command, to the location of block we found that matches
                commandInput.inputController.inputString = "go to " + x + " " + y;
                commandInput.PostMessage(commandInput.inputController.inputString);
            }
        }
    }

}
