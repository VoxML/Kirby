using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

using Newtonsoft.Json;

public class WallAdjusterDataGathering : MonoBehaviour
{
    public string groundTruthPath;
    sdf groundTruth;

    public string sampleMapPath;
    MapUpdate sampleMap;

#if UNITY_EDITOR
    [CustomEditor(typeof(WallAdjusterDataGathering))]
    public class WallAdjusterDataGatheringEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Ground Truth Path", GUILayout.Width(120));
            ((WallAdjusterDataGathering)target).groundTruthPath = GUILayout.TextField(((WallAdjusterDataGathering)target).groundTruthPath,
                GUILayout.MaxWidth(200));
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Load Ground Truth"))
            {
                ((WallAdjusterDataGathering)target).LoadGroundTruth(((WallAdjusterDataGathering)target).groundTruthPath);
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Sample Map Path", GUILayout.Width(120));
            ((WallAdjusterDataGathering)target).sampleMapPath = GUILayout.TextField(((WallAdjusterDataGathering)target).sampleMapPath,
                GUILayout.MaxWidth(200));
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Load Sample Map"))
            {
                ((WallAdjusterDataGathering)target).LoadSampleMap(((WallAdjusterDataGathering)target).sampleMapPath);
            }

            if (GUILayout.Button("Delete Sample Map"))
            {
                ((WallAdjusterDataGathering)target).DeleteSampleMap();
            }
        }
    }
