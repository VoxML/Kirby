using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VoxSimPlatform.UI.UIButtons;

/// <summary>
/// A button to clear the contents of the Redis DB
/// </summary>

public class FlushDBButton : UIButton
{
    public int fontSize = 12;

    GUIStyle buttonStyle;

    RedisInterface redis;

    // Use this for initialization
    void Start()
    {
        FontSizeModifier = (int)(fontSize / defaultFontSize);

        redis = GameObject.Find("KirbyManager").GetComponent<RedisInterface>();
        if (redis == null)
        {
            Debug.LogError("FlushDBButton.Start: Could not find RedisInterface!");
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
            redis.WriteCommand("flushdb");
            return;
        }

        base.OnGUI();
    }

    public override void DoUIButton(int buttonID)
    {
        base.DoUIButton(buttonID);
    }
}
