using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VoxSimPlatform.Global;
using VoxSimPlatform.Vox;

public class RoboUpdater : MonoBehaviour
{
    public GameObject kirby;

    Voxeme kirbyVox;

    bool inited = false;

    // Start is called before the first frame update
    void Start()
    {
    
    }

    // Update is called once per frame
    void Update()
    {
        if (!inited)
        {
            kirby = GlobalHelper.GetMostImmediateParentVoxeme(kirby);
            kirbyVox = kirby.GetComponent<Voxeme>();
        }
    }

    public void UpdateRobot(RoboUpdate update)
    {
        Vector3 targetPos = new Vector3(update.location[0], kirbyVox.transform.position.y, update.location[1]);
        targetPos = Quaternion.Euler(0.0f, 90.0f, 0.0f) * targetPos;
        kirbyVox.targetPosition = targetPos;
    }
}
