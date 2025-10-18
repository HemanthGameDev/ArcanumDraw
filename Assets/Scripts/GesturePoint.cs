using UnityEngine;

public struct GesturePoint
{
    public Vector2 position;
    public float timestamp;
    
    public GesturePoint(Vector2 position, float timestamp)
    {
        this.position = position;
        this.timestamp = timestamp;
    }
}
