using System.Collections.Generic;

using Newtonsoft.Json;

public class MapUpdate
{
    [JsonProperty("id")]
    public int id { get; set; }

    [JsonProperty("width")]
    public int width { get; set; }

    [JsonProperty("height")]
    public int height { get; set; }

    [JsonProperty("resolution")]
    public float resolution { get; set; }

    [JsonProperty("data")]
    public List<List<int>> data { get; set; }

    public MapUpdate()
    {
        id = -1;
        width = 0;
        height = 0;
        resolution = 0;
        data = new List<List<int>>();
    }
}

