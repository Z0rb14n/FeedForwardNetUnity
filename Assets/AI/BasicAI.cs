using System;

namespace AI
{
    public class BasicAI
    {
        protected Node[][] layers; // Node[layer number from left-1][node from top]

        protected uint outputNumber;
        protected uint inputNumber;
        protected uint hiddenLayerNumber;

        // EFFECTS: initializes an AI with given inputNumbers, outputNumbers and hidden layer number
        public BasicAI(uint inputNumber, uint outputNumber, uint hiddenLayerNumber)
        {
            if (outputNumber < 1) throw new Exception("Invalid output number");
            this.hiddenLayerNumber = hiddenLayerNumber;
            layers = new Node[hiddenLayerNumber + 1][];
            for (int i = 0; i < layers.Length; i++)
            {
                if (i == layers.Length - 1)
                {
                    layers[i] = new Node[outputNumber];
                }
                else
                {
                    layers[i] = new Node[inputNumber];
                }
                for (int j = 0; j < layers[i].Length; j++)
                {
                    layers[i][j] = new Node(inputNumber);
                }
            }
            this.outputNumber = outputNumber;
            this.inputNumber = inputNumber;
        }

        // MODIFIES: this
        // EFFECTS: randomizes the weights and biases in every node
        public void Randomize()
        {
            foreach (Node[] node in layers)
            {
                foreach (Node n in node)
                {
                    n.Randomize();
                }
            }
        }

        // MODIFIES: this
        // EFFECTS: mutates the weights and biases
        public void Mutate(double chance, double amt)
        {
            foreach (Node[] nodeLayer in layers)
            {
                foreach (Node node in nodeLayer)
                {
                    node.Mutate(chance, amt);
                }
            }
        }

        public int CalcOutputIndex(double[] inputs, OutputMode mode)
        {
            double[] result = CalcOutputs(inputs, mode);
            return DetermineHighestIndex(result);
        }

        // EFFECTS: calculates the output array
        public double[] CalcOutputs(double[] inputs, OutputMode mode)
        {
            double[] nextLayerInputs = new double[inputs.Length];
            Array.Copy(inputs, 0, nextLayerInputs, 0, inputs.Length);
            double[] output = new double[outputNumber];
            for (int i = 0; i < layers.Length; i++)
            {
                if (i == layers.Length - 1)
                {
                    for (int j = 0; j < layers[i].Length; j++)
                    {
                        output[j] = layers[i][j].CalculateOutput(nextLayerInputs, mode);
                    }
                }
                else
                {
                    double[] tempInputs = new double[nextLayerInputs.Length];
                    Array.Copy(nextLayerInputs, 0, tempInputs, 0, nextLayerInputs.Length);
                    for (int j = 0; j < layers[i].Length; j++)
                    {
                        nextLayerInputs[j] = layers[i][j].CalculateOutput(tempInputs, mode);
                    }
                }
            }
            return output;
        }

        // EFFECTS: calculates the output array quickly (i.e. simple addition of all input weights/biases - no sigmoid)
        protected  double[] FastCalcOutputs(double[] inputs)
        {
            double[] nextLayerInputs = new double[inputs.Length];
            Array.Copy(inputs, 0, nextLayerInputs, 0, inputs.Length);
            double[] output = new double[outputNumber];
            for (int i = 0; i < layers.Length; i++)
            {
                if (i == layers.Length - 1)
                {
                    for (int j = 0; j < layers[i].Length; j++)
                    {
                        output[j] = layers[i][j].FastCalculateOutput(nextLayerInputs);
                    }
                }
                else
                {
                    double[] tempInputs = new double[nextLayerInputs.Length];
                    Array.Copy(nextLayerInputs, 0, tempInputs, 0, nextLayerInputs.Length);
                    for (int j = 0; j < layers[i].Length; j++)
                    {
                        nextLayerInputs[j] = layers[i][j].FastCalculateOutput(tempInputs);
                    }
                }
            }
            return output;
        }

        // EFFECTS: determines the highest index in given array
        public static int DetermineHighestIndex(double[] arr)
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
        public void CopyNodes(Node[][] nodes)
        {
            if (!VerifyNodes(nodes)) throw new Exception();
            Node[][] tempNodes = new Node[nodes.Length][];
            for (int i = 0; i < nodes.Length; i++)
            {
                Node[] tempNodeArray = new Node[nodes[i].Length];
                for (int j = 0; j < nodes[i].Length; j++) tempNodeArray[j] = nodes[i][j].Copy();
                tempNodes[i] = tempNodeArray;
            }
            layers = tempNodes;
        }

        // EFFECTS: returns true if nodes are of correct size/length
        protected bool VerifyNodes(Node[][] node)
        {
            if (node.Length != layers.Length) return false;
            for (int i = 0; i < layers.Length; i++)
            {
                if (node[i].Length != layers[i].Length) return false;
            }
            return true;
        }

        // EFFECTS: returns exact copy of this AI
        public BasicAI copy()
        {
            BasicAI ai = new BasicAI(inputNumber, outputNumber, hiddenLayerNumber);
            ai.CopyNodes(layers);
            return ai;
        }

        private static bool LayersEquals(Node[][] layerOne, Node[][] layerTwo)
        {
            if (layerOne == null) return layerTwo == null;
            if (layerTwo == null) return false;
            if (layerOne.Length != layerTwo.Length) return false;
            for (int i = 0; i < layerOne.Length; i++)
            {
                if (layerOne[i].Length != layerTwo[i].Length) return false;
                for (int j = 0; j < layerOne[i].Length; j++)
                {
                    if (layerOne[i][j] != layerTwo[i][j]) return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            return 31 * base.GetHashCode() + layers.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is BasicAI)) return false;
            BasicAI ai = (BasicAI)obj;
            return LayersEquals(ai.layers, layers);
        }
    }
}