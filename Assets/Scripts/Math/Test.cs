using System;
using System.Diagnostics;
using UnityEngine;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;

namespace MatrixOp.Test
{

    public class Test : MonoBehaviour {

        public static bool TestMatMul(ComputeShader matmul, float[,] A, float[,] B)
        {
            Stopwatch sw = Stopwatch.StartNew();
            var C0 = CPUMatMul.Multiply(A, B);
            Debug.Log("CPU took " + sw.ElapsedMilliseconds);
            sw = Stopwatch.StartNew();
            var C1 = GPUMatMul.Multiply(matmul, A, B);
            Debug.Log("GPU took " + sw.ElapsedMilliseconds);

            //Debug.Log(C0.ToMatrixString());
            //Debug.Log(C1.ToMatrixString());

            int rows = C0.GetLength(0);
            int columns = C0.GetLength(1);
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    if (!Mathf.Approximately(C0[y, x], C1[y, x]))
                    {
                        Debug.Log(C0[y, x] + " - " + C1[y, x]);
                        return false;
                    }
                }
            }
            return true;
        }
        [Range(8, 2048)] public int a = 1024, b = 512, c = 512;
        public ComputeShader matmul;

        void Start () {
            float[,] A = new float[a, b];
            float[,] B = new float[b, c];

            for(int y = 0; y < a; y++)
            {
                for(int x = 0; x < b; x++)
                {
                    A[y, x] = x;
                }
            }

            for(int y = 0; y < b; y++)
            {
                for(int x = 0; x < c; x++)
                {
                    B[y, x] = x;
                }
            }

            if (!TestMatMul(matmul,A,B)) Debug.LogWarning("CPU impl & GPU impl results are not same.");

            const int iterations = 32;

            Measure("SharedMemory method", () => {
                GPUMatMul.Multiply(matmul, A, B, GPUMatMulMethod.SharedMemory);
            }, iterations);

            Measure("Naive method", () => {
                GPUMatMul.Multiply(matmul, A, B, GPUMatMulMethod.Naive);
            }, iterations);

        }

        void Measure(string label, Action act, int iterations)
        {
            GC.Collect();

            // run once outside of loop to avoid initialization costs
            act.Invoke();
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                act.Invoke();
            }
            sw.Stop();

            Debug.Log(label + " : " + (sw.ElapsedMilliseconds / iterations).ToString());
        }

    }

}


