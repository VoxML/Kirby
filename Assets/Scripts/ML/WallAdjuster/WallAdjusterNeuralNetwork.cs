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

#if UNITY_EDITOR
    [CustomEditor(typeof(WallAdjusterNeuralNetwork))]
    public class WallAdjusterNeuralNetworkEditor : NeuralNetworkLearnerEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            // add other custom inspector stuff here
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
        for (int j = 0; j < Inputs.Count; j++)
        {
            Debug.Log(net.FeedForward(Inputs[j])[0]);
        }
        Debug.Log(net.FeedForward(new float[] { 0, 0, 0, 1, 0, 1, 1, 1 })[0]);
        Debug.Log(net.FeedForward(new float[] { 0, 0, 0, .9f, 0, 1, 1, 1 })[0]);
    }
}
