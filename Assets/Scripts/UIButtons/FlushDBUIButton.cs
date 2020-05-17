using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VoxSimPlatform.UI.UIButtons;

/// <summary>
/// A button to clear the contents of the Redis DB
/// </summary>

public class FlushDBUIButton : UIButton
{
    public int fontSize = 12;

    GUIStyle buttonStyle;

    RedisPublisher redisPublisher;

    // Use this for initialization
    void Start()
    {
        FontSizeModifier = (int)(fontSize / defaultFontSize);

        redisPublisher = GameObject.Find("KirbyManager").GetComponent<RedisPublisher>();
        if (redisPublisher == null)
        {
            Debug.LogError("FlushDBButton.Start: Could not find RedisPublisher!");
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
            // clear all redis keys
            redisPublisher.ResetBridge();
            return;
        }

        base.OnGUI();
    }

    public override void DoUIButton(int buttonID)
    {
        base.DoUIButton(buttonID);
    }
}
