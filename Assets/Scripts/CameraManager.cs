using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Camera curMainCamera;
    public Camera curSecondCamera;
    public RenderTexture renderTextureRef;

    // Start is called before the first frame update
    void Start()
    {
        if (curMainCamera != null)
        {
            curMainCamera.depth = -1;
            curMainCamera.targetTexture = null;
            curSecondCamera.gameObject.tag = "MainCamera";
        }

        if (curSecondCamera != null)
        {
            curSecondCamera.depth = 0;
            curSecondCamera.targetTexture = renderTextureRef;
            curSecondCamera.gameObject.tag = "Camera";
        }
    }

    // Update is called once per frame
    void Update()
    {
        
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
}
