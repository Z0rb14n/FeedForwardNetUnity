using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI
{
    class NodeLayerWithBias : NodeLayer
    {
        public NodeLayerWithBias(int numInputs, int numOutputs, Activation activation = Activation.RELU, bool forceUseGPU = false) :
            base(numInputs + 1, numOutputs, activation, forceUseGPU)
        {
        }

        public override float[,] CalculateOutput(float[] inputs)
        {
            float[] actualInputs = new float[inputs.Length + 1];
            Array.Copy(inputs, actualInputs, inputs.Length);
            actualInputs[inputs.Length] = 1;
            return base.CalculateOutput(actualInputs);
        }

        public override NodeLayer Copy()
        {
            NodeLayerWithBias temp = new NodeLayerWithBias(numInputs, numOutputs);
            temp.CopyValues(weights);
            return temp;
        }
    }
}
