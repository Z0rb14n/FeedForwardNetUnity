using AI;
using UnityEngine;
using System.Collections.Generic;

public class PlatformerAI : MonoBasicAI
{
    public const float HighJumpForce = 10;
    public const float SmallJumpForce = 1;
    public float ForwardJumpForce = 0.1f;
    public float HorizontalForce = 2;
    public int TimeMultiplier = 1000;
    public float CanJumpWidthOffset = 0.1f;
    public Vector2 CanJumpTestOffset = new Vector2(0, 0.4f);
    public float CanJumpTestHeight = 0.3f;
    private LayerMask platformLayerMask;
    private const uint HiddenLayerNum = 1;
    private const uint InputNumber = 5;
    private const uint OutputNumber = 4;
    private Rigidbody2D body;
    public bool IsDead { get; private set; }

    public PlatformerAI() : base(HiddenLayerNum,InputNumber,OutputNumber)
    {
        IsDead = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        platformLayerMask = LayerMask.GetMask("Platforms");
    }

    private const double KILL_RATE = 0.5;
    private const double MUTATION_CHANCE = 0.5;
    private const double MUTATION_AMOUNT = 1;

    /**
     * Evolves the list of AIs. Returns index of best
     */
    public static void Evolve(List<PlatformerAI> ais)
    {
        Evolve(new List<MonoBasicAI>(ais), KILL_RATE, MUTATION_CHANCE, MUTATION_AMOUNT);
    }

    public void FixedUpdate()
    {
        if (!IsDead && CanJump()) RunAI();
    }

    public override void Reset()
    {
        base.Reset();
        IsDead = false;
        transform.position = PlatformerAIManager.StartPoint;
        GetComponent<SpriteRenderer>().color = Color.white;
        body.velocity = new Vector2(0, 0);
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

    private Collider2D[] buffer = new Collider2D[2];

    private bool IsGrounded()
    {
        return Physics2D.OverlapBoxNonAlloc((Vector2)transform.position - CanJumpTestOffset, new Vector2(transform.localScale.x - CanJumpWidthOffset, CanJumpTestHeight), 0, buffer, platformLayerMask) > 0;
    }

    private bool CanJump()
    {
        return IsGrounded();

    }
    public override double CalculateFitness()
    {
        return transform.position.x - PlatformerAIManager.StartPoint.x;
    }

    public void SetDead()
    {
        GetComponent<SpriteRenderer>().color = Color.red;
        IsDead = true;
    }

    public float GetLeftBound()
    {
        return transform.position.x - transform.localScale.x / 2;
    }

    public float GetRightBound()
    {
        return transform.position.x + transform.localScale.x / 2;
    }

    protected override void DoOutput(int index)
    {
        if (!CanJump()) return;
        switch (index)
        {
            case 0:
                body.AddForce(new Vector2(ForwardJumpForce, HighJumpForce), ForceMode2D.Impulse); // big jump
                break;
            case 1:
                body.AddForce(new Vector2(ForwardJumpForce, SmallJumpForce), ForceMode2D.Impulse); // small jump
                break;
            case 2:
                body.AddForce(new Vector2(HorizontalForce * Time.fixedDeltaTime * TimeMultiplier, 0)); // move forward
                break;
        }
    }



    private PlatformScript GetPlatformBelow()
    {
        RaycastHit2D leftBelow = Physics2D.Raycast(new Vector2(GetLeftBound(),transform.position.y), new Vector2(0, -1), Mathf.Infinity, platformLayerMask);
        if (leftBelow.collider != null) return leftBelow.collider.gameObject.GetComponent<PlatformScript>();
        RaycastHit2D rightBelow = Physics2D.Raycast(new Vector2(GetRightBound(), transform.position.y), new Vector2(0, -1), Mathf.Infinity, platformLayerMask);
        if (rightBelow.collider != null) return rightBelow.collider.gameObject.GetComponent<PlatformScript>();
        return null;
        // RaycastHit2D centerBelow = Physics2D.Raycast(transform.position, new Vector2(0, -1), Mathf.Infinity, platformLayerMask);
        // if (centerBelow.collider != null) return centerBelow.collider.gameObject.GetComponent<PlatformScript>();
        // return null;
    }

    protected override void UpdateInputs()
    {
        // TODO CODE
        PlatformScript platform = GetPlatformBelow();
        if (platform != null)
        {
            Inputs[0] = platform.DistanceToEdge(this); // distance to edge of platform
            Inputs[1] = platform.DistanceToNextPlatform(); // Gap to next platform
            if (Inputs[1] == PlatformScript.InvalidDistanceToPlatform) Inputs[1] = -1;
            Inputs[2] = platform.VerticalOffsetToNextPlatform(); // Next Height Difference
            if (Inputs[2] == PlatformScript.InvalidVerticalOffset) Inputs[2] = 0;
        } else
        {
            Inputs[0] = -1;
            Inputs[1] = -1;
            Inputs[2] = 0;
        }
        Inputs[3] = body.velocity.x;
        Inputs[4] = body.velocity.y;
    }
}
