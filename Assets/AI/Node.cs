using System;
using UnityEngine.Assertions;

namespace AI
{
    /**
     * Represents an individual node.
     */
    public class Node
    {
        private static Random RAND = new Random();
        public double[] Weights { get; private set; }
        public double Bias { get; private set; }

        /**
         * Initializes node with given number of inputs
         * 
         * @param numInputs number of inputs
         */
        public Node(uint numInputs)
        {
            Weights = new double[numInputs];

            Bias = RandomGaussian() / Math.Sqrt(2.0 / Weights.Length);
        }

        /**
         * See www.stackoverflow.com/questions/218060/random-gaussian-variables
         * 
         * Returns the next random gaussian value with given mean and standard deviation.
         * 
         * @param mean mean of normal distribution
         * @param stdDev standard deviation of normal distribution.
         */
        private static double RandomGaussian(double mean = 0, double stdDev = 1)
        {
            double u1 = 1.0 - RAND.NextDouble();
            double u2 = 1.0 - RAND.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); // random normal (0,1)
            return mean + (stdDev * randStdNormal);
        }

        /**
         * Makes a full copy of the given weights and biases.
         * 
         * @param newWeights new weights to set
         * @param newBias new bias value to use
         */
        public void CopyValues(double[] newWeights, double newBias)
        {
            if (newWeights.Length != Weights.Length) throw new Exception();
            double[] temp = new double[newWeights.Length];
            Array.Copy(newWeights, 0, temp, 0, newWeights.Length);
            Bias = newBias;
            Weights = temp;
        }

        /**
         * Randomizes the weights and biases to new values.
         */
        public void Randomize()
        {
            for (int i = 0; i < Weights.Length; i++)
            {
                Weights[i] = RandomGaussian() / Math.Sqrt(Weights.Length);
            }
            Bias = RandomGaussian() / Math.Sqrt(2.0 / Weights.Length);
        }

        /**
         * Updates/mutates the weights and biases.
         * 
         * @param mutateChance chance of the value changing (0-1)
         * @param mutateLimit max amount the weight can change
         */
        public void Mutate(double mutateChance, double mutateLimit)
        {
            for (int i = 0; i < Weights.Length; i++)
            {
                double result = RAND.NextDouble();
                if (mutateChance > result)
                {
                    double value = Weights[i];
                    Weights[i] +=  RandBetween(-mutateLimit, mutateLimit);
                }
            }
            if (mutateChance > RAND.NextDouble())
            {
                Bias += RandBetween(-mutateLimit, mutateLimit);
            }
        }

        /**
         * Calculates the output without performing any mathematical calculation over the result.
         * 
         * @param inputs values to calculate output from
         */
        public double FastCalculateOutput(double[] inputs)
        {
            double result = 0;
            for (int i = 0; i < Weights.Length; i++) result += inputs[i] * Weights[i];
            result += Bias;
            return result;
        }

        public double CalculateOutput(double[] inputs, OutputMode om)
        {
            if (inputs.Length != Weights.Length) throw new Exception("Invalid number of inputs.");
            double result = FastCalculateOutput(inputs);
            switch (om)
            {
                case OutputMode.NONE:
                    return result;
                case OutputMode.SIGMOID:
                    return Math.Exp(result) / (1 + Math.Exp(result)); // e^result/(e^result + 1)
                case OutputMode.RELU:
                    return Math.Max(0, result);
                case OutputMode.ARCTAN:
                    return Math.Atan(result);
                default:
                    return result;
            }
        }

        // MODIFIES: this
        // EFFECTS: adjusts the seed of the random number generator to seed
        public static void SetSeed(int seed)
        {
            RAND = new Random(seed);
        }

        // EFFECTS: returns random number between min and max
        public static double RandBetween(double min, double max)
        {
            return (RAND.NextDouble() * (max - min)) + min;
        }

        // EFFECTS: makes a new node
        public Node Copy()
        {
            Node node = new Node((uint) Weights.Length);
            node.CopyValues(Weights, Bias);
            return node;
        }

        private static bool WeightsEquals(double[] weightOne, double[] weightTwo)
        {
            if (weightOne == null) return weightTwo == null;
            if (weightTwo == null) return false;
            if (weightOne.Length != weightTwo.Length) return false;
            for (int i = 0; i < weightOne.Length; i++)
            {
                if (weightOne[i] != weightTwo[i]) return false;
            }
            return true;
        }

        private static bool WeightsNotEquals(double[] weightOne, double[] weightTwo)
        {
            if (weightOne == null) return weightTwo != null;
            if (weightTwo == null) return true;
            if (weightOne.Length != weightTwo.Length) return true;
            for (int i = 0; i < weightOne.Length; i++)
            {
                if (weightOne[i] != weightTwo[i]) return true;
            }
            return false;
        }
        public override int GetHashCode()
        {
            int hashCode = base.GetHashCode();
            hashCode = 31 * hashCode + Weights.GetHashCode();
            hashCode = 31 * hashCode + Bias.GetHashCode();
            return hashCode;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Node)) return false;
            Node n = (Node)obj;
            return WeightsEquals(n.Weights,Weights) && n.Bias == Bias;
        }
    }
    /**
     * Represents the Output mode/function
     */
    public enum OutputMode
    {
        NONE, SIGMOID, RELU, ARCTAN
    }
}
