using UnityEngine;

public class PlatformerAIDeathScript : MonoBehaviour
{
    public const float DefaultSpeed = 1;
    public float Speed = 1;
    public const float MinSpeed = 0.1f;
    public const float MaxSpeed = 30f;
    public PlatformerAIManager manager;
    void FixedUpdate()
    {
        GetComponent<Rigidbody2D>().velocity = new Vector2(Speed, 0);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlatformerAI pai = collision.gameObject.GetComponent<PlatformerAI>();
        if (pai != null)
        {
            bool actuallyIncrement = !pai.IsDead;
            pai.SetDead();
            if (actuallyIncrement && manager != null) manager.IncrementAIDeathCount();
        }
    }

    public void Reset()
    {
        transform.position = PlatformerAIManager.DeathStarter;
    }
}
