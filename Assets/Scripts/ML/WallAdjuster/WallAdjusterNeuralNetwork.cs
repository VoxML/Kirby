using UnityEngine;
using UnityEditor;
using System.IO;
using System.Xml.Serialization;

/*
 * Action space:
 *  0 - none
 *  1 - align, using first segment as anchor
 *  2 - align, using second segment as anchor
 *  3 - close, using first segment as anchor
 *  4 - close, using second segment as anchor
 */

public class WallAdjusterNeuralNetwork : NeuralNetworkLearner
{
    public string groundTruthPath;

    sdf groundTruth;

#if UNITY_EDITOR
    [CustomEditor(typeof(WallAdjusterNeuralNetwork))]
    public class WallAdjusterNeuralNetworkEditor : NeuralNetworkLearnerEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Ground Truth Path", GUILayout.Width(120));
            ((WallAdjusterNeuralNetwork)target).groundTruthPath = GUILayout.TextField(((WallAdjusterNeuralNetwork)target).groundTruthPath,
                GUILayout.MaxWidth(200));
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Load Ground Truth"))
            {
                ((WallAdjusterNeuralNetwork)target).LoadGroundTruth(((WallAdjusterNeuralNetwork)target).groundTruthPath);
            }
        }
    }
#endif

    // Start is called before the first frame update
    void Start()
    {
        layers = new int[4] { 8, 16, 8, 1 };
        activations = new string[3] { "leakyRelu", "leakyRelu", "leakyRelu" };
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void BeginTraining()
    {
        base.BeginTraining();
        for (int i = 0; i < epochs; i++)
        {
            //net.BackPropagate(new float[] { 0, 0, 0 }, new float[] { 0 });
            //net.BackPropagate(new float[] { 1, 0, 0 }, new float[] { 1 });
            //net.BackPropagate(new float[] { 0, 1, 0 }, new float[] { 1 });
            //net.BackPropagate(new float[] { 0, 0, 1 }, new float[] { 1 });
            //net.BackPropagate(new float[] { 1, 1, 0 }, new float[] { 1 });
            //net.BackPropagate(new float[] { 0, 1, 1 }, new float[] { 1 });
            //net.BackPropagate(new float[] { 1, 0, 1 }, new float[] { 1 });
            //net.BackPropagate(new float[] { 1, 1, 1 }, new float[] { 1 });
            net.BackPropagate(new float[] { 0, 0, 0, 1, 0, 1, 1, 1 }, new float[] { 1 });
            net.BackPropagate(new float[] { 0, 0, 0, .9f, 0, 1, 1, 1 }, new float[] { 0 });
        }
    }

    public override void Test()
    {
        base.Test();
        Debug.Log(net.FeedForward(new float[] { 0, 0, 0, 1, 0, 1, 1, 1 })[0]);
        Debug.Log(net.FeedForward(new float[] { 0, 0, 0, .9f, 0, 1, 1, 1 })[0]);
        //Debug.Log(net.FeedForward(new float[] { 0, 0, 0 })[0]);
        //Debug.Log(net.FeedForward(new float[] { 1, 0, 0 })[0]);
        //Debug.Log(net.FeedForward(new float[] { 0, 1, 0 })[0]);
        //Debug.Log(net.FeedForward(new float[] { 0, 0, 1 })[0]);
        //Debug.Log(net.FeedForward(new float[] { 1, 1, 0 })[0]);
        //Debug.Log(net.FeedForward(new float[] { 0, 1, 1 })[0]);
        //Debug.Log(net.FeedForward(new float[] { 1, 0, 1 })[0]);
        //Debug.Log(net.FeedForward(new float[] { 1, 1, 1 })[0]);
        //We want the gate to simulate 3 input or gate (A or B or C)
        // 0 0 0    => 0
        // 1 0 0    => 1
        // 0 1 0    => 1
        // 0 0 1    => 1
        // 1 1 0    => 1
        // 0 1 1    => 1
        // 1 0 1    => 1
        // 1 1 1    => 1
    }

    void LoadGroundTruth(string path)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(sdf));
        using (var stream = new FileStream(path, FileMode.Open))
        {
            groundTruth = serializer.Deserialize(stream) as sdf;
        }

        Debug.Log(groundTruth.version);
        Debug.Log(groundTruth.model.name);
        Debug.Log(groundTruth.model.pose.frame);
        Debug.Log(groundTruth.model.pose.value);

        for (int i = 0; i < groundTruth.model.link.Count; i++)
        {
            Debug.Log(groundTruth.model.link[i].name);
            Debug.Log(groundTruth.model.link[i].collision.name);
            Debug.Log(groundTruth.model.link[i].collision.geometry.box.size);
            Debug.Log(groundTruth.model.link[i].collision.pose.frame);
            Debug.Log(groundTruth.model.link[i].collision.pose.value);
            Debug.Log(groundTruth.model.link[i].visual.name);
            Debug.Log(groundTruth.model.link[i].visual.pose.frame);
            Debug.Log(groundTruth.model.link[i].visual.pose.value);
            Debug.Log(groundTruth.model.link[i].visual.geometry.box.size);
            Debug.Log(groundTruth.model.link[i].visual.material.script.uri);
            Debug.Log(groundTruth.model.link[i].visual.material.script.name);
            Debug.Log(groundTruth.model.link[i].visual.material.ambient);
            Debug.Log(groundTruth.model.link[i].pose.frame);
            Debug.Log(groundTruth.model.link[i].pose.value);
        }

        Debug.Log(groundTruth.model.s);

        VisualizeGroundTruthWorld(groundTruth);
    }

    void VisualizeGroundTruthWorld(sdf groundTruth)
    {
        // groundTruth.model.link[i].pose.value -> posX posZ posY radX radZ radY 
        // groundTruth.model.link[i].visual.geometry.box.size -> sizeX sizeZ sizeY

        for (int i = 0; i < groundTruth.model.link.Count; i++)
        {
            GameObject wallGeom = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallGeom.transform.localScale = new Vector3(System.Convert.ToSingle(groundTruth.model.link[i].visual.geometry.box.size.Split()[0]),
                System.Convert.ToSingle(groundTruth.model.link[i].visual.geometry.box.size.Split()[2]),
                System.Convert.ToSingle(groundTruth.model.link[i].visual.geometry.box.size.Split()[1]));
            wallGeom.transform.position = new Vector3(System.Convert.ToSingle(groundTruth.model.link[i].pose.value.Split()[0]),
                System.Convert.ToSingle(groundTruth.model.link[i].pose.value.Split()[2]),
                System.Convert.ToSingle(groundTruth.model.link[i].pose.value.Split()[1]));
            wallGeom.transform.eulerAngles = new Vector3(System.Convert.ToSingle(groundTruth.model.link[i].pose.value.Split()[3]) * Mathf.Rad2Deg,
                System.Convert.ToSingle(groundTruth.model.link[i].pose.value.Split()[5]) * Mathf.Rad2Deg,
                System.Convert.ToSingle(groundTruth.model.link[i].pose.value.Split()[4]) * Mathf.Rad2Deg);
        }
    }
}
