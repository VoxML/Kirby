using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VoxSimPlatform.UI.UIButtons;

/// <summary>
/// A button to switch the two camera buttons
/// </summary>

public class SwitchCamerasUIButton : UIButton
{
    public int fontSize = 12;

    GUIStyle buttonStyle;

    CameraManager cameraManager;

    // Use this for initialization
    void Start()
    {
        FontSizeModifier = (int)(fontSize / defaultFontSize);

        cameraManager = GameObject.Find("CameraManager").GetComponent<CameraManager>();
        if (cameraManager == null)
        {
            Debug.LogError("SwitchCamerasUIButton.Start: Could not find CameraManager.  Expect errors!");
        }

        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
    }

    protected override void OnGUI()
    {
        buttonStyle = new GUIStyle("Button");
        buttonStyle.fontSize = fontSize;

        if (GUI.Button(buttonRect, buttonText, buttonStyle))
        {
            cameraManager.SwitchCameraViews();
            return;
        }

        base.OnGUI();
    }

    public override void DoUIButton(int buttonID)
    {
        base.DoUIButton(buttonID);
    }
}
