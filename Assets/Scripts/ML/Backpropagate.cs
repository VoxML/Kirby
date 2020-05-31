using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public static class Backpropagate
{
    // Backpropagation implementation
    // backpropagation
    public static void BackPropagate(this NeuralNetwork nn, float[] inputs, float[] expected)
    {
        // runs feed forward to ensure neurons are populated correctly
        float[] output = nn.FeedForward(inputs);

        nn.cost = 0;

        // calculated cost of network
        for (int i = 0; i < output.Length; i++)
        {
            nn.cost += (float)Math.Pow(output[i] - expected[i], 2);
        }

        // this value is not used in calculions, rather used to identify the performance of the network
        nn.cost = nn.cost / 2;

        // gamma initialization
        float[][] gamma;
        List<float[]> gammaList = new List<float[]>();
        for (int i = 0; i < nn.Layers.Length; i++)
        {
            gammaList.Add(new float[nn.Layers[i]]);
        }
        gamma = gammaList.ToArray();

        int layer = nn.Layers.Length - 2;

        // gamma calculation
        for (int i = 0; i < output.Length; i++)
        {
            gamma[nn.Layers.Length - 1][i] = (output[i] - expected[i]) * activateDyDx(nn, output[i], layer);
        }

        // calculates the w' and b' for the last layer in the network
        for (int i = 0; i < nn.Layers[nn.Layers.Length - 1]; i++)
        {
            nn.Biases[nn.Layers.Length - 2][i] -= gamma[nn.Layers.Length - 1][i] * nn.learningRate;
            for (int j = 0; j < nn.Layers[nn.Layers.Length - 2]; j++)
            {
                //*learning 
                nn.Weights[nn.Layers.Length - 2][i][j] -= gamma[nn.Layers.Length - 1][i] * nn.Neurons[nn.Layers.Length - 2][j] * nn.learningRate;
            }
        }

        // runs on all hidden layers
        for (int i = nn.Layers.Length - 2; i > 0; i--)
        {
            layer = i - 1;

            // outputs
            for (int j = 0; j < nn.Layers[i]; j++)
            {
                // calculate gamma
                gamma[i][j] = 0;
                for (int k = 0; k < gamma[i + 1].Length; k++)
                {
                    gamma[i][j] = gamma[i + 1][k] * nn.Weights[i][k][j];
                }
                gamma[i][j] *= activateDyDx(nn, nn.Neurons[i][j], layer);
            }

            // iterate over outputs of layer
            for (int j = 0; j < nn.Layers[i]; j++)
            {
                // modify biases of network
                nn.Biases[i - 1][j] -= gamma[i][j] * nn.learningRate;

                // iterate over inputs to layer
                for (int k = 0; k < nn.Layers[i - 1]; k++)
                {
                    // modify weights of network
                    nn.Weights[i - 1][j][k] -= gamma[i][j] * nn.Neurons[i - 1][k] * nn.learningRate;
                }
            }
        }
    }

    // all activation functions
    public static float activate(NeuralNetwork nn, float value, int layer)
    {
        switch (nn.Activations[layer])
        {
            case Activation.Sigmoid:
                return sigmoid(value);
            case Activation.Tanh:
                return tanh(value);
            case Activation.ReLU:
                return relu(value);
            case Activation.LeakyReLU:
                return leakyRelu(value);
            default:
                return relu(value);
        }
    }

    // all activation function derivatives
    public static float activateDyDx(NeuralNetwork nn, float value, int layer)
    {
        switch (nn.Activations[layer])
        {
            case Activation.Sigmoid:
                return sigmoidDyDx(value);
            case Activation.Tanh:
                return tanhDyDx(value);
            case Activation.ReLU:
                return reluDyDx(value);
            case Activation.LeakyReLU:
                return leakyReluDyDx(value);
            default:
                return reluDyDx(value);
        }
    }

    // activation functions and their corresponding derivatives
    public static float sigmoid(float x)
    {
        float k = (float)Math.Exp(x);
        return k / (1.0f + k);
    }

    public static float softmax(float[] vector, int index, float bias)
    {
        float k = (float)Math.Exp(vector[index] + bias);
        return k / vector.Select(v => (float)Math.Exp(v + bias)).Sum();
    }

    public static float tanh(float x)
    {
        return (float)Math.Tanh(x);
    }

    public static float relu(float x)
    {
        return (0 >= x) ? 0 : x;
    }

    public static float leakyRelu(float x)
    {
        return (0 >= x) ? 0.01f * x : x;
    }


    public static float sigmoidDyDx(float x)
    {
        return x * (1 - x);
    }

    public static float tanhDyDx(float x)
    {
        return 1 - (x * x);
    }

    public static float reluDyDx(float x)
    {
        return (0 >= x) ? 0 : 1;
    }

    public static float leakyReluDyDx(float x)
    {
        return (0 >= x) ? 0.01f : 1;
    }
}
