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

    [JsonProperty("code")]
    public string code { get; set; }

    public LogUpdate()
    {
        level = "";
        nodeName = "";
        message = "";
        code = "";
    }

    public void Log()
    {
        Debug.Log(string.Format("Value of \"level\" in jsonObj = {0}", this.level));
        Debug.Log(string.Format("Value of \"from\" in jsonObj = {0}", this.nodeName));
        Debug.Log(string.Format("Value of \"message\" in jsonObj = {0}", this.message));
        Debug.Log(string.Format("Value of \"code\" in jsonObj = {0}", this.code));
    }

    public static bool Validate(LogUpdate log)
    {
        return (!((log.level == "") && (log.nodeName == "") &&
            (log.message == "") && (log.code == "")));
    }
}
