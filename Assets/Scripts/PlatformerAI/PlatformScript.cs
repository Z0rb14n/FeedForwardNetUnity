using UnityEngine;

public class PlatformScript : MonoBehaviour
{
    public PlatformScript NextPlatform;

    public float GetRightBound()
    {
        return transform.position.x + transform.localScale.x / 2;
    }

    public float GetTopBound()
    {
        return transform.position.y + transform.localScale.y / 2;
    }

    public float GetLeftBound()
    {
        return transform.position.x - transform.localScale.x / 2;
    }

    public float GetBottomBound()
    {
        return transform.position.y - transform.localScale.y / 2;
    }

    // REQUIRES: pai is above/on platform
    public float DistanceToEdge(PlatformerAI pai)
    {
        return Mathf.Max(0, GetRightBound() - pai.GetLeftBound());
    }

    public const float InvalidDistanceToPlatform = -1;
    public const float InvalidVerticalOffset = float.PositiveInfinity;

    public float DistanceToNextPlatform()
    {
        if (NextPlatform == null) return InvalidDistanceToPlatform;
        return NextPlatform.GetLeftBound() - GetRightBound();
    }

    public float VerticalOffsetToNextPlatform()
    {
        if (NextPlatform == null) return InvalidVerticalOffset;
        return NextPlatform.GetTopBound() - GetTopBound();
    }
}
