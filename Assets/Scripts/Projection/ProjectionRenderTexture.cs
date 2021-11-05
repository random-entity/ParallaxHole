using UnityEngine;

[ExecuteInEditMode]
public class ProjectionRenderTexture : MonoBehaviour // must be child of projection plane
{
    public static Vector3 simulationOffsetFromProjectionPlane = Vector3.zero;// Vector3.right * 10f;
    [SerializeField] private ProjectionPlane projectionPlane;

    private void LateUpdate()
    {
        Vector2 Size = projectionPlane.Size;

        transform.localPosition = simulationOffsetFromProjectionPlane;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Size;
    }
}