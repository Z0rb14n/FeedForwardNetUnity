using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI
{
    public interface IAI
    {
        void Randomize();
        void Mutate(double chance, double amt);
        double[] CalculateOutputs(double[] inputs);

        IAI Copy();
    }
}
