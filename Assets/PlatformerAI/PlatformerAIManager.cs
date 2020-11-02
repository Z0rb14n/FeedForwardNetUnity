using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlatformerAIManager : MonoBehaviour
{
    public static Vector2 DeathStarter = new Vector2(-15, -10);
    public static Vector2 StartPoint = new Vector2(-8, 4);
    public static Vector3 CamResetPosition = new Vector3(0, 0, -20);
    public float MaxCameraSpeed = 40;
    public const float MinCameraSpeed = 1;
    public float CameraSpeed = 10;
    public int InitialRandomSeed = 69420;
    public int InitialNumAIs = 100;
    public uint InitialNumPlatforms = 50;
    public float TypicalPlatformGap = 5;
    public float PlatformGapMaxVariability = 1;
    public float PlatformHeightMaxVariability = 3;
    private List<PlatformerAI> ais;
    private PlatformerAIDeathScript deathWall;
    public GameObject PlatformerAIPrefab;
    public GameObject PlatformPrefab;
    public GameObject DeathWallPrefab;
    private uint Generation = 1;
    private uint AIsDead = 0;
    private Slider cameraSpeedSlider;
    private Slider deathWallSpeedSlider;
    private Text GenerationText;
    private Text AIDeathCounter;
    private Text CameraSpeedText;
    private Text DeathWallSpeedText;
    private GameObject cam;
    // Start is called before the first frame update
    void Start()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera");
        GameObject wallOfDoom = Instantiate(DeathWallPrefab, DeathStarter, Quaternion.identity);
        deathWall = wallOfDoom.GetComponent<PlatformerAIDeathScript>();
        deathWall.manager = this;
        ais = new List<PlatformerAI>(InitialNumAIs);
        for (int i = 0; i < InitialNumAIs; i++)
        {
            GameObject go = Instantiate(PlatformerAIPrefab, StartPoint, Quaternion.identity);
            ais.Add(go.GetComponent<PlatformerAI>());
        }
        InitializePlatforms();
        InitializeGUI();
    }

    private void InitializeGUI()
    {
        GameObject ui = GameObject.Find("PlatformerGUI");
        cameraSpeedSlider = ui.transform.Find("CameraSpeedSlider").GetComponent<Slider>();
        cameraSpeedSlider.minValue = MinCameraSpeed;
        cameraSpeedSlider.maxValue = MaxCameraSpeed;
        cameraSpeedSlider.value = CameraSpeed;
        CameraSpeedText = cameraSpeedSlider.gameObject.transform.Find("CameraSpeedText").GetComponent<Text>();
        CameraSpeedText.text = "Camera Speed: " + CameraSpeed;
        deathWallSpeedSlider = ui.transform.Find("DeathWallSpeedSlider").GetComponent<Slider>();
        deathWallSpeedSlider.minValue = PlatformerAIDeathScript.MinSpeed;
        deathWallSpeedSlider.maxValue = PlatformerAIDeathScript.MaxSpeed;
        deathWallSpeedSlider.value = deathWall.Speed;
        DeathWallSpeedText = deathWallSpeedSlider.gameObject.transform.Find("DeathWallSpeedText").GetComponent<Text>();
        DeathWallSpeedText.text = "Death Wall Speed: " + deathWall.Speed;
        GenerationText = ui.transform.Find("GenerationNumberText").GetComponent<Text>();
        AIDeathCounter = ui.transform.Find("AIDeathCounter").GetComponent<Text>();
        AIDeathCounter.text = "AIs Dead: 0/" + InitialNumAIs;
    }

    private void InitializePlatforms()
    {
        PlatformScript prevPlatform = null;
        System.Random random = new System.Random(InitialRandomSeed);
        float prevHeight = 0;
        float prevX = -8;
        for (int i = 0; i < InitialNumPlatforms; i++)
        {
            float newX; float newHeight;
            if (i == 0)
            {
                newX = -8;
                newHeight = 0;
            }
            else
            {
                newX = prevX + TypicalPlatformGap + (float)RandBetween(random, -PlatformGapMaxVariability, PlatformGapMaxVariability);
                newHeight = prevHeight + (float)RandBetween(random, -PlatformHeightMaxVariability, PlatformHeightMaxVariability);
            }
            PlatformScript currPlatform = Instantiate(PlatformPrefab, new Vector2(newX, newHeight), Quaternion.identity).GetComponent<PlatformScript>();
            if (prevPlatform != null) prevPlatform.NextPlatform = currPlatform;
            prevPlatform = currPlatform;
            prevX = newX;
            prevHeight = newHeight;
        }
    }

    private static double RandBetween(System.Random rand, double min, double max)
    {
        return (rand.NextDouble() * (max - min)) + min;
    }

    public void ResetCameraPosition()
    {
        cam.transform.position = CamResetPosition;
    }

    public void ResetDeathWallSpeed()
    {
        deathWall.Speed = PlatformerAIDeathScript.DefaultSpeed;
        deathWallSpeedSlider.value = PlatformerAIDeathScript.DefaultSpeed;
        DeathWallSpeedText.text = "Death Wall Speed: " + deathWall.Speed;
    }

    public void IncrementAIDeathCount()
    {
        AIsDead++;
        AIDeathCounter.text = "AIs Dead: " + AIsDead + "/" + InitialNumAIs;
    }


    private void Update()
    {
        CameraSpeed = cameraSpeedSlider.value;
        CameraSpeedText.text = "Camera Speed: " + CameraSpeed;
        deathWall.Speed = deathWallSpeedSlider.value;
        DeathWallSpeedText.text = "Death Wall Speed: " + deathWall.Speed;
        if (Input.GetKeyDown(KeyCode.Space)) ResetCameraPosition();
        if (Input.GetKey(KeyCode.A)) cam.transform.position -= new Vector3(CameraSpeed * Time.deltaTime, 0);
        if (Input.GetKey(KeyCode.D)) cam.transform.position += new Vector3(CameraSpeed * Time.deltaTime, 0);
        if (Input.GetKey(KeyCode.S)) cam.transform.position -= new Vector3(0, CameraSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.W)) cam.transform.position += new Vector3(0, CameraSpeed * Time.deltaTime);
    }

    // FixedUpdate called once per physics update
    void FixedUpdate()
    {
        foreach (PlatformerAI pai in ais) if (!pai.IsDead) return; // do not prematurely evolve
        Generation++;
        GenerationText.text = "Generation: " + Generation;
        PlatformerAI.Evolve(ais);
        deathWall.Reset();
        AIsDead = 0;
        AIDeathCounter.text = "AIs Dead: 0/" + InitialNumAIs;
    }
}
