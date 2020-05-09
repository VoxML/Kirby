using UnityEngine;

using TMPro;

public class OutputDisplay : MonoBehaviour
{
    TextMeshProUGUI gui;

    // Start is called before the first frame update
    void Start()
    {
        gui = gameObject.GetComponent<TextMeshProUGUI>();

        if (gui == null)
        {
            Debug.LogError("OutputDisplay: could not find component TextMeshProUGUI!  Expect errors!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetText(string text)
    {
        gui.text = text;
    }

    public void Clear()
    {
        SetText(string.Empty);
    }
}
