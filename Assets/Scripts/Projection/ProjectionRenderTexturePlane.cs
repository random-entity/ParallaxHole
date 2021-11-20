using UnityEngine;

[ExecuteInEditMode]
public class ProjectionRenderTexturePlane : MonoBehaviour
{
    [SerializeField] private ProjectionPlane projectionPlane;

    private void LateUpdate()
    {
        transform.localScale = projectionPlane.Size;
    }
}