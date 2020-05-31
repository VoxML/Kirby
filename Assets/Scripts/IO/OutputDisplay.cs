using UnityEngine;

using System.Timers;

using TMPro;

public enum TextDisplayMode
{
    Default,
    Persistent
}

public class OutputDisplay : MonoBehaviour
{
    TextMeshProUGUI gui;

    Timer displayTimeoutTimer;
    public int displayTimeoutTime;

    bool clear = false;

    // Start is called before the first frame update
    void Start()
    {
        gui = gameObject.GetComponent<TextMeshProUGUI>();

        if (gui == null)
        {
            Debug.LogWarning("OutputDisplay: could not find component TextMeshProUGUI!  Expect errors!");
        }

        displayTimeoutTimer = new Timer(displayTimeoutTime);
        displayTimeoutTimer.Enabled = false;
        displayTimeoutTimer.Elapsed += TimeoutDisplay;
    }

    // Update is called once per frame
    void Update()
    {
        if (clear)
        {
            Clear();
            clear = false;
        }
    }

    public void SetText(string text, TextDisplayMode displayMode = TextDisplayMode.Default)
    {
        gui.text = text;

        if (displayMode == TextDisplayMode.Default)
        {
            displayTimeoutTimer.Enabled = true;
        }
        else if (displayMode == TextDisplayMode.Persistent)
        {
            displayTimeoutTimer.Enabled = false;
            displayTimeoutTimer.Interval = displayTimeoutTime;
        }
    }

    public void Clear()
    {
        SetText(string.Empty);
    }

    void TimeoutDisplay(object sender, ElapsedEventArgs e)
    {
        displayTimeoutTimer.Enabled = false;
        displayTimeoutTimer.Interval = displayTimeoutTime;
        clear = true;
    }
}
