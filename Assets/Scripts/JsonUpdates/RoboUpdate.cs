using System.Collections.Generic;

using Newtonsoft.Json;

/// <summary>
/// This class represents updates to Kirby's odometry
/// </summary>

public class RoboUpdate
{
    [JsonProperty("odom_id")]
    public int odomId { get; set; }

    [JsonProperty("time")]
    public int time { get; set; }

    [JsonProperty("location")]
    public List<float> location { get; set; }

    [JsonProperty("orientation")]
    public List<float> orientation { get; set; }

    [JsonProperty("linearvelocity")]
    public float linearVelocity { get; set; }

    [JsonProperty("angularvelocity")]
    public float angularVelocity { get; set; }

    public RoboUpdate()
    {
        odomId = -1;
        time = 0;
        location = new List<float>;
        orientation = new List<float>;
        linearVelocity = 0.0f;
        angularVelocity = 0.0f;
    }
}

