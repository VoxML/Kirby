using UnityEngine;
using UnityEditor;
using System;

public class NeuralNetworkManager : MonoBehaviour
{
    NeuralNetwork net;
    int[] layers = new int[3] { 8, 12, 1 };
    string[] activations = new string[2] { "leakyRelu", "leakyRelu" };

    public int epochs;
    public float learningRate;
    public float cost;
    public string savePath;

#if UNITY_EDITOR
    [CustomEditor(typeof(NeuralNetworkManager))]
    public class NeuralNetworkManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Epochs", GUILayout.Width(120));
            ((NeuralNetworkManager)target).epochs = Convert.ToInt32(GUILayout.TextField(((NeuralNetworkManager)target).epochs.ToString(),
            GUILayout.MaxWidth(200)));
            GUILayout.EndHorizontal();

              GUILayout.BeginHorizontal();
            GUILayout.Label("Learning Rate", GUILayout.Width(120));
            ((NeuralNetworkManager)target).learningRate = Convert.ToSingle(GUILayout.TextField(((NeuralNetworkManager)target).learningRate.ToString(),
            GUILayout.MaxWidth(200)));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Cost", GUILayout.Width(120));
            ((NeuralNetworkManager)target).cost = Convert.ToSingle(GUILayout.TextField(((NeuralNetworkManager)target).cost.ToString(),
            GUILayout.MaxWidth(200)));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Save Path", GUILayout.Width(120));
            ((NeuralNetworkManager)target).savePath = GUILayout.TextField(((NeuralNetworkManager)target).savePath,
            GUILayout.MaxWidth(200));
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Train and Test"))
            {
                ((NeuralNetworkManager)target).RunTest();
            }

            if (GUILayout.Button("Save"))
            {
                ((NeuralNetworkManager)target).SaveNet();
            }
        }
    }
#endif

    void Start()
    {
        net = new NeuralNetwork(layers, activations, learningRate, cost);
    }

    void RunTest()
    {
        Debug.Log(string.Format("===== New test ===== {0}", DateTime.Now.ToString("F")));
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

        Debug.Log(string.Format("Cost: {0}", net.cost));

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

    void SaveNet()
    {
        net.Save(savePath);
    }
}
