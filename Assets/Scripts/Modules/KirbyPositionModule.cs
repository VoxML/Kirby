using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KirbyPositionModule : ModuleBase
{
    public GameObject kirby;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (kirby == null)
        {
            return;
        }

        SetValue("kirby:position", kirby.transform.position, string.Empty);
        SetValue("kirby:orientation", kirby.transform.eulerAngles, string.Empty);
    }
}
