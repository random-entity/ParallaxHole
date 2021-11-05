using UnityEngine;

public class WaterSurfaceY : MonoBehaviour
{
    [SerializeField] private Material waterMaterial;
    private void Awake()
    {
        waterMaterial.SetFloat("_WaterSurfaceY", transform.position.y);
    }
    // private void FixedUpdate()
    // {
    //     waterMaterial.SetFloat("_WaterSurfaceY", transform.position.y);
    // }
}