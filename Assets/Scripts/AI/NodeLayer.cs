using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MatrixOp;
using UnityEngine;
using Random = System.Random;
namespace AI
{
    public enum Activation
    {
        NONE, SIGMOID, RELU, ARCTAN
    }
    public static class ExtensionMethods
    {
        public static double[] Flatten(this double[,] A)
        {
            int dim0 = A.GetLength(0);
            int dim1 = A.GetLength(1);
            double[] result = new double[dim0 * dim1];
            for (int i = 0; i < dim0; i++)
            {
                for (int j = 0; j < dim1; j++)
                {
                    result[i * dim1 + j] = A[i, j];
                }
            }
            return result;
        }
        public static float[] Flatten(this float[,] A)
        {
            int dim0 = A.GetLength(0);
            int dim1 = A.GetLength(1);
            float[] result = new float[dim0 * dim1];
            for (int i = 0; i < dim0; i++)
            {
                for (int j = 0; j< dim1; j++)
                {
                    result[i * dim1 + j] = A[i, j];
                }
            }
            return result;
        }

        public static double[,] Unflatten(this double[] A)
        {
            double[,] result = new double[A.Length, 1];
            for (int i = 0; i < A.Length; i++)
            {
                result[i, 0] = A[i];
            }
            return result;
        }
        public static float[,] Unflatten(this float[] A)
        {
            float[,] result = new float[A.Length, 1];
            for (int i = 0; i < A.Length; i++)
            {
                result[i, 0] = A[i];
            }
            return result;
        }
        public static bool MatrixNotEquals(this float[,] A, float[,] B) => !A.MatrixEquals(B);
        public static bool MatrixEquals(this float[,] A, float[,] B)
        {
            int dim0 = A.GetLength(0);
            int dim1 = A.GetLength(1);
            int dim2 = B.GetLength(0);
            int dim3 = B.GetLength(1);
            if (dim0 != dim2 && dim1 != dim3) return false;
            for (int i = 0; i < dim0; i++)
            {
                for (int j = 0; j < dim1; j++)
                {
                    if (A[i, j] != B[i, j]) return false;
                }
            }
            return true;
        }
        /// <summary>
        /// Gets a random gaussian value
        /// </summary>
        /// <param name="rand">Random number generator</param>
        /// <param name="mean">Mean, default is 0</param>
        /// <param name="stdDev">Standard deviation, default is 1</param>
        /// <see href="www.stackoverflow.com/questions/218060/random-gaussian-variables"/>
        /// <returns>Next random gaussian value with given mean and standard deviation</returns>
        public static float NextRandomGaussian(this Random rand, double mean = 0, double stdDev = 1)
        {
            double u1 = 1.0 - rand.NextDouble();
            double u2 = 1.0 - rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); // random normal (0,1)
            return (float)(mean + (stdDev * randStdNormal));
        }

        public static float NextDoubleBetween(this Random rand, double min, double max)
        {
            return (float)((rand.NextDouble() * (max - min)) + min);
        }

        public static float[,] PerformActivation(this Activation activation, float[,] input)
        {
            int dim0 = input.GetLength(0);
            int dim1 = input.GetLength(1);
            switch (activation)
            {
                case Activation.SIGMOID:
                    for (int i = 0; i < dim0; i++)
                    {
                        for (int j = 0; j < dim1; j++) input[i,j]= (float)(1 / (1 + Math.Exp(-input[i,j])));
                    }
                    break;
                case Activation.RELU:
                    for (int i = 0; i < dim0; i++)
                    {
                        for (int j = 0; j < dim1; j++) input[i, j] = Math.Min(input[i, j], 0);
                    }
                    break;
                case Activation.ARCTAN:
                    for (int i = 0; i < dim0; i++)
                    {
                        for (int j = 0; j < dim1; j++) input[i, j] = (float)Math.Atan(input[i,j]);
                    }
                    break;
            }
            return input;
        }
    }

    public class NodeLayer
    {
        public static int Seed
        {
            set => RAND = new Random(value);
        }
        protected static Random RAND = new Random();
        protected float[,] weights = new float[0, 0];
        public ComputeShader MatMulShader;
        public Activation ActivationFunction;
        public bool UseGPU = false;
        public int GPUCutoff = 128;
        public int numOutputs { get; protected set; }
        public int numInputs { get; protected set; }
        // simulate bias yourself by adding another input of 1s
        public NodeLayer(int numInputs, int numOutputs, Activation activation = Activation.RELU, bool forceUseGPU = false)
        {
            ActivationFunction = activation;
            UseGPU = numInputs >= GPUCutoff || forceUseGPU;
            weights = new float[numInputs, numOutputs];
            this.numOutputs = numOutputs;
            this.numInputs = numInputs;
        }

        public void CopyValues(float[,] newWeights)
        {
            if (weights.GetLength(0) != newWeights.GetLength(0) || weights.GetLength(1) != weights.GetLength(1)) throw new ArgumentException();
            weights = newWeights.Clone() as float[,];
        }

        public void Randomize()
        {
            for (int i = 0; i < weights.GetLength(0); i++)
            {
                for (int j = 0; j < weights.GetLength(1); j++)
                {
                    weights[i, j] = RAND.NextRandomGaussian(0, 1 / Math.Sqrt(weights.Length));
                }
            }
        }

        public virtual float[,] CalculateOutput(float[] inputs)
        {
            if (inputs.Length != numInputs) throw new ArgumentException();
            float[,] inputsUnFlattened = inputs.Unflatten();
            float[,] result;
            if (UseGPU) result = GPUMatMul.Multiply(MatMulShader, inputsUnFlattened, weights);
            else result = CPUMatMul.Multiply(inputsUnFlattened, weights);
            return ActivationFunction.PerformActivation(result);
        }

        public void Mutate(double mutateChance, double mutateLimit)
        {
            for (int i = 0; i < weights.GetLength(0); i++)
            {
                for (int j = 0; j < weights.GetLength(1); j++)
                {
                    double result = RAND.NextDouble();
                    if (mutateChance > result)
                    {
                        weights[i, j] += RAND.NextDoubleBetween(-mutateLimit, mutateLimit);
                    }
                }
            }
        }

        public virtual NodeLayer Copy()
        {
            NodeLayer temp = new NodeLayer(numInputs, numOutputs, ActivationFunction, UseGPU);
            temp.CopyValues(weights);
            return temp;
        }

        public override int GetHashCode()
        {
            return (UseGPU, numInputs, numOutputs, ActivationFunction, weights).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is NodeLayer)) return false;
            NodeLayer layer = obj as NodeLayer;
            if (layer.UseGPU != UseGPU) return false;
            if (numInputs != layer.numInputs || numOutputs != layer.numOutputs) return false;
            if (ActivationFunction != layer.ActivationFunction) return false;
            return (obj as NodeLayer).weights.MatrixEquals(weights);
        }
    }
}
