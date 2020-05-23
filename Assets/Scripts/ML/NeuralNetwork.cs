/*
 * Native Unity neural network implementation
 * derived from https://github.com/kipgparker/BackPropNetwork
 * TODO: if this works, move it to VoxSimPlatform
 */

using UnityEngine;
using System.Collections.Generic;
using System.IO;

using VoxSimPlatform.Global;

public enum Activation
{
    Sigmoid,
    Tanh,
    ReLU,
    LeakyReLU
}

public class NeuralNetwork
{
    // fundamental 
    private int[] layers;
    public int[] Layers
    {
        get { return layers; }
    }

    private float[][] neurons;
    public float[][] Neurons
    {
        get { return neurons; }
    }

    private float[][] biases;
    public float[][] Biases
    {
        get { return biases; }
    }

    private float[][][] weights;
    public float[][][] Weights
    {
        get { return weights; }
    }

    private Activation[] activations;
    public Activation[] Activations
    {
        get { return activations; }
    }

    // genetic
    public float fitness = 0;

    // backprop
    public float learningRate;
    public float cost;

    private float[][] deltaBiases;
    private float[][][] deltaWeights;
    private int deltaCount;

    public NeuralNetwork(int[] layers, string[] layerActivations, float _learningRate, float _cost)
    {
        learningRate = _learningRate;
        cost = _cost;

        this.layers = new int[layers.Length];
        for (int i = 0; i < layers.Length; i++)
        {
            this.layers[i] = layers[i];
        }

        activations = new Activation[layers.Length - 1];
        for (int i = 0; i < layers.Length - 1; i++)
        {
            string action = layerActivations[i];
            switch (action)
            {
                case "sigmoid":
                    activations[i] = Activation.Sigmoid;
                    break;
                case "tanh":
                    activations[i] = Activation.Tanh;
                    break;
                case "relu":
                    activations[i] = Activation.ReLU;
                    break;
                case "leakyRelu":
                    activations[i] = Activation.LeakyReLU;
                    break;
                default:
                    activations[i] = Activation.ReLU;
                    break;
            }
        }

        InitNeurons();
        InitBiases();
        InitWeights();
    }

    //create empty storage array for the neurons in the network
    private void InitNeurons()
    {
        List<float[]> neuronsList = new List<float[]>();
        for (int i = 0; i < layers.Length; i++)
        {
            neuronsList.Add(new float[layers[i]]);
        }
        neurons = neuronsList.ToArray();
    }

    //initializes random array for the biases being held within the network
    private void InitBiases()
    {
        List<float[]> biasList = new List<float[]>();
        for (int i = 1; i < layers.Length; i++)
        {
            float[] bias = new float[layers[i]];
            for (int j = 0; j < layers[i]; j++)
            {
                bias[j] = RandomHelper.RandomFloat(-0.5f, 0.5f, 0);
            }
            biasList.Add(bias);
        }
        biases = biasList.ToArray();
    }

    // initializes random array for the weights being held in the network
    private void InitWeights()
    {
        List<float[][]> weightsList = new List<float[][]>();
        for (int i = 1; i < layers.Length; i++)
        {
            List<float[]> layerWeightsList = new List<float[]>();
            int neuronsInPreviousLayer = layers[i - 1];
            for (int j = 0; j < layers[i]; j++)
            {
                float[] neuronWeights = new float[neuronsInPreviousLayer];
                for (int k = 0; k < neuronsInPreviousLayer; k++)
                {
                    neuronWeights[k] = RandomHelper.RandomFloat(-0.5f, 0.5f, 0);
                }
                layerWeightsList.Add(neuronWeights);
            }
            weightsList.Add(layerWeightsList.ToArray());
        }
        weights = weightsList.ToArray();
    }

    // feed forward, inputs >==> outputs
    public float[] FeedForward(float[] inputs)
    {
        for (int i = 0; i < inputs.Length; i++)
        {
            neurons[0][i] = inputs[i];
        }
        for (int i = 1; i < layers.Length; i++)
        {
            int layer = i - 1;
            for (int j = 0; j < layers[i]; j++)
            {
                float value = 0f;
                for (int k = 0; k < layers[i - 1]; k++)
                {
                    value += weights[i - 1][j][k] * neurons[i - 1][k];
                }
                neurons[i][j] = Backpropagate.activate(this, value + biases[i-1][j], layer);
            }
        }
        return neurons[layers.Length-1];
    }

    // For creating a deep copy, to ensure arrays are serialized
    public NeuralNetwork Copy(NeuralNetwork nn) 
    {
        for (int i = 0; i < biases.Length; i++)
        {
            for (int j = 0; j < biases[i].Length; j++)
            {
                nn.biases[i][j] = biases[i][j];
            }
        }
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    nn.weights[i][j][k] = weights[i][j][k];
                }
            }
        }
        return nn;
    }

    // save and load functions
    // this loads the biases and weights from within a file into the neural network
    public void Load(string path)
    {
        TextReader tr = new StreamReader(path);
        int NumberOfLines = (int)new FileInfo(path).Length;
        string[] ListLines = new string[NumberOfLines];
        int index = 1;
        for (int i = 1; i < NumberOfLines; i++)
        {
            ListLines[i] = tr.ReadLine();
        }
        tr.Close();
        if (new FileInfo(path).Length > 0)
        {
            for (int i = 0; i < biases.Length; i++)
            {
                for (int j = 0; j < biases[i].Length; j++)
                {
                    biases[i][j] = float.Parse(ListLines[index]);
                    index++;
                }
            }

            for (int i = 0; i < weights.Length; i++)
            {
                for (int j = 0; j < weights[i].Length; j++)
                {
                    for (int k = 0; k < weights[i][j].Length; k++)
                    {
                        weights[i][j][k] = float.Parse(ListLines[index]); ;
                        index++;
                    }
                }
            }
        }
    }

    // this is used for saving the biases and weights within the network to a file
    public void Save(string path)
    {
        File.Create(path).Close();
        StreamWriter writer = new StreamWriter(path, true);

        for (int i = 0; i < biases.Length; i++)
        {
            for (int j = 0; j < biases[i].Length; j++)
            {
                writer.WriteLine(biases[i][j]);
            }
        }

        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    writer.WriteLine(weights[i][j][k]);
                }
            }
        }
        writer.Close();

        Debug.Log(string.Format("Network saved to file \"{0}\""));
    }

    //Genetic implementations down onwards until save.
    //used as a simple mutation function for any genetic implementations.
    public void Mutate(int high, float val)
    {
        for (int i = 0; i < biases.Length; i++)
        {
            for (int j = 0; j < biases[i].Length; j++)
            {
                biases[i][j] = (UnityEngine.Random.Range(0f, high) <= 2) ? biases[i][j] += UnityEngine.Random.Range(-val, val) : biases[i][j];
            }
        }

        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    weights[i][j][k] = (UnityEngine.Random.Range(0f, high) <= 2) ? weights[i][j][k] += UnityEngine.Random.Range(-val, val) : weights[i][j][k];
                }
            }
        }
    }

    //Comparing For Genetic implementations. Used for sorting based on the fitness of the network
    public int CompareTo(NeuralNetwork other)
    {
        if (other == null) return 1;

        if (fitness > other.fitness)
            return 1;
        else if (fitness < other.fitness)
            return -1;
        else
            return 0;
    }
}