#endif

    void LoadGroundTruth(string path)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(sdf));
        using (var stream = new FileStream(path, FileMode.Open))
        {
            groundTruth = serializer.Deserialize(stream) as sdf;
        }

        Debug.Log(groundTruth.version);
        Debug.Log(groundTruth.model.name);
        //Debug.Log(groundTruth.model.pose.frame);
        //Debug.Log(groundTruth.model.pose.value);

        for (int i = 0; i < groundTruth.model.link.Count; i++)
        {
            Debug.Log(groundTruth.model.link[i].name);
            Debug.Log(groundTruth.model.link[i].collision.name);
            Debug.Log(groundTruth.model.link[i].collision.geometry.box.size);
            Debug.Log(groundTruth.model.link[i].collision.pose.frame);
            Debug.Log(groundTruth.model.link[i].collision.pose.value);
            Debug.Log(groundTruth.model.link[i].visual.name);
            Debug.Log(groundTruth.model.link[i].visual.pose.frame);
            Debug.Log(groundTruth.model.link[i].visual.pose.value);
            Debug.Log(groundTruth.model.link[i].visual.geometry.box.size);
            Debug.Log(groundTruth.model.link[i].visual.material.script.uri);
            Debug.Log(groundTruth.model.link[i].visual.material.script.name);
            Debug.Log(groundTruth.model.link[i].visual.material.ambient);
            Debug.Log(groundTruth.model.link[i].pose.frame);
            Debug.Log(groundTruth.model.link[i].pose.value);
        }

        Debug.Log(groundTruth.model.s);

        VisualizeGroundTruthWorld(groundTruth);
    }

    void VisualizeGroundTruthWorld(sdf groundTruth)
    {
        // groundTruth.model.link[i].pose.value -> posX posZ posY radX radZ radY 
        // groundTruth.model.link[i].visual.geometry.box.size -> sizeX sizeZ sizeY

        Debug.Log(string.Format("Line count: {0}", groundTruth.model.link.Count));

        GameObject groundTruthMapObj = new GameObject("GroundTruthMap");

        for (int i = 0; i < groundTruth.model.link.Count; i++)
        {
            GameObject wallGeom = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallGeom.name = string.Format("GroundTruthSegment-{0}", i);

            wallGeom.transform.localScale = new Vector3(Convert.ToSingle(groundTruth.model.link[i].visual.geometry.box.size.Split()[0]),
                Convert.ToSingle(groundTruth.model.link[i].visual.geometry.box.size.Split()[2]),
                Convert.ToSingle(groundTruth.model.link[i].visual.geometry.box.size.Split()[1]));
            wallGeom.transform.position = new Vector3(Convert.ToSingle(groundTruth.model.link[i].pose.value.Split()[0]),
                (wallGeom.transform.localScale.y / 2.0f),
                Convert.ToSingle(groundTruth.model.link[i].pose.value.Split()[1]));
            wallGeom.transform.eulerAngles = new Vector3(Convert.ToSingle(groundTruth.model.link[i].pose.value.Split()[3]) * Mathf.Rad2Deg,
                Convert.ToSingle(groundTruth.model.link[i].pose.value.Split()[5]) * Mathf.Rad2Deg,
                Convert.ToSingle(groundTruth.model.link[i].pose.value.Split()[4]) * Mathf.Rad2Deg);

            float startX = wallGeom.transform.position.x - (wallGeom.transform.localScale.x / 2.0f);
            float endX = wallGeom.transform.position.x + (wallGeom.transform.localScale.x / 2.0f);
            float startY = wallGeom.transform.position.z - (wallGeom.transform.localScale.z / 2.0f);
            float endY = wallGeom.transform.position.z + (wallGeom.transform.localScale.z / 2.0f);

            Vector3 start = new Vector3(startX, wallGeom.transform.position.y, startY);
            Vector3 startDir = start - wallGeom.transform.position; // get point direction relative to pivot
            startDir = Quaternion.Euler(wallGeom.transform.eulerAngles) * startDir; // rotate it
            start = startDir + wallGeom.transform.position; // calculate rotated point

            GameObject startMarker = new GameObject("StartMarker");
            startMarker.transform.position = start;
            startMarker.transform.parent = wallGeom.transform;

            Vector3 end = new Vector3(endX, wallGeom.transform.position.y, endY);
            Vector3 endDir = end - wallGeom.transform.position; // get point direction relative to pivot
            endDir = Quaternion.Euler(wallGeom.transform.eulerAngles) * endDir; // rotate it
            end = endDir + wallGeom.transform.position; // calculate rotated point

            GameObject endMarker = new GameObject("EndMarker");
            endMarker.transform.position = end;
            endMarker.transform.parent = wallGeom.transform;

            Debug.Log(string.Format("Endpoints: [{0}, {1}, {2}, {3}]", start.x, start.z, end.x, end.z));

            wallGeom.transform.parent = groundTruthMapObj.transform;
        }
    }

    void LoadSampleMap(string path)
    {
        using (var stream = new StreamReader(path))
        {
            sampleMap = JsonConvert.DeserializeObject<MapUpdate>(stream.ReadToEnd()); ;
        }

        if (MapUpdate.Validate(sampleMap))
        {
            sampleMap.Log();

            VisualizeSampleMap();
        }
    }

    void VisualizeSampleMap()
    {
        GameObject sampleMapObj = new GameObject("SampleMap");
        for (int i = 0; i < sampleMap.data.Count; i++)
        {
            // first pair is start coords (X,Z)
            Vector3 start = new Vector3(sampleMap.data[i][0], 0.0f, sampleMap.data[i][1]);

            // second pair is end coords (X,Z)
            Vector3 end = new Vector3(sampleMap.data[i][2], 0.0f, sampleMap.data[i][3]);

            GameObject wallGeom = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallGeom.name = string.Format("SampleSegment-{0}", i);

            wallGeom.GetComponent<Renderer>().material.color = new Color(0, 1, 0);

            // scale it along the X-axis by the length of the line segment (and make it thin along the Z)
            wallGeom.transform.localScale = new Vector3((end - start).magnitude, 1.0f, 0.05f);

            // position it at the center of the line segment (at ground level)
            wallGeom.transform.position = new Vector3((end.x + start.x) / 2.0f,
                ((end.y + start.y) / 2.0f) + (wallGeom.transform.localScale.y / 2.0f), (end.z + start.z) / 2.0f);

            // create an equivalent unit vector
            Vector3 normalized = (end - start).normalized;

            // rotate the wall segment around the Y by the arcsin of the unit vector
            //  result is in radians so convert to degrees
            float yRot = -Mathf.Asin(normalized.z) * Mathf.Sign(normalized.x) * Mathf.Rad2Deg;
            wallGeom.transform.eulerAngles = new Vector3(0.0f, yRot, 0.0f);

            float startX = wallGeom.transform.position.x - (wallGeom.transform.localScale.x / 2.0f);
            float endX = wallGeom.transform.position.x + (wallGeom.transform.localScale.x / 2.0f);
            float startY = wallGeom.transform.position.z - (wallGeom.transform.localScale.z / 2.0f);
            float endY = wallGeom.transform.position.z + (wallGeom.transform.localScale.z / 2.0f);

            Vector3 startDir = new Vector3(startX, wallGeom.transform.position.y, startY) - wallGeom.transform.position; // get point direction relative to pivot
            startDir = Quaternion.Euler(wallGeom.transform.eulerAngles) * startDir; // rotate it
            Vector3 startPoint = startDir + wallGeom.transform.position; // calculate rotated point

            GameObject startMarker = new GameObject("StartMarker");
            startMarker.transform.position = start;
            startMarker.transform.parent = wallGeom.transform;

            Vector3 endDir = new Vector3(endX, wallGeom.transform.position.y, endY) - wallGeom.transform.position; // get point direction relative to pivot
            endDir = Quaternion.Euler(wallGeom.transform.eulerAngles) * endDir; // rotate it
            Vector3 endPoint = endDir + wallGeom.transform.position; // calculate rotated point

            GameObject endMarker = new GameObject("EndMarker");
            endMarker.transform.position = end;
            endMarker.transform.parent = wallGeom.transform;

            Debug.Log(string.Format("Endpoints: [{0}, {1}, {2}, {3}]", startPoint.x, startPoint.z, endPoint.x, endPoint.z));

            wallGeom.transform.parent = sampleMapObj.transform;
        }
    }

    void DeleteSampleMap()
    {
        GameObject sampleMapObj = GameObject.Find("SampleMap");

        if (sampleMapObj != null)
        {
            Destroy(GameObject.Find("SampleMap"));
        }
        else
        {
            Debug.Log("No sample map object!");
        }
    }
}
