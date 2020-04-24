using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using VoxSimPlatform.Global;

using Newtonsoft.Json;

/// <summary>
/// This class represents updates to the world map received from Kirby
/// </summary>

public class MapUpdate
{
    [JsonProperty("id")]
    public int id { get; set; }

    [JsonProperty("line_count")]
    public int lineCount { get; set; }

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
        lineCount = 0;
        width = 0.0f;
        height = 0.0f;
        resolution = 0;
        data = new List<List<float>>();
    }

    public void Log()
    {
        // print the contents of the map update to the console
        Debug.Log(string.Format("Value of \"id\" in jsonObj = {0}", this.id));
        Debug.Log(string.Format("Value of \"line_count\" in jsonObj = {0}", this.lineCount));
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

                // second pair is end coords (X,Z)
                Vector3 end = new Vector3(coordPair[2], 0.0f, coordPair[3]);

                // create a cube
                GameObject wallSegment = GameObject.CreatePrimitive(PrimitiveType.Cube);

                // example using [2.0, 0.0, 0.0, -2.0] line segment
                // start: (2.0, 0.0, 0.0)
                // end: (0.0,0.0,-2.0)
                // end - start: (-2.0,0.0,-2.0)
                // (end - start).magnitude: √8 ≈ 2.828
                // wallSegment.transform.localScale ≈ (2.828, 1.0, 0,1)
                // wallSegment.transform.position = (-2.0/2, 0.0/2 + 1.0/2, -2.0/2) = (-1.0, 0.5, -1.0)
                // normalized = (-1/√2, 0, -1/√2) ≈ (-0.707, 0, -0.707)
                // arcsin(-0.707) ≈ -45 degrees
                // -45 * sign(-0.707) = 45 degrees

                // scale it along the X-axis by the length of the line segment (and make it thin along the Z)
                wallSegment.transform.localScale = new Vector3((end - start).magnitude, 1.0f, 0.1f);

                // position it at the center of the line segment (at ground level)
                wallSegment.transform.position = new Vector3((end.x + start.x) / 2.0f,
                    ((end.y + start.y) / 2.0f) + (wallSegment.transform.localScale.y / 2.0f), (end.z + start.z) / 2.0f);

                // create an equivalent unit vector
                Vector3 normalized = (end - start).normalized;

                // rotate the wall segment around the Y by the arcsin of the unit vector
                //  result is in radians so convert to degrees
                float yRot = -Mathf.Asin(normalized.z) * Mathf.Sign(normalized.x) * Mathf.Rad2Deg;
                wallSegment.transform.eulerAngles = new Vector3(0.0f, yRot, 0.0f);
            }
        }
    }
}

