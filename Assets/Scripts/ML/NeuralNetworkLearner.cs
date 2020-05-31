using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

public class NeuralNetworkLearner : MonoBehaviour
{
    public NeuralNetwork net;
    public int[] layers;
    public string[] activations;

    public int epochs;
    public float learningRate;
    public float cost;
    public string savePath;

    DateTime trainStartTime;

    List<float[]> inputs;
    public List<float[]> Inputs
    {
        get { return inputs; }
        set { inputs = value; }
    }

    List<float[]> outputs;
    public List<float[]> Outputs
    {
        get { return outputs; }
        set { outputs = value; }
    }

    public static event EventHandler TrainingDataLoaded;

    public static void OnTrainingDataLoaded(object sender, EventArgs e)
    {
        if (TrainingDataLoaded != null)
        {
            TrainingDataLoaded(sender, e);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(NeuralNetworkLearner))]
    public class NeuralNetworkLearnerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Epochs", GUILayout.Width(120));
            ((NeuralNetworkLearner)target).epochs = Convert.ToInt32(GUILayout.TextField(((NeuralNetworkLearner)target).epochs.ToString(),
                GUILayout.MaxWidth(200)));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Learning Rate", GUILayout.Width(120));
            ((NeuralNetworkLearner)target).learningRate = Convert.ToSingle(GUILayout.TextField(((NeuralNetworkLearner)target).learningRate.ToString(),
                GUILayout.MaxWidth(200)));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Cost", GUILayout.Width(120));
            ((NeuralNetworkLearner)target).cost = Convert.ToSingle(GUILayout.TextField(((NeuralNetworkLearner)target).cost.ToString(),
                GUILayout.MaxWidth(200)));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Save Path", GUILayout.Width(120));
            ((NeuralNetworkLearner)target).savePath = GUILayout.TextField(((NeuralNetworkLearner)target).savePath,
                GUILayout.MaxWidth(200));
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Save Model"))
            {
                ((NeuralNetworkLearner)target).SaveNet();
            }

            if (GUILayout.Button("Train and Test"))
            {
                ((NeuralNetworkLearner)target).BeginTraining();
                ((NeuralNetworkLearner)target).EndTraining();
                ((NeuralNetworkLearner)target).Test();
            }
        }
    }
#endif

    public virtual void Start()
    {
        net = new NeuralNetwork(layers, activations, learningRate, cost);

        inputs = new List<float[]>();
        outputs = new List<float[]>();
    }

    public virtual void BeginTraining()
    {
        trainStartTime = DateTime.Now;
        Debug.Log(string.Format("===== Training started ===== {0}", trainStartTime.ToString("F")));
    }

    public virtual void EndTraining()
    {
        Debug.Log(string.Format("===== Training finished ===== {0}; training took {1} seconds", DateTime.Now.ToString("F"),
            (DateTime.Now-trainStartTime).Seconds.ToString()));
    }

    public virtual void Test()
    {
        Debug.Log(string.Format("===== Test started ===== {0}", DateTime.Now.ToString("F")));
        Debug.Log(string.Format("Cost: {0}", net.cost));
    }

    void SaveNet()
    {
        net.Save(savePath);
    }
}
