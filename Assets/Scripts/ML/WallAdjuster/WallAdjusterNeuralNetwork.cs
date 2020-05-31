using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Newtonsoft.Json;

/*
 * Action space:
 *  0 - no transformation
 *  1 - align first invariant (align, using first segment as anchor)
 *  2 - align second invariant (align, using second segment as anchor)
 *  3 - close first invariant (close gap, using first segment as anchor)
 *  4 - close second invariant (close gap, using second segment as anchor)
 */

public class WallAdjusterNeuralNetwork : NeuralNetworkLearner
{
    public string trainingDataPath;
    public List<float[]> trainingData;

    public string testingDataPath;

#if UNITY_EDITOR
    [CustomEditor(typeof(WallAdjusterNeuralNetwork))]
    public class WallAdjusterNeuralNetworkEditor : NeuralNetworkLearnerEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            // add other custom inspector stuff here
            GUILayout.BeginHorizontal();
            GUILayout.Label("Testing Data Path", GUILayout.Width(120));
            ((WallAdjusterNeuralNetwork)target).testingDataPath = GUILayout.TextField(((WallAdjusterNeuralNetwork)target).testingDataPath,
                GUILayout.MaxWidth(200));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Training Data Path", GUILayout.Width(120));
            ((WallAdjusterNeuralNetwork)target).trainingDataPath = GUILayout.TextField(((WallAdjusterNeuralNetwork)target).trainingDataPath,
                GUILayout.MaxWidth(200));
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Load Training Data"))
            {
                ((WallAdjusterNeuralNetwork)target).LoadTrainingData(((WallAdjusterNeuralNetwork)target).trainingDataPath);
            }
        }
    }
#endif

    // Start is called before the first frame update
    void Start()
    {
        layers = new int[] { 8, 16, 16, 5, 1 };
        activations = new string[] { "relu", "tanh", "tanh", "relu" };
        
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LoadTrainingData(string path)
    {
        using (var stream = new StreamReader(path))
        {
            trainingData = JsonConvert.DeserializeObject<List<float[]>>(stream.ReadToEnd());
        }

        foreach (float[] vector in trainingData)
        {
            Inputs.Add(vector.Take(8).ToArray());
            Outputs.Add(new float[] { vector.Last() });
            Debug.Log(string.Format("Added input [{0}], output [{1}]",
                string.Join(", ", Inputs.Last()), string.Join(", ", Outputs.Last())));
        }
    }

    public List<float[]> LoadTestingData(string path)
    {
        List<float[]> testingData;

        using (var stream = new StreamReader(path))
        {
            testingData = JsonConvert.DeserializeObject<List<float[]>>(stream.ReadToEnd());
        }

        return testingData;
    }

    public override void BeginTraining()
    {
        base.BeginTraining();
        for (int i = 0; i < epochs; i++)
        {
            for (int j = 0; j < Inputs.Count; j++)
            {
                net.BackPropagate(Inputs[j], Outputs[j]);
            }
            //net.BackPropagate(new float[] { 0, 0, 0 }, new float[] { 0 });
            //net.BackPropagate(new float[] { 1, 0, 0 }, new float[] { 1 });
            //net.BackPropagate(new float[] { 0, 1, 0 }, new float[] { 1 });
            //net.BackPropagate(new float[] { 0, 0, 1 }, new float[] { 1 });
            //net.BackPropagate(new float[] { 1, 1, 0 }, new float[] { 1 });
            //net.BackPropagate(new float[] { 0, 1, 1 }, new float[] { 1 });
            //net.BackPropagate(new float[] { 1, 0, 1 }, new float[] { 1 });
            //net.BackPropagate(new float[] { 1, 1, 1 }, new float[] { 1 });
            //net.BackPropagate(new float[] { 0, 0, 0, 1, 0, 1, 1, 1 }, new float[] { 1 });
            //net.BackPropagate(new float[] { 0, 0, 0, .9f, 0, 1, 1, 1 }, new float[] { 0 });
        }
    }

    public override void Test()
    {
        base.Test();

        if (!string.IsNullOrEmpty(testingDataPath))
        {
            List<float[]> testingData = LoadTestingData(testingDataPath);
            for (int j = 0; j < testingData.Count; j++)
            {
                float output = net.FeedForward(testingData[j])[0];
                Debug.Log(string.Format("[{0}] -> {1} -> {2}",
                    string.Join(", ",testingData[j]), output,
                    Mathf.RoundToInt(output)));
            }
        }
        else
        {
            float[] i1 = new float[] { 0, 0, 0, 1, 0, 1, 1, 1 };
            float[] i2 = new float[] { 0, 0, 0, .9f, 0, 1, 1, 1 };

            Debug.Log(string.Format("[{0}] -> {1} -> {2}",
                string.Join(", ", i1), net.FeedForward(i1)[0],
                Mathf.RoundToInt(net.FeedForward(i1)[0])));
            Debug.Log(string.Format("[{0}] -> {1} -> {2}",
                string.Join(", ", i2), net.FeedForward(i2)[0],
                Mathf.RoundToInt(net.FeedForward(i2)[0])));
        }
    }
}
