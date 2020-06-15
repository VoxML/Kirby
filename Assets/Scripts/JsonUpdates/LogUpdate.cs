using System;
using Newtonsoft.Json;
using UnityEngine;

public class LogUpdate
{
    [JsonProperty("level")]
    public string level { get; set; }

    [JsonProperty("from")]
    public string nodeName { get; set; }

    [JsonProperty("message")]
    public string message { get; set; }

    public LogUpdate()
    {
        level = "";
        nodeName = "";
        message = "";
    }

    public void Log()
    {
        Debug.Log(string.Format("Value of \"level\" in jsonObj = {0}", this.level));
        Debug.Log(string.Format("Value of \"from\" in jsonObj = {0}", this.nodeName));
        Debug.Log(string.Format("Value of \"message\" in jsonObj = {0}", this.message));
    }

    public static bool Validate(LogUpdate log)
    {
        return (!((log.level == "") && (log.nodeName == "") &&
            (log.message == "")));
    }
}
