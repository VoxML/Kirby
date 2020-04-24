using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using VoxSimPlatform.Global;

using Newtonsoft.Json;

public class MapUpdate
{
    [JsonProperty("id")]
    public int id { get; set; }

    [JsonProperty("width")]
    public float width { get; set; }

    [JsonProperty("height")]
    public float height { get; set; }

    [JsonProperty("resolution")]
    public float resolution { get; set; }

    [JsonProperty("data")]
    public List<List<float>> data { get; set; }

    public MapUpdate()
    {
        id = -1;
        width = 0.0f;
        height = 0.0f;
        resolution = 0;
        data = new List<List<float>>();
    }

    public void Log()
    {
        Debug.Log(string.Format("Value of \"id\" in jsonObj = {0}", this.id));
        Debug.Log(string.Format("Value of \"width\" in jsonObj = {0}", this.width));
        Debug.Log(string.Format("Value of \"height\" in jsonObj = {0}", this.height));
        Debug.Log(string.Format("Value of \"resolution\" in jsonObj = {0}", this.resolution));
        Debug.Log(string.Format("Value of \"data\" in jsonObj = {0}", string.Format("[{0}]", string.Join(",",
            this.data.Select(l => string.Format("[{0}]", string.Join(",", l.Select(ll => ll.ToString()))))))));
    }


    public void Interpret()
    {
        foreach (List<float> coordPair in data)
        {
            if (coordPair.Count < 4)
            {
                Debug.Log(string.Format("Badly formed input: [{0}]",
                    string.Join(",", data.Select(f => f.ToString()))));
            }
            else 
            {
                // first pair is start coords (X,Z)
                Vector3 start = new Vector3(coordPair[0], 0.0f, coordPair[1]);
                Debug.Log(string.Format("start = {0}", GlobalHelper.VectorToParsable(start)));

                // second pair is end coords (X,Z)
                Vector3 end = new Vector3(coordPair[2], 0.0f, coordPair[3]);
                Debug.Log(string.Format("end = {0}", GlobalHelper.VectorToParsable(end)));

                GameObject wallSegment = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wallSegment.transform.localScale = new Vector3((end - start).magnitude, 1.0f, 0.1f);
                wallSegment.transform.position = new Vector3((end.x + start.x) / 2.0f,
                    ((end.y + start.y) / 2.0f) + (wallSegment.transform.localScale.y / 2.0f), (end.z + start.z) / 2.0f);
                //wallSegment.transform.eulerAngles = Vector3.zero;

                Vector3 normalized = (end - start).normalized;
                Debug.Log(string.Format("normalized = {0}", GlobalHelper.VectorToParsable(normalized)));

                //Quaternion rotation = Quaternion.Euler(0.0f, Mathf.Asin(normalized.z) * Mathf.Rad2Deg, 0.0f);
                Debug.Log(string.Format("arcsin = {0}", Mathf.Asin(normalized.z)));
                Debug.Log(string.Format("arcsin*Rad2Def = {0}", Mathf.Asin(normalized.z) * Mathf.Rad2Deg));

                wallSegment.transform.eulerAngles = new Vector3(0.0f, Mathf.Asin(normalized.z) * Mathf.Rad2Deg, 0.0f);
                Debug.Log(string.Format("eulerAngles = {0}", GlobalHelper.VectorToParsable(wallSegment.transform.eulerAngles)));
                Debug.Log(string.Format("position = {0}", GlobalHelper.VectorToParsable(wallSegment.transform.position)));

                /*
                    [4.8, 0.2, 5.0, 0.25]

                    (4.8 + 5.0)/2 = 4.9
                    (0.2 + 0.25)/2 = 0.225
                  
                    5.0-4.8 = 0.2
                    0.25-0.2 = 0.05

                    normalized (0.970, 0.243)

                */
            }
        }
    }
}

