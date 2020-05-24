using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/*
 * Action space:
 *  0 - none
 *  1 - align, using first segment as anchor
 *  2 - align, using second segment as anchor
 *  3 - close, using first segment as anchor
 *  4 - close, using second segment as anchor
 */

public class NeuralNetworkTest : NeuralNetworkLearner
{
#if UNITY_EDITOR
    [CustomEditor(typeof(NeuralNetworkTest))]
    public class NeuralNetworkTestEditor : NeuralNetworkLearnerEditor
    {

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
}
