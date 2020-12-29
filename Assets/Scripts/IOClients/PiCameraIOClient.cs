using UnityEngine;

using VoxSimPlatform.Network;

public class PiCameraIOClient : MonoBehaviour {
    PiCameraRESTClient _piCamRestClient;

    /// <summary>
    /// The associated REST client
    /// </summary>
    public PiCameraRESTClient piCamRestClient {
        get { return _piCamRestClient; }
        set { _piCamRestClient = value; }
    }

    CommunicationsBridge commBridge;

    // Use this for initialization
    void Start() {
        commBridge = GameObject.Find("CommunicationsBridge").GetComponent<CommunicationsBridge>();
        _piCamRestClient = (PiCameraRESTClient)commBridge.FindRESTClientByLabel("PiCam");
    }

    // Update is called once per frame
    void Update() {
        if (_piCamRestClient != null) {
            string piCamUrl = string.Format("{0}:{1}", _piCamRestClient.address, _piCamRestClient.port);
            if (_piCamRestClient.isConnected) {
                if (commBridge.tryAgainRest.ContainsKey(_piCamRestClient.name)) {
                    if (commBridge.tryAgainSockets[piCamUrl] == typeof(PiCameraRESTClient)) {
                        _piCamRestClient = (PiCameraRESTClient)commBridge.FindRESTClientByLabel("PiCam");
                    }
                }
            }
            else {
                if (!commBridge.tryAgainRest.ContainsKey(piCamUrl)) {
                    commBridge.tryAgainRest.Add(piCamUrl, _piCamRestClient.GetType());
                }
            }
        }
    }

    public void Get(string route) {
        piCamRestClient.Get(route);
    }

    public void Post(string route, string content) {
        piCamRestClient.Post(route,content);
    }

    public void Put(string route, string content) {
        piCamRestClient.Put(route, content);
    }

    public void Delete(string route, string content) {
        piCamRestClient.Delete(route, content);
    }
}