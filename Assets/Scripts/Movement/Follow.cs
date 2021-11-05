using UnityEngine;

[ExecuteInEditMode]
public class Follow : MonoBehaviour
{
    [SerializeField] private Transform master;
    [SerializeField] private bool useRenderTextureQuadOffset = true;
    [SerializeField] private Vector3 offset;

    private void Update()
    {
        if (enabled)
        {
            if (useRenderTextureQuadOffset) offset = -ProjectionRenderTexture.simulationOffsetFromProjectionPlane;
            this.transform.position = master.transform.position + offset;
        }
    }
}