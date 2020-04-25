using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MapUpdater : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateMap(MapUpdate update)
    {
        foreach (List<float> coordPair in update.data)
        {
            if (coordPair.Count < 4)
            {
                Debug.Log(string.Format("Badly formed input: [{0}]",
                    string.Join(",", update.data.Select(f => f.ToString()))));
            }
            else
            {
                // first pair is start coords (X,Z)
                Vector3 start = new Vector3(coordPair[0], 0.0f, coordPair[1]);

                // second pair is end coords (X,Z)
                Vector3 end = new Vector3(coordPair[2], 0.0f, coordPair[3]);

                // create a cube
                GameObject wallSegment = GameObject.CreatePrimitive(PrimitiveType.Cube);

                // example using [2.0, 0.0, 0.0, -2.0] line segment
                // start: (2.0, 0.0, 0.0)
                // end: (0.0,0.0,-2.0)
                // end - start: (-2.0,0.0,-2.0)
                // (end - start).magnitude: √8 ≈ 2.828
                // wallSegment.transform.localScale ≈ (2.828, 1.0, 0,1)
                // wallSegment.transform.position = (-2.0/2, 0.0/2 + 1.0/2, -2.0/2) = (-1.0, 0.5, -1.0)
                // normalized = (-1/√2, 0, -1/√2) ≈ (-0.707, 0, -0.707)
                // arcsin(-0.707) ≈ -45 degrees
                // -45 * sign(-0.707) = 45 degrees

                // scale it along the X-axis by the length of the line segment (and make it thin along the Z)
                wallSegment.transform.localScale = new Vector3((end - start).magnitude, 1.0f, 0.1f);

                // position it at the center of the line segment (at ground level)
                wallSegment.transform.position = new Vector3((end.x + start.x) / 2.0f,
                    ((end.y + start.y) / 2.0f) + (wallSegment.transform.localScale.y / 2.0f), (end.z + start.z) / 2.0f);

                // create an equivalent unit vector
                Vector3 normalized = (end - start).normalized;

                // rotate the wall segment around the Y by the arcsin of the unit vector
                //  result is in radians so convert to degrees
                float yRot = -Mathf.Asin(normalized.z) * Mathf.Sign(normalized.x) * Mathf.Rad2Deg;
                wallSegment.transform.eulerAngles = new Vector3(0.0f, yRot, 0.0f);
            }
        }
    }
}
