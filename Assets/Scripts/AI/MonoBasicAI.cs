using System;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    public abstract class MonoBasicAI : MonoBehaviour
    {
        public double[] Inputs;
        public int BestIndex = -1;
        public bool IsBest = false;
        protected BasicAI ai;

        public MonoBasicAI(uint HiddenLayerNum, uint InputNum, uint OutputNum) : base()
        {
            Inputs = new double[InputNum];
            ai = new BasicAI(InputNum, OutputNum, HiddenLayerNum);
            ai.Randomize();
        }

        /**
         * Evolves the list of AIs. Returns index of best
         */
        public static void Evolve(List<MonoBasicAI> ais, double KillRate, double MutationChance, double MutationAmount)
        {
            int desiredSize = ais.Count;
            ais.Sort(new AIComparator());
            List<BasicAI> basicAIs = new List<BasicAI>(ais.Count);
            for (int i = 0; i < ais.Count; i++) basicAIs.Add((BasicAI)ais[i].ai.Copy());
            int amountKilled = (int)Math.Round(KillRate * ais.Count);
            basicAIs.RemoveRange(0, amountKilled);
            int currentSize = ais.Count;
            int currentIndex = 0;
            while (basicAIs.Count < desiredSize)
            {
                BasicAI ai = (BasicAI)basicAIs[currentIndex].Copy();
                ai.Mutate(MutationChance, MutationAmount);
                basicAIs.Add(ai);
                currentIndex++;
                if (currentIndex == currentSize) currentIndex = 0;
            }
            for (int i = 0; i < basicAIs.Count; i++) ais[i].ai = basicAIs[i];
            foreach (MonoBasicAI ai in ais) ai.Reset();
            ais[currentSize - 1].SetAsBest();
        }

        public virtual void SetNotBest()
        {
            IsBest = false;
        }

        public virtual void SetAsBest()
        {
            IsBest = true;
        }

        public abstract double CalculateFitness();

        protected abstract void DoOutput(int index);

        public virtual void Reset()
        {
            SetNotBest();
        }

        protected abstract void UpdateInputs();

        public virtual void RunAI()
        {
            UpdateInputs();
            int index = ai.CalcOutputIndex(Inputs, OutputMode.SIGMOID);
            BestIndex = index;
            DoOutput(index);
        }

        protected sealed class AIComparator : IComparer<MonoBasicAI>
        {
            public int Compare(MonoBasicAI x, MonoBasicAI y)
            {
                if (x.CalculateFitness() > y.CalculateFitness()) return 1;
                if (x.CalculateFitness() == y.CalculateFitness()) return 0;
                return -1;
            }
        }

        public override bool Equals(object other)
        {
            if (!(other is MonoBasicAI)) return false;
            MonoBasicAI sai = (MonoBasicAI)other;
            return sai.ai.Equals(ai);
        }

        public override int GetHashCode()
        {
            return (ai).GetHashCode();
        }
    }
}