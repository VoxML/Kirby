using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KirbyWorldKnowledge : MonoBehaviour
{
    public Dictionary<GameObject, Vector3> objectDict;

    // declare "to find" variable
    public string toFind;

    // Start is called before the first frame update
    void Start()
    {
        // TODO: instantiate object dict (new Dictionary<,>)
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
}
