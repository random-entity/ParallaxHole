using UnityEngine;

public class WaterSurfaceY : MonoBehaviour
{
    public static float waterSurfaceY;
    private void FixedUpdate()
    {
        waterSurfaceY = transform.position.y;
    }
}