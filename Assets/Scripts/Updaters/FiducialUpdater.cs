using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VoxSimPlatform.Vox;

public class FiducialUpdater : MonoBehaviour
{
    public List<Material> presetMaterials;

    public GameObject fiducials;

    RedisPublisherManager manager;

    // TODO: declare "objects of interest" (type ObjectsOfInterest)

    VoxemeInit voxemeInit;

    bool inited = false;

    // Start is called before the first frame update
    void Start()
    {
        fiducials = new GameObject("Fiducials");
        manager = gameObject.GetComponent<RedisPublisherManager>();

        voxemeInit = GameObject.Find("VoxWorld").GetComponent<VoxemeInit>();

        // TODO: Find KirbyManager object and get ObjectsOfInterestComponent
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DatabaseFlushed()
    {
        Debug.Log("FiducialUpdater: picked up message DatabaseFlushed");
        if (!inited)
        {
            if (manager.publishers[manager.fiducialKey].usingRejson)
            {
                manager.publishers[manager.fiducialKey].WriteArrayCommand(string.Format("json.get {0}",
                    string.Format("{0}/{1}", manager.namespacePrefix, manager.fiducialKey)));
            }
            else
            {
                manager.publishers[manager.fiducialKey].WriteArrayCommand(string.Format("get {0}",
                    string.Format("{0}/{1}", manager.namespacePrefix, manager.fiducialKey)));
            }
        }
    }

    public void UpdateFiducial(FiducialUpdate update)
    {
        for (int i = 0; i < update.fidCount; i++)
        {
            GameObject fidObj = null;
            try
            {
                fidObj = fiducials.transform.Find(string.Format("Fiducial_{0}", update.data[i].fid)).gameObject;
            }
            catch (Exception ex)
            {
                if (ex is NullReferenceException)
                {
                    Debug.Log(string.Format("No fiducial object with ID {0} found.  Creating new fiducial object.", i));
                }
            }

            if (fidObj != null)
            {
                Voxeme fidObjVox = fidObj.GetComponent<Voxeme>();

                if (fidObjVox != null)
                {
                    // fiducial object with this ID already exists
                    Vector3 coords = new Vector3(-update.data[i].pose.location[1],
                        update.data[i].pose.location[2], update.data[i].pose.location[0]);

                    Vector3 rot = new Vector3(update.data[i].pose.orientation[1] * Mathf.Rad2Deg,
                        update.data[i].pose.location[0] * Mathf.Rad2Deg, update.data[i].pose.location[2] * Mathf.Rad2Deg);

                    if (update.frame == "odom")
                    {
                        fidObjVox.targetPosition = coords;
                    }
                    else if (update.frame == "camera")
                    {

                    }
                    fidObjVox.targetRotation = coords;
                }
                else
                {
                    Debug.LogWarningFormat("FiducialUpdater.UpdateFiducial: {0} has no voxeme component!", fidObj.name);
                }
            }
            else
            {
                // create a new fiducial object
                fidObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                fidObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

                Vector3 coords = new Vector3(-update.data[i].pose.location[1],
                    update.data[i].pose.location[2], update.data[i].pose.location[0]);

                Vector3 rot = new Vector3(update.data[i].pose.orientation[1] * Mathf.Rad2Deg,
                    update.data[i].pose.location[0] * Mathf.Rad2Deg, update.data[i].pose.location[2] * Mathf.Rad2Deg);

                if (update.frame == "odom")
                {
                    fidObj.transform.position = coords;
                }
                else if (update.frame == "camera")
                {

                }
                fidObj.transform.eulerAngles = rot;

                if (presetMaterials.Count > update.data[i].fid)
                {
                    fidObj.GetComponent<Renderer>().material = presetMaterials[update.data[i].fid-1];
                }

                fidObj.name = string.Format("Fiducial_{0}", update.data[i].fid);

                fidObj.transform.parent = fiducials.transform;

                // add voxeme
                fidObj.AddComponent<Voxeme>();

                // TODO: add to ObjectsOfInterest objects dictionary
                // key: fidObj, value fidObj.transform.position

                // reinitialize voxemes
                voxemeInit.InitializeVoxemes();
            }
        }
    }
}
