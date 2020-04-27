using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using VoxSimPlatform.Global;

public class MapSegment
{
    public int id;
    public List<float> endpoints;
    public GameObject geom;
    public bool remove;

    public MapSegment(int _id, List<float> _endpoints, GameObject _geom)
    {
        id = _id;
        endpoints = new List<float>(_endpoints);
        geom = _geom;
        remove = false;
    }
}

public class MapUpdater : MonoBehaviour
{
    public GameObject map;

    MapUpdate curMap;
    List<MapSegment> mapSegments;

    // Start is called before the first frame update
    void Start()
    {
        curMap = new MapUpdate();
        mapSegments = new List<MapSegment>();
        map = new GameObject("Map");
    }

    // Update is called once per frame
    void Update()
    {

    }

    void LateUpdate()
    {
        for (int i = 0; i < mapSegments.Count; i++)
        {
            if (mapSegments[i].remove)
            {
                mapSegments.RemoveAt(i);
            }
        }
    }

    public void UpdateMap(MapUpdate update)
    {
        List<List<float>> updateData = new List<List<float>>(update.data);

        List<List<float>> add = new List<List<float>>();
        foreach (List<float> segment in updateData)
        {
            // if no segment in current map already matches this segment
            if (!curMap.data.Any(s => (s[0] == segment[0] && s[1] == segment[1] &&
                s[2] == segment[2] && s[3] == segment[3])))
            {
                add.Add(segment);
            }
        }

        List<MapSegment> delete = new List<MapSegment>();
        foreach (MapSegment segment in mapSegments)
        {
            // if no segment in this update matches this current map segment
            if (!updateData.Any(s => (s[0] == segment.endpoints[0] &&
                s[1] == segment.endpoints[1] && s[2] == segment.endpoints[2] &&
                s[3] == segment.endpoints[3])))
            {
                delete.Add(segment);
            }
        }

        Debug.Log(string.Format("Segments to add: {0}", string.Format("[{0}]", string.Join(",",
            add.Select(l => string.Format("[{0}]", string.Join(",", l.Select(ll => ll.ToString()))))))));
        Debug.Log(string.Format("Segments to delete: {0}", string.Format("[{0}]", string.Join(",",
            delete.Select(l => string.Format("[{0}]", string.Join(",", l.endpoints.Select(ll => ll.ToString()))))))));

        foreach (MapSegment segment in delete)
        {
            Destroy(segment.geom);
            segment.remove = true;
        }

        foreach (List<float> coordPair in add)
        {
            if (coordPair.Count != 4)
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

                // create a cube, add it to "Map" object
                GameObject wallGeom = GameObject.CreatePrimitive(PrimitiveType.Cube);

                wallGeom.name = string.Format("WallSegment{0}", System.Guid.NewGuid());

                wallGeom.transform.parent = map.transform;

                // create a new MapSegment object
                MapSegment wallSegment = new MapSegment(map.transform.childCount + 1,
                    coordPair, wallGeom);

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
                wallSegment.geom.transform.localScale = new Vector3((end - start).magnitude, 1.0f, 0.1f);

                // position it at the center of the line segment (at ground level)
                wallSegment.geom.transform.position = new Vector3((end.x + start.x) / 2.0f,
                    ((end.y + start.y) / 2.0f) + (wallSegment.geom.transform.localScale.y / 2.0f), (end.z + start.z) / 2.0f);

                // create an equivalent unit vector
                Vector3 normalized = (end - start).normalized;

                // rotate the wall segment around the Y by the arcsin of the unit vector
                //  result is in radians so convert to degrees
                float yRot = -Mathf.Asin(normalized.z) * Mathf.Sign(normalized.x) * Mathf.Rad2Deg;
                wallSegment.geom.transform.eulerAngles = new Vector3(0.0f, yRot, 0.0f);

                mapSegments.Add(wallSegment);
            }
        }

        curMap = update;
    }
}
