using AI;
using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class SimpleAI : MonoBasicAI
{
    private static Vector2 goal = SimpleAIManager.GOAL;
    private const uint HiddenLayerNum = 1;
    private const uint InputNumber = 4;
    private const uint OutputNumber = 5;
    private double CachedFitness = double.NaN;
    private Vector2 vel;

    public SimpleAI() : base(HiddenLayerNum, InputNumber, OutputNumber)
    {}


    // Start is called before the first frame update
    void Start()
    {
        transform.position = SimpleAIManager.StartPoint;
    }

    private void Update()
    {
        RunAI();
    }

    public override double CalculateFitness()
    {
        if (double.IsNaN(CachedFitness)) CachedFitness = -Vector2.Distance(transform.position, SimpleAIManager.GOAL);
        return CachedFitness;
    }


    private const double KILL_RATE = 0.5;
    private const double MUTATION_CHANCE = 0.5;
    private const double MUTATION_AMOUNT = 1;

    /**
     * Evolves the list of AIs. Returns index of best
     */
    public static void Evolve(List<SimpleAI> ais)
    {
        Evolve(new List<MonoBasicAI>(ais), KILL_RATE, MUTATION_CHANCE, MUTATION_AMOUNT);
    }

    public override void SetAsBest()
    {
        base.SetAsBest();
        GetComponent<SpriteRenderer>().color = Color.green;
        GetComponent<SpriteRenderer>().sortingOrder = 1;
    }

    public override void SetNotBest()
    {
        base.SetNotBest();
        GetComponent<SpriteRenderer>().color = Color.white;
        GetComponent<SpriteRenderer>().sortingOrder = 0;
    }

    /**
     * Performs the given output given the index (of the output)
     * @param index index of output
     */
    protected override void DoOutput(int index)
    {
        switch (index)
        {
            case 0:
                transform.position += new Vector3(0, 10) * Time.deltaTime;
                break;

            case 1:
                transform.position += new Vector3(10, 0) * Time.deltaTime;
                break;

            case 2:
                transform.position -= new Vector3(0, 10) * Time.deltaTime;
                break;

            case 3:
                transform.position -= new Vector3(10,0) * Time.deltaTime;
                break;
        }
    }

    // MODIFIES: this
    // EFFECTS: resets the AI to it's initial position
    public override void Reset()
    {
        base.Reset();
        transform.position = SimpleAIManager.StartPoint;
        CachedFitness = double.NaN;
        vel = Vector2.zero;
    }

    // EFFECTS: gets the various inputs of this AI
    protected override void UpdateInputs()
    {
        Inputs[0] = (goal.x - transform.position.x) / 4;
        Inputs[1] = (goal.y - transform.position.y) / 4;
        Inputs[2] = vel.x;
        Inputs[3] = vel.y;
    }

    public override bool Equals(object other)
    {
        if (!(other is SimpleAI)) return false;
        SimpleAI sai = (SimpleAI)other;
        return sai.ai.Equals(ai) && sai.transform.position.Equals(transform.position);
    }

    public override int GetHashCode()
    {
        return 31 * base.GetHashCode() + transform.position.GetHashCode();
    }
}
