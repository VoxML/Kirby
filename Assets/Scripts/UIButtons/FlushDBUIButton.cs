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

    RedisPublisherManager redisPublisherManager;

    // Use this for initialization
    void Start()
    {
        FontSizeModifier = (int)(fontSize / defaultFontSize);

        redisPublisherManager = GameObject.Find("KirbyManager").GetComponent<RedisPublisherManager>();
        if (redisPublisherManager == null)
        {
            Debug.LogWarning("FlushDBButton.Start: Could not find RedisPublisherManager.  Expect errors!");
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
            redisPublisherManager.TriggerResetBridge();
            return;
        }

        base.OnGUI();
    }

    public override void DoUIButton(int buttonID)
    {
        base.DoUIButton(buttonID);
    }
}
