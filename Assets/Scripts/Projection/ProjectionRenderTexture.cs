using UnityEngine;

[ExecuteInEditMode]
public class ProjectionRenderTexture : MonoBehaviour // must be child of projection plane
{
    public Vector3 offset;
    [SerializeField] private ProjectionPlane projectionPlane;

    private void LateUpdate()
    {
        Vector2 Size = projectionPlane.Size;

        transform.localPosition = offset;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Size;
    }
}