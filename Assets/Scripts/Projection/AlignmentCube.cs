using UnityEngine;

[ExecuteInEditMode]
public class AlignmentCube : MonoBehaviour // must be child of projection plane + local transform all default
{
    [SerializeField] private ProjectionPlane projectionPlane;
    [SerializeField] private float Depth = 5;
    [SerializeField] private Transform back, left, right, top, bottom;

    private void Update()
    {
        if (gameObject.activeInHierarchy)
        {
            UpdateCube();
        }
    }
    private void UpdateQuad(Transform t, Vector3 pos, Vector3 scale, Quaternion rotation)
    {
        t.localPosition = pos;
        t.localScale = scale;
        t.localRotation = rotation;
    }
    public void UpdateCube()
    {
        Vector2 Size = projectionPlane.Size;
        Vector2 halfSize = Size * 0.5f;

        UpdateQuad(back,
            new Vector3(0, 0, Depth),
            new Vector3(Size.x, Size.y),
            Quaternion.identity
        );
        UpdateQuad(left,
            new Vector3(-halfSize.x, 0, Depth * 0.5f),
            new Vector3(Depth, Size.y, 0),
            Quaternion.Euler(0, -90, 0)
        );
        UpdateQuad(right,
            new Vector3(halfSize.x, 0, Depth * 0.5f),
            new Vector3(Depth, Size.y, 0),
            Quaternion.Euler(0, 90, 0)
        );
        UpdateQuad(top,
            new Vector3(0, halfSize.y, Depth * 0.5f),
            new Vector3(Size.x, Depth, 0),
            Quaternion.Euler(-90, 0, 0)
        );
        UpdateQuad(bottom,
            new Vector3(0, -halfSize.y, Depth * 0.5f),
            new Vector3(Size.x, Depth, 0),
            Quaternion.Euler(90, 0, 0)
        );
    }
}