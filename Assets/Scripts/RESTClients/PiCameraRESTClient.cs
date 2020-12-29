using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

using VoxSimPlatform.Network;

public class PiCameraRESTClient : RESTClient
{
    /// <summary>
    /// Name to match, that's about it.
    /// </summary>
    public PiCameraRESTClient()
    {
        clientType = typeof(PiCameraIOClient);
    }

    public override IEnumerator AsyncRequest(string payload, string method, string url, string success, string error)
    {
        webRequest = new UnityWebRequest(url.StartsWith("http://") ? url + "/cur_frame" :
            "http://" + url + "/cur_frame", method); // route is specific page as directed by server
        var payloadBytes = string.IsNullOrEmpty(payload)
            ? Encoding.UTF8.GetBytes("{}")
            : Encoding.UTF8.GetBytes(payload);

        UploadHandler upload = new UploadHandlerRaw(payloadBytes);
        webRequest.uploadHandler = upload;
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");

        webRequest.SendWebRequest();
        int count = 0; // Try several times before failing
        while (count < 20)
        { // 2 seconds max is good? Probably.
            yield return new WaitForSeconds((float)0.1); // Totally sufficient
            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.LogWarning("Some sort of network error: " + webRequest.error + " from " + url);
            }
            else
            {
                // Show results as text            
                if (webRequest.downloadHandler.text != "")
                {
                    //Debug.Log("Server took " + count * 0.1 + " seconds");
                    break;
                }
            }
            count++;
        }
        // look for response method in this class first
        // then if null, see if one has been implemented in the base class
        if (webRequest.isNetworkError)
        {
            MethodInfo responseMethod = GetType().GetMethod(error);

            if (responseMethod != null)
            {
                responseMethod.Invoke(this, new object[] { webRequest.error });
            }
            else
            {
                throw new NullReferenceException();
            }
        }
        else if (webRequest.responseCode < 200 || webRequest.responseCode >= 400)
        {
            MethodInfo responseMethod = GetType().GetMethod(error);

            if (responseMethod != null)
            {
                responseMethod.Invoke(this, new object[] { webRequest.downloadHandler.text });
            }
            else
            {
                throw new NullReferenceException();
            }
        }
        else
        {
            MethodInfo responseMethod = GetType().GetMethod(success);

            if (responseMethod != null)
            {
                responseMethod.Invoke(this, new object[] { webRequest.downloadHandler.text });
            }
            else
            {
                throw new NullReferenceException();
            }
        }
    }
}