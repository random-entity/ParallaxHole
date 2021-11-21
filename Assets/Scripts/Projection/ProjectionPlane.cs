using UnityEngine;

[ExecuteInEditMode]
public class ProjectionPlane : MonoBehaviour
{
    [Header("Size")]
    public Vector2 Size = new Vector2(8, 4.5f);
    private Vector2 previousSize = new Vector2(8, 4.5f);
    public Vector2 AspectRatio = new Vector2(16f, 9f);
    private Vector2 previousAspectRatio = new Vector2(16f, 9f);
    public bool LockAspectRatio = true;

    [Header("Visualization")]
    public bool DrawGizmo = true;

    // corners
    public Vector3 BottomLeft { get; private set; }
    public Vector3 BottomRight { get; private set; }
    public Vector3 TopLeft { get; private set; }
    public Vector3 TopRight { get; private set; }

    // plane space orthonormal basis
    public Vector3 DirRight { get; private set; }
    public Vector3 DirUp { get; private set; }
    public Vector3 DirNormal { get; private set; }

    private Matrix4x4 m;
    public Matrix4x4 M { get => m; }

    private void Update()
    {
        aspectRatioConstraint();
    }

    private void OnDrawGizmos()
    {
        if (DrawGizmo)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(BottomLeft, BottomRight);
            Gizmos.DrawLine(BottomRight, TopRight);
            Gizmos.DrawLine(TopRight, TopLeft);
            Gizmos.DrawLine(TopLeft, BottomLeft);

            Gizmos.color = Color.cyan;
            var planeCenter = 0.5f * (BottomLeft + TopRight);
            Gizmos.DrawLine(planeCenter, planeCenter + DirNormal);
        }
    }

    private void aspectRatioConstraint()
    {
        if (LockAspectRatio)
        {
            if (AspectRatio.x != previousAspectRatio.x)
            {
                Size.y = Size.x / AspectRatio.x * AspectRatio.y;
                previousAspectRatio.y = AspectRatio.y;
            }
            if (AspectRatio.y != previousAspectRatio.y)
            {
                Size.x = Size.y / AspectRatio.y * AspectRatio.x;
            }

            if (Size.x != previousSize.x)
            {
                Size.y = Size.x / AspectRatio.x * AspectRatio.y;
                previousSize.y = Size.y;
            }
            if (Size.y != previousSize.y)
            {
                Size.x = Size.y / AspectRatio.y * AspectRatio.x;
            }

            Size.x = Mathf.Max(1f, Size.x);
            Size.y = Mathf.Max(1f, Size.y);
            AspectRatio.x = Mathf.Max(1f, AspectRatio.x);
            AspectRatio.y = Mathf.Max(1f, AspectRatio.y);

            previousSize = Size;
            previousAspectRatio = AspectRatio;

            BottomLeft = transform.TransformPoint(new Vector3(-Size.x, -Size.y) * 0.5f);
            BottomRight = transform.TransformPoint(new Vector3(Size.x, -Size.y) * 0.5f);
            TopLeft = transform.TransformPoint(new Vector3(-Size.x, Size.y) * 0.5f);
            TopRight = transform.TransformPoint(new Vector3(Size.x, Size.y) * 0.5f);

            DirRight = (BottomRight - BottomLeft).normalized;
            DirUp = (TopLeft - BottomLeft).normalized;
            DirNormal = -Vector3.Cross(DirRight, DirUp).normalized;

            m = Matrix4x4.zero;

            m.SetColumn(0, DirRight);
            m.SetColumn(1, DirUp);
            m.SetColumn(2, DirNormal);
            m[3, 3] = 1f;
        }
    }
}