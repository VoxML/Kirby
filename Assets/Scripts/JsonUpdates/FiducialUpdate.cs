using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

/// <summary>
/// This class represents updates to the fiducials (objects of interest) received from Kirby
/// </summary>

/*
 *  {
 *      "fid_count": 1,
 *      "frame": "odom",
 *      "dict": 7,
 *      "data": [{
 *          "pose": {
 *              "location": [0.5841248162492049, 0.001350950778304289, 0.1153394747832589],
 *              "orientation": [1.5614911244084855, -0.00026252152137503856, -1.5692386470299342]
 *          }, 
 *          "fid": "1"
 *      }]
 *  }  
 */

public class FiducialPose
{
    [JsonProperty("location")]
    public List<float> location { get; set; }

    [JsonProperty("orientation")]
    public List<float> orientation { get; set; }
}

public class FiducialData
{
    [JsonProperty("pose")]
    public FiducialPose pose { get; set; }

    [JsonProperty("fid")]
    public int fid { get; set; }

    public FiducialData()
    {
        pose = new FiducialPose();
        fid = -1;
    }
}

public class FiducialUpdate
{
    [JsonProperty("fid_count")]
    public int fidCount { get; set; }

    [JsonProperty("frame")]
    public string frame { get; set; }

    [JsonProperty("dict")]
    public int dict { get; set; }

    [JsonProperty("data")]
    public List<FiducialData> data { get; set; }

    public FiducialUpdate()
    {
        fidCount = -1;
        frame = string.Empty;
        dict = -1;
        data = new List<FiducialData>();
    }

    public void Log()
    {
        // print the contents of the map update to the console
        Debug.Log(string.Format("Value of \"fid_count\" in jsonObj = {0}", this.fidCount));
        Debug.Log(string.Format("Value of \"frame\" in jsonObj = {0}", this.frame));
        Debug.Log(string.Format("Value of \"dict\" in jsonObj = {0}", this.dict));
        Debug.Log(string.Format("Value of \"data\" in jsonObj = {0}", string.Format("[{0}]",
            string.Join(",", this.data.Select(l => string.Format("{{ pose : {0}, fid : {1} }",
                string.Format("{{ location : {0}, orientation : {1} }", l.pose.location, l.pose.orientation), l.fid))))));
    }

    public static bool Validate(FiducialUpdate fid)
    {
        return (!((fid.fidCount == -1) && (string.IsNullOrEmpty(fid.frame)) &&
            (fid.dict == -1) && (fid.data.Count == 0)));
    }
}

