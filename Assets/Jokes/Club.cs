using UnityEngine;

public class Club : MonoBehaviour
{
    [SerializeField] private MeshRenderer pointCloudMeshRenderer;
    private Material pointCloudMaterial;

    private void Start()
    {
        pointCloudMaterial = pointCloudMeshRenderer.material;
    }

    private void FixedUpdate()
    {
        float time = Time.time;

        float r = time % 1f;
        float g = (2f * time + 1f) % 2f;
        float b = (3f * time + 2f) % 1.5f;
        pointCloudMaterial.SetVector("_BrightnessScale", new Vector4(r, g, b, 1));
    }
}