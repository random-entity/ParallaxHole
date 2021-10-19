using UnityEngine;

public class VirtualKinect : MonoBehaviour
{
    public static readonly float FrameResX = 512f, FrameResY = 424f;
    public static readonly float fovX = 70.6f * Mathf.PI / 180.0f, fovY = 60.0f * Mathf.PI / 180.0f;
    [SerializeField] CameraSettings cam;
    [SerializeField] private Transform virtualHead;
    private Vector3 groundHit;
    [SerializeField] private Transform groudHitDebugger;
    private Material groudHitDebuggerMaterial;
    private float kinectFrameX, kinectFrameY;

    private void Awake()
    {
        groudHitDebuggerMaterial = groudHitDebugger.GetComponent<MeshRenderer>().material;
    }

    private Vector2 getKinectFrameXYFromVirtualHead()
    {
        float kinectHeight = transform.position.y;

        Vector3 dir = virtualHead.position - transform.position;
        float scale = kinectHeight / (kinectHeight - virtualHead.position.y);
        dir *= scale;
        groundHit = transform.position + dir;

        float maxX = kinectHeight * Mathf.Tan(fovX * 0.5f);
        float maxY = kinectHeight * Mathf.Tan(fovY * 0.5f);

        kinectFrameX = Mathf.InverseLerp(-maxX, maxX, groundHit.x) * FrameResX;
        kinectFrameY = Mathf.InverseLerp(-maxY, maxY, groundHit.z) * FrameResY;

        return new Vector2(kinectFrameX, kinectFrameY);
    }

    void Update()
    {
        groudHitDebugger.transform.position = groundHit;
        groudHitDebuggerMaterial.SetFloat("_x", kinectFrameX / FrameResX);
        groudHitDebuggerMaterial.SetFloat("_y", kinectFrameY / FrameResY);

        Vector2 kinectFrameXY = getKinectFrameXYFromVirtualHead();
        cam.SetTransform(kinectFrameXY.x, kinectFrameXY.y, (transform.position - virtualHead.position).magnitude);
    }
}