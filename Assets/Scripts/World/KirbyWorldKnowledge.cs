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

    // checks whether what we just found is what we were looking for,
    // stops searching and navigates to the located target if so
    public void CheckTargetLocated(GameObject fidObject)
    {
        if (toFind != null)
        {
            // trim top predicate (a determiner) from the toFind string 
            string trimmed = toFind.Remove(0, toFind.IndexOf('(') + 1);
            // remove outer closing parenthesis 
            trimmed = trimmed.Remove(trimmed.Length - 1, 1);
            // top predicate should now be the color of the block
            string targetColor = GlobalHelper.GetTopPredicate(trimmed);
            //Debug.Log("Target color: " + targetColor);

            // trim the outer two predicates 
            trimmed = toFind.Remove(0, toFind.IndexOf('(') + 1);
            trimmed = trimmed.Remove(trimmed.Length - 1, 1);
            trimmed = trimmed.Remove(0, trimmed.IndexOf('(') + 1);
            trimmed = trimmed.Remove(trimmed.Length - 1, 1);
            // only remaining predicate should be the object, in this case 'block'
            string targetShape = trimmed;

            // get the Voxeme of the new fiducial/block
            Voxeme fidObjVox = fidObject.GetComponent<Voxeme>();
            // the color of the object we found
            string locatedColor = fidObjVox.voxml.Attributes.Attrs[0].Value;
            // the shape/type of the object we found
            string locatedShape = fidObjVox.voxml.Lex.Pred;

            // if what we found matches what we're looking for
            if (locatedColor.Equals(targetColor) && locatedShape.Equals(targetShape))
            {
                commandInput.PostMessage("stop patrol");

                // get offset from Kirby to object
                Vector3 offset = DataStore.GetVector3Value("kirby:position") - fidObject.transform.position;
                offset = new Vector3(offset.x, 0.0f, offset.z);
                offset = offset.normalized * .125f;

                Vector3 position = fidObject.transform.position + offset;
                List<string> coords = new List<string>();
                coords.Add(position.z.ToString());
                coords.Add((-position.x).ToString());

                // publish a go to command, to the location of block we found that matches
                commandInput.inputController.inputString = string.Format("go to {0} {1}", coords[0], coords[1]);
                commandInput.PostMessage(commandInput.inputController.inputString);
            }
        }
    }

}
