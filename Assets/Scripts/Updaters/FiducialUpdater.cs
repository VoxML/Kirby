using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VoxSimPlatform.Global;
using VoxSimPlatform.Vox;

public class FiducialUpdater : MonoBehaviour
{
    public List<Material> presetMaterials;

    public GameObject fiducials;

    RedisPublisherManager manager;

    KirbyWorldKnowledge objects;

    VoxemeInit voxemeInit;

    bool inited = false;

    // Start is called before the first frame update
    void Start()
    {
        fiducials = new GameObject("Fiducials");
        fiducials.layer = LayerMask.NameToLayer("Objects");
        manager = gameObject.GetComponent<RedisPublisherManager>();

        voxemeInit = GameObject.Find("VoxWorld").GetComponent<VoxemeInit>();

        objects = gameObject.GetComponent<KirbyWorldKnowledge>();
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
                manager.publishers[manager.fiducialKey].WriteCommand(string.Format("json.get \"{0}\"",
                    string.Format("{0}/{1}", manager.namespacePrefix, manager.fiducialKey)));
            }
            else
            {
                manager.publishers[manager.fiducialKey].WriteCommand(string.Format("get \"{0}\"",
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
                    Debug.Log(string.Format("No fiducial object with ID {0} found.", i));
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
                Debug.Log("Creating new fiducial object.");
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
                fidObj.layer = fiducials.layer;

                // add voxeme
                fidObj.AddComponent<Voxeme>();

                // reinitialize voxemes
                Debug.Log("Reinitializing voxemes.");
                voxemeInit.InitializeVoxemes();

                // add to ObjectsOfInterest objects dictionary
                // key: fidObj, value fidObj.transform.position
                objects.objectDict.Add(fidObj, fidObj.transform.position);
                string s = "";
                foreach (KeyValuePair<GameObject, Vector3> kvp in objects.objectDict)
                {
                    s += string.Format("Key = {0}, Value = {1}\n",
                        kvp.Key.name, GlobalHelper.VectorToParsable(kvp.Value));
                }
                Debug.Log("Known objects dictionary content:" + s);

                // TODO:
                // in KirbyWorldKnowledge, create a method that assesses if the
                //  thing we just added to known objects matches the characteristics
                //  of the "to find" variable (see KirbyWorldKnowledge)
                // this method should take fidObj as an input and be typed void
            }
        }
    }
}
