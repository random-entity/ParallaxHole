//NOTE: Undefine this if you need to move the plane at runtime
//#define PRECALC_PLANE

using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class ProjectionCamera : MonoBehaviour
{
    private Camera cam;

    [Header("Projection plane")]
    public ProjectionPlane projectionPlane;
    public bool ProjectionPlaneIsNearPlane;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        cam.depthTextureMode = DepthTextureMode.Depth;
    }

    private void setMatrices()
    {
        Vector3 eyePos = transform.position;

        Vector3 pa = projectionPlane.BottomLeft;
        Vector3 pb = projectionPlane.BottomRight;
        Vector3 pc = projectionPlane.TopLeft;

        Vector3 vr = projectionPlane.DirRight;
        Vector3 vu = projectionPlane.DirUp;
        Vector3 vn = projectionPlane.DirNormal;

        Vector3 va = pa - eyePos;
        Vector3 vb = pb - eyePos;
        Vector3 vc = pc - eyePos;

        Matrix4x4 M = projectionPlane.M;
        Matrix4x4 T = Matrix4x4.Translate(-eyePos);
        Matrix4x4 R = Matrix4x4.Rotate(Quaternion.Inverse(transform.rotation) * projectionPlane.transform.rotation);

        cam.worldToCameraMatrix = M * R * T;

        float d = -Vector3.Dot(va, vn);

        if (ProjectionPlaneIsNearPlane) cam.nearClipPlane = d;
        float n = cam.nearClipPlane;
        float f = cam.farClipPlane;

        float nod = n / d;

        float l = Vector3.Dot(vr, va) * nod;
        float r = Vector3.Dot(vr, vb) * nod;
        float b = Vector3.Dot(vu, va) * nod;
        float t = Vector3.Dot(vu, vc) * nod;

        Matrix4x4 P = Matrix4x4.Frustum(l, r, b, t, n, f);

        cam.projectionMatrix = P;
    }
    private void LateUpdate()
    {
        setMatrices();
    }
}