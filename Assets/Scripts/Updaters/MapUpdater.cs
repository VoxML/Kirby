using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

using VoxSimPlatform.Global;
using TMPro;

public class MapSegment
{
    public Guid guid;
    public List<float> endpoints;
    public GameObject geom;
    public bool remove;

    public MapSegment(Guid _guid, List<float> _endpoints, GameObject _geom)
    {
        guid = _guid;
        endpoints = new List<float>(_endpoints);
        geom = _geom;
        remove = false;
    }
}

public class MapUpdater : MonoBehaviour
{
    public event EventHandler MapInited;

    public void OnMapInited(object sender, EventArgs e)
    {
        if (MapInited != null)
        {
            MapInited(this, e);
        }
    }

    public GameObject map;

    public bool keepRosCoords;

    RedisPublisherManager manager;
    OutputDisplay outputDisplay;

    MapUpdate curMap;
    List<MapSegment> mapSegments;

    bool inited = false;

    // Start is called before the first frame update
    void Start()
    {
        curMap = new MapUpdate();
        mapSegments = new List<MapSegment>();
        map = new GameObject("Map");

        manager = gameObject.GetComponent<RedisPublisherManager>();

        //TODO: route this through VoxSim OutputController
        outputDisplay = GameObject.Find("OutputDisplay").GetComponent<OutputDisplay>();
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

    public void DatabaseFlushed()
    {
        Debug.Log("MapUpdater: picked up message DatabaseFlushed");
        if (!inited)
        {
            outputDisplay.SetText("Waiting for Map...", TextDisplayMode.Persistent);
            if (manager.publishers[manager.mapKey].usingRejson)
            {
                manager.publishers[manager.mapKey].WriteArrayCommand(string.Format("json.lpop {0}",
                    string.Format("{0}/{1}", manager.namespacePrefix, manager.mapKey)));
            }
            else
            {
                manager.publishers[manager.mapKey].WriteArrayCommand(string.Format("lpop {0}",
                    string.Format("{0}/{1}", manager.namespacePrefix, manager.mapKey)));
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
                // get coordinates from map update instance

                Vector3 start, end;
                if (keepRosCoords)
                {
                    // first pair is start coords (X,Z)
                    start = new Vector3(coordPair[0], 0.0f, coordPair[1]);

                    // second pair is end coords (X,Z)
                    end = new Vector3(coordPair[2], 0.0f, coordPair[3]);
                }
                else
                {
                    // need to transform from Redis +X forward space to Unity +Z forward space
                    // do this by flipping the X and Z and coordinates and reflecting horizontally

                    // first pair is start coords (X,Z)
                    start = new Vector3(-coordPair[1], 0.0f, coordPair[0]);

                    // second pair is end coords (X,Z)
                    end = new Vector3(-coordPair[3], 0.0f, coordPair[2]);
                }

                // create a cube, add it to "Map" object
                GameObject wallGeom = GameObject.CreatePrimitive(PrimitiveType.Cube);

                Guid guid = Guid.NewGuid();
                wallGeom.name = string.Format("WallSegment-{0}", guid);

                wallGeom.transform.parent = map.transform;

                // create a new MapSegment object
                MapSegment wallSegment = new MapSegment(guid, coordPair, wallGeom);

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
                wallSegment.geom.transform.localScale = new Vector3((end - start).magnitude, 1.0f, 0.05f);

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

        if (!inited)
        {
            outputDisplay.Clear();
            inited = true;
            OnMapInited(this, null);
        }
    }
}
