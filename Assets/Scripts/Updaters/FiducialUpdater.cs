using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FiducialUpdater : MonoBehaviour
{
    public List<Material> presetMaterials;

    public GameObject fiducials;

    // Start is called before the first frame update
    void Start()
    {
        fiducials = new GameObject("Fiducials");
    }

    // Update is called once per frame
    void Update()
    {
        
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
                // fiducial object with this ID already exists
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

                if (presetMaterials.Count > i)
                {
                    fidObj.GetComponent<Renderer>().material = presetMaterials[i];
                }

                fidObj.name = string.Format("Fiducial_{0}", update.data[i].fid);

                fidObj.transform.parent = fiducials.transform;
            }
        }
    }
}
