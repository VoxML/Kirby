using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
}

