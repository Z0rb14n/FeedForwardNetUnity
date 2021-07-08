using System;

namespace AI
{
    public class BasicAIMatMul : IAI
    {
        protected NodeLayer[] layers;

        protected int outputNumber;
        protected int inputNumber;
        protected int[] HiddenLayerSizes
        {
            get {
                if (layers.Length <= 1) return new int[0];
                int[] result = new int[layers.Length - 1];
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = layers[i].numOutputs;
                }
                return result;
            }
        }

        // EFFECTS: initializes an AI with given inputNumbers, outputNumbers and hidden layer number
        public BasicAIMatMul(int inputNumber,
                             int outputNumber,
                             int[] HiddenlayerSizes,
                             Activation desiredActivation = Activation.RELU,
                             bool forceUseGPU = false)
        {
            if (outputNumber < 1) throw new ArgumentException("Invalid output number");
            layers = new NodeLayer[HiddenlayerSizes.Length + 1];
            int currentInputDim = inputNumber;
            for (int i = 0; i < layers.Length-1; i++)
            {
                layers[i] = new NodeLayer(currentInputDim, HiddenlayerSizes[i], desiredActivation, forceUseGPU);
            }
            layers[layers.Length] = new NodeLayer(currentInputDim, outputNumber, desiredActivation, forceUseGPU);
            this.outputNumber = outputNumber;
            this.inputNumber = inputNumber;
        }

        // MODIFIES: this
        // EFFECTS: randomizes the weights and biases in every node
        public void Randomize()
        {
            foreach (NodeLayer layer in layers) layer.Randomize();
        }

        // MODIFIES: this
        // EFFECTS: mutates the weights and biases
        public void Mutate(double chance, double amt)
        {
            foreach (NodeLayer layer in layers) layer.Mutate(chance, amt);
        }

        public int CalcOutputIndex(float[] inputs)
        {
            float[] result = CalculateOutputs(inputs);
            return DetermineHighestIndex(result);
        }

        public double[] CalculateOutputs(double[] inputs) => CalculateOutputs(inputs);

        // EFFECTS: calculates the output array
        public float[] CalculateOutputs(float[] inputs)
        {
            float[] nextLayerInputs = (inputs.Clone() as float[]);
            for (int i = 0; i < layers.Length; i++)
            {
                nextLayerInputs = layers[i].CalculateOutput(nextLayerInputs).Flatten();
            }
            return nextLayerInputs;
        }

        // EFFECTS: determines the highest index in given array
        public static int DetermineHighestIndex(float[] arr)
        {
            int result = -1;
            double max = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] > max)
                {
                    max = arr[i];
                    result = i;
                }
            }
            return result;
        }

        // MODIFIES: this
        // EFFECTS: copies the given nodes to AI nodes, assuming it's valid
        public void CopyNodes(NodeLayer[] layers)
        {
            if (layers.Length != this.layers.Length) throw new ArgumentException();
            this.layers = new NodeLayer[layers.Length];
            for (int i = 0; i < layers.Length; i++)
            {
                this.layers[i] = layers[i].Copy();
            }
        }

        // EFFECTS: returns exact copy of this AI
        public IAI Copy()
        {
            BasicAIMatMul ai = new BasicAIMatMul(inputNumber, outputNumber, HiddenLayerSizes);
            ai.CopyNodes(layers);
            return ai;
        }

        private static bool LayersEquals(NodeLayer[] layerOne, NodeLayer[] layerTwo)
        {
            if (layerOne == null) return layerTwo == null;
            if (layerTwo == null) return false;
            if (layerOne.Length != layerTwo.Length) return false;
            for (int i = 0; i < layerOne.Length; i++)
            {
                if (!layerOne[i].Equals(layerTwo[i])) return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return 31 * base.GetHashCode() + layers.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is BasicAIMatMul)) return false;
            BasicAIMatMul ai = obj as BasicAIMatMul;
            return LayersEquals(ai.layers, layers);
        }
    }
}