using AI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class SimpleAIManager : MonoBehaviour
{
    public static Vector2 GOAL = new Vector2(4, -2);
    public static Vector2 StartPoint = new Vector2(-8, 4);
    public int InitialNumAIs = 100;
    public float TimeBetweenEvolutions = 3;
    public float TimeToNextUpdate;
    private List<SimpleAI> ais;
    public GameObject GoalPrefab;
    public GameObject SimpleAIPrefab;
    // Start is called before the first frame update
    void Start()
    {
        GoalPrefab = Instantiate(GoalPrefab, GOAL, Quaternion.identity);
        TimeToNextUpdate = TimeBetweenEvolutions;
        ais = new List<SimpleAI>(InitialNumAIs);
        for (int i = 0; i < InitialNumAIs; i++) {
            GameObject go = Instantiate(SimpleAIPrefab, StartPoint, Quaternion.identity);
            ais.Add(go.GetComponent<SimpleAI>());
        }
    }

    // Update is called once per frame
    void Update()
    {
        TimeToNextUpdate -= Time.deltaTime;
        if (TimeToNextUpdate < 0)
        {
            SimpleAI.Evolve(ais);
            TimeToNextUpdate = TimeBetweenEvolutions;
        }
    }
}
