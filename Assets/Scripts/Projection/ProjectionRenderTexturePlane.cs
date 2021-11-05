using UnityEngine;

[ExecuteInEditMode]
public class ProjectionRenderTexturePlane : MonoBehaviour // must be child of projection plane
{
    [SerializeField] private ProjectionPlane projectionPlane;

    private void LateUpdate()
    {
        transform.localScale = projectionPlane.Size;
    }
}