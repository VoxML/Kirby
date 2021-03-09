using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxSimPlatform.Global;
using VoxSimPlatform.Vox;

public class KirbyWorldKnowledge : MonoBehaviour
{
    // Dictionary of known objects
    public Dictionary<GameObject, Vector3> objectDict;

    // Represents what, if anything, is being looked for
    public string toFind;

    CommandInput commandInput;
    KirbyEventsModule events;

    // Whether or not Kirby has exhaustively explored his environemnt
    public bool fullyExplored;

    // Ordered list of most recently referenced objects
    public List<GameObject> salientObjects;
    public const int INITIAL_SALIENCEY_LIST_SIZE = 20;

    KirbySpeechModule speech;

    // Start is called before the first frame update
    void Start()
    {
        objectDict = new Dictionary<GameObject, Vector3>();
        GameObject kirbyManager = GameObject.Find("KirbyManager");
        commandInput = kirbyManager.GetComponent<CommandInput>();
        events = kirbyManager.GetComponent<KirbyEventsModule>();
        fullyExplored = false;
        salientObjects = new List<GameObject>(INITIAL_SALIENCEY_LIST_SIZE);
        speech = GameObject.Find("KirbyManager").GetComponent<KirbySpeechModule>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Gets the attribute list of a Voxeme
    public List<string> GetVoxAttributes(GameObject o)
    {
        return o.GetComponent<AttributeSet>().attributes;
    }

    // Gets the predicate of a Voxeme
    public string GetVoxPredicate(GameObject o)
    {
        return o.GetComponent<Voxeme>().voxml.Lex.Pred;
    }

    // Checks whether what we just found is what we were looking for,
    // stops searching and navigates to the located target if so.
    // This is called every time we enconter a new Fiducial.
    public void CheckTargetLocated(GameObject fidObject)
    {
        // Get color and shape of located object
        string locatedColor = GetVoxAttributes(fidObject)[0];
        string locatedShape = GetVoxPredicate(fidObject);
        // As long as we were searching for something
        if (!string.IsNullOrEmpty(toFind))
        {
            string targetColor = ExtractColorFromToFind();
            string targetShape = ExtractShapeFromToFind();

            // If found object's attributes match those of the target
            if (locatedColor.Equals(targetColor) && locatedShape.Equals(targetShape))
            {
                if (toFind.Contains("all") && !fullyExplored)
                {
                    Debug.Log("MADE IT TO ALL" + toFind);
                    int count = CountKnownMatches(targetShape, targetColor);
                    if (count == 1)
                    {
                        DataStore.SetStringValue("kirby:speech", new DataStore.StringValue("There is a " + targetColor + " " + targetShape), speech, string.Empty);
                    }
                    else
                    {
                        DataStore.SetStringValue("kirby:speech", new DataStore.StringValue("There is another " + targetColor + " " + targetShape), speech, string.Empty);
                    }
                }
                else
                {
                    // Update flag to say object has been located
                    DataStore.SetValue("kirby:locatedObject", new DataStore.BoolValue(true), events, string.Empty);
                    // Post a message to make Kirby stop searching
                    commandInput.PostMessage("stop patrol");

                    // get offset from Kirby to object
                    Vector3 offset = DataStore.GetVector3Value("kirby:position") - fidObject.transform.position;
                    offset = new Vector3(offset.x, 0.0f, offset.z);
                    offset = offset.normalized * .125f;

                    Vector3 position = fidObject.transform.position + offset;
                    List<string> coords = new List<string>();
                    coords.Add(position.z.ToString());
                    coords.Add((-position.x).ToString());

                    // publish a go to command, to the location of object we found that matches
                    commandInput.inputController.inputString = string.Format("go to {0} {1}", coords[0], coords[1]);
                    commandInput.PostMessage(commandInput.inputController.inputString);
                }
            }
        }
    }

    public int CountKnownMatches(string shape, string color)
    {
        if (!string.IsNullOrEmpty(toFind))
        {
            int count = 0;
            foreach (KeyValuePair<GameObject, Vector3> kvp in objectDict)
            {
                if (GetVoxAttributes(kvp.Key)[0].Equals(color) &&
                    GetVoxPredicate(kvp.Key).Equals(shape))
                {
                    count++;
                }
            }
            return count;
        }
        else
        {
            return 0;
        }
    }

    public string ExtractColorFromToFind()
    {
        // trim top predicate (a determiner) from the toFind string 
        string trimmed = toFind.Remove(0, toFind.IndexOf('(') + 1);
        trimmed = trimmed.Remove(trimmed.Length - 1, 1);
        // top predicate should now be the color of the block
        string targetColor = GlobalHelper.GetTopPredicate(trimmed);
        return targetColor;
    }

    public string ExtractShapeFromToFind()
    {
        // trim top predicate (a determiner) from the toFind string 
        string trimmed = toFind.Remove(0, toFind.IndexOf('(') + 1);
        trimmed = trimmed.Remove(trimmed.Length - 1, 1);
        // top predicate should now be the color of the block
        string targetColor = GlobalHelper.GetTopPredicate(trimmed);
        // trim the outer two predicates 
        trimmed = toFind.Remove(0, toFind.IndexOf('(') + 1);
        trimmed = trimmed.Remove(trimmed.Length - 1, 1);
        trimmed = trimmed.Remove(0, trimmed.IndexOf('(') + 1);
        trimmed = trimmed.Remove(trimmed.Length - 1, 1);
        // only remaining predicate should be the object, in this case 'block'
        string targetShape = trimmed;
        return targetShape;
    }

}
