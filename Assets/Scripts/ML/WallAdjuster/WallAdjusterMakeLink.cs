using UnityEditor;
using UnityEngine;
using System.Linq;

public class WallAdjusterMakeLink
{
    static void Log()
    {
        Debug.Log(string.Format("Selection: {0}, {1}",
                (Selection.objects[0] as GameObject).name,
                (Selection.objects[1] as GameObject).name));
    }

    /// <summary>
    /// No transformation
    /// </summary>
    [MenuItem("Make Link/No Transformation &#0")]
    static void NoTransformation() 
    {
        if (Selection.objects.All(o => o is GameObject))
        {
            Log();
            Vector3 firstStart = (Selection.objects[0] as GameObject).
            transform.Find("StartMarker").transform.position;
            Vector3 firstEnd = (Selection.objects[0] as GameObject).
                transform.Find("EndMarker").transform.position;
            Vector3 secondStart = (Selection.objects[1] as GameObject).
                transform.Find("StartMarker").transform.position;
            Vector3 secondEnd = (Selection.objects[1] as GameObject).
                transform.Find("EndMarker").transform.position;
            Debug.Log(string.Format("[{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, 0]",
                firstStart.x, firstStart.z, firstEnd.x, firstEnd.z,
                secondStart.x, secondStart.z, secondEnd.x, secondEnd.z));
        }
    }

    [MenuItem("Make Link/No Transformation &#0", true)]
    static bool ValidateNoTransformation()
    {
        return (Selection.objects.Length == 2);
    }

    /// <summary>
    /// Align, first invariant
    /// </summary>
    [MenuItem("Make Link/Align First Invariant &#1")]
    static void AlignFirstInvariant()
    {
        if (Selection.objects.All(o => o is GameObject))
        {
            Log();
            Vector3 firstStart = (Selection.objects[0] as GameObject).
                transform.Find("StartMarker").transform.position;
            Vector3 firstEnd = (Selection.objects[0] as GameObject).
                transform.Find("EndMarker").transform.position;
            Vector3 secondStart = (Selection.objects[1] as GameObject).
                transform.Find("StartMarker").transform.position;
            Vector3 secondEnd = (Selection.objects[1] as GameObject).
                transform.Find("EndMarker").transform.position;

            Debug.Log(string.Format("[{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, 1]",
                firstStart.x, firstStart.z, firstEnd.x, firstEnd.z,
                secondStart.x, secondStart.z, secondEnd.x, secondEnd.z));
        }
    }

    [MenuItem("Make Link/Align First Invariant &#1", true)]
    static bool ValidateAlignFirstInvariant()
    {
        return (Selection.objects.Length == 2);
    }

    /// <summary>
    /// Aligns, second invariant
    /// </summary>
    [MenuItem("Make Link/Align Second Invariant &#2")]
    static void AlignSecondInvariant()
    {
        if (Selection.objects.All(o => o is GameObject))
        {
            Log();
            Vector3 firstStart = (Selection.objects[0] as GameObject).
                transform.Find("StartMarker").transform.position;
            Vector3 firstEnd = (Selection.objects[0] as GameObject).
                transform.Find("EndMarker").transform.position;
            Vector3 secondStart = (Selection.objects[1] as GameObject).
                transform.Find("StartMarker").transform.position;
            Vector3 secondEnd = (Selection.objects[1] as GameObject).
                transform.Find("EndMarker").transform.position;

            Debug.Log(string.Format("[{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, 2]",
                firstStart.x, firstStart.z, firstEnd.x, firstEnd.z,
                secondStart.x, secondStart.z, secondEnd.x, secondEnd.z));
        }
    }

    [MenuItem("Make Link/Align Second Invariant &#2", true)]
    static bool ValidateAlignSecondInvariant()
    {
        return (Selection.objects.Length == 2);
    }

    /// <summary>
    /// Close gap, first invariant
    /// </summary>
    [MenuItem("Make Link/Close First Invariant &#3")]
    static void CloseFirstInvariant()
    {
        if (Selection.objects.All(o => o is GameObject))
        {
            Log();
            Vector3 firstStart = (Selection.objects[0] as GameObject).
                transform.Find("StartMarker").transform.position;
            Vector3 firstEnd = (Selection.objects[0] as GameObject).
                transform.Find("EndMarker").transform.position;
            Vector3 secondStart = (Selection.objects[1] as GameObject).
                transform.Find("StartMarker").transform.position;
            Vector3 secondEnd = (Selection.objects[1] as GameObject).
                transform.Find("EndMarker").transform.position;

            Debug.Log(string.Format("[{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, 3]",
                firstStart.x, firstStart.z, firstEnd.x, firstEnd.z,
                secondStart.x, secondStart.z, secondEnd.x, secondEnd.z));
        }
    }

    [MenuItem("Make Link/Close First Invariant &#3", true)]
    static bool ValidateCloseFirstInvariant()
    {
        return (Selection.objects.Length == 2);
    }

    /// <summary>
    /// Close gap, second invariant
    /// </summary>
    [MenuItem("Make Link/Close Second Invariant &#4")]
    static void CloseSecondInvariant()
    {
        if (Selection.objects.All(o => o is GameObject))
        {
            Log();
            Vector3 firstStart = (Selection.objects[0] as GameObject).
                transform.Find("StartMarker").transform.position;
            Vector3 firstEnd = (Selection.objects[0] as GameObject).
                transform.Find("EndMarker").transform.position;
            Vector3 secondStart = (Selection.objects[1] as GameObject).
                transform.Find("StartMarker").transform.position;
            Vector3 secondEnd = (Selection.objects[1] as GameObject).
                transform.Find("EndMarker").transform.position;

            Debug.Log(string.Format("[{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, 4]",
                firstStart.x, firstStart.z, firstEnd.x, firstEnd.z,
                secondStart.x, secondStart.z, secondEnd.x, secondEnd.z));
        }
    }

    [MenuItem("Make Link/Close Second Invariant &#4", true)]
    static bool ValidateCloseSecondInvariant()
    {
        return (Selection.objects.Length == 2);
    }
}
