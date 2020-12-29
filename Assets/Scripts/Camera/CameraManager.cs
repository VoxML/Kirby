using System;
using System.Timers;
using UnityEngine;
using UnityEngine.UI;

using VoxSimPlatform.Global;
using VoxSimPlatform.Network;

public class CameraManager : MonoBehaviour
{
    [Serializable]
    class PiCamPixels
    {
        public PiCamPixels(byte[] _bytes)
        {
            bytes = _bytes;
        }

        public byte[] bytes;
    }

    public Camera curMainCamera;
    public Camera curSecondCamera;
    public RenderTexture renderTextureRef;

    bool showPiCamView;
    public bool ShowPiCamView
    {
        get { return showPiCamView; }
        set { showPiCamView = value; }
    }

    public int piCamRefreshTime;
    public Vector2 piCamResolution;

    public bool verboseDebug;

    CommunicationsBridge commBridge;
    PiCameraRESTClient piCam;

    bool gotPiStream;
    bool refreshPiCam;

    Timer piCamRefreshTimer;

    GameObject piCamCanvasObj;
    RawImage piCamRawImage;
    Texture2D piCamTexture;

    // Start is called before the first frame update
    void Start()
    {
        commBridge = GameObject.Find("CommunicationsBridge").GetComponent<CommunicationsBridge>();
        if (curMainCamera != null)
        {
            curMainCamera.depth = -1;
            curMainCamera.targetTexture = null;
            curMainCamera.gameObject.tag = "MainCamera";
        }

        if (curSecondCamera != null)
        {
            curSecondCamera.depth = 0;
            curSecondCamera.targetTexture = renderTextureRef;
            curSecondCamera.gameObject.tag = "Camera";
        }

        gotPiStream = false;
        showPiCamView = true;
        piCam = (PiCameraRESTClient)commBridge.FindRESTClientByLabel("PiCam");

        if (piCam != null)
        {
            piCam.GetError += StreamError;
            piCam.GetOkay += GetImageFrame;
            piCamRefreshTimer = new Timer(piCamRefreshTime);
            piCamRefreshTimer.Enabled = true;
            piCamRefreshTimer.Elapsed += RefreshPiCam;

            piCamCanvasObj = GameObject.Find("PiCamViewCanvas");

            if (piCamCanvasObj != null)
            {
                piCamTexture = new Texture2D((int)piCamResolution.x, (int)piCamResolution.y, TextureFormat.RGB24, false);
                piCamRawImage = GameObject.Find("PiCamRawImage").GetComponent<RawImage>();
                showPiCamView = true;
            }
            else
            {
                Debug.Log("Couldn't find Pi camera view canvas object!");
                showPiCamView = false;
            }
        }
        else
        {
            Debug.Log("Couldn't get PiCam!");
            showPiCamView = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (piCam != null)
        {
            if (showPiCamView)
            {
                if (refreshPiCam)
                {
                    refreshPiCam = false;
                    StartCoroutine(piCam.Get(""));
                }
            }

            if (piCamCanvasObj != null)
            {
                piCamCanvasObj.SetActive(showPiCamView);
            }
        }
    }

    public void SwitchCameraViews()
    {
        var newMainCameraDepth = curSecondCamera.depth;
        var newMainCameraTargetTexture = curSecondCamera.targetTexture;
        var newMainCameraTag = curSecondCamera.gameObject.tag;
        var newMainCamera = curSecondCamera;

        curSecondCamera.depth = curMainCamera.depth;
        curSecondCamera.targetTexture = curMainCamera.targetTexture;
        curSecondCamera.gameObject.tag = curMainCamera.gameObject.tag;
        curSecondCamera = curMainCamera;

        curMainCamera.depth = newMainCameraDepth;
        curMainCamera.targetTexture = newMainCameraTargetTexture;
        curMainCamera.gameObject.tag = newMainCameraTag;
        curMainCamera = newMainCamera;
    }

    public void StreamError(object sender, EventArgs e)
    {
        if (!gotPiStream && showPiCamView)  // error before first successful get
        {                                   //  probably means we have no connection
            showPiCamView = false;  // may as well shut it off
            Debug.Log("Can't get PiCam stream!  Maybe the streaming client is not active?  " +
                "Shutting off stream viewer.");
        }
    }

    public void GetImageFrame(object sender, EventArgs e)
    {
        object content = ((RESTEventArgs)e).Content;
        try
        {
            PiCamPixels dict = JsonUtility.FromJson<PiCamPixels>(content.ToString());

            if (verboseDebug)
            {
                Debug.Log(string.Format("Got EventArgs: size {0}", dict.bytes.Length));
            }

            Debug.Log(GlobalHelper.PrintByteArray(dict.bytes));

            Texture2D texture = new Texture2D(320, 240, TextureFormat.RGB24, false);
            texture.LoadImage(dict.bytes);
            texture.Apply();
            RenderPiCamView(texture);
            gotPiStream = true; // we've received and processed at least one frame
        }
        catch (Exception ex)
        {
            if (verboseDebug)
            {
                Debug.LogWarning(ex);
                if (ex is ArgumentException)
                {
                    Debug.LogWarning(string.Format("Badly formed input: {0}", content));
                }
            }
        }
    }

    public void RenderPiCamView(Texture2D texture)
    {
        piCamRawImage.texture = texture;
    }

    void RefreshPiCam(object sender, ElapsedEventArgs e)
    {
        refreshPiCam = true;
    }
}
