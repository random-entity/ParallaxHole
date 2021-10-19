using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraSettings : MonoBehaviour
{
    private Camera cam;
    [SerializeField] VirtualKinect kinect;
    [SerializeField] private Vector3 headToEyeOffset;
    [SerializeField] private Vector3 headProjToGround;
    [SerializeField] private Transform headProjToGroundDebugger;

    public void SetTransform(float headFrameX, float headFrameY, float headRealDist)
    {
        float kinectHeight = kinect.transform.position.y;

        float maxX = kinectHeight * Mathf.Tan(VirtualKinect.fovX * 0.5f);
        float maxY = kinectHeight * Mathf.Tan(VirtualKinect.fovY * 0.5f);

        float x = Mathf.LerpUnclamped(-maxX, maxX, headFrameX / VirtualKinect.FrameResX);
        float y = Mathf.LerpUnclamped(-maxY, maxY, headFrameY / VirtualKinect.FrameResY);

        headProjToGround = new Vector3(x, 0f, y);

        Vector3 dir = headProjToGround - kinect.transform.position;
        dir.Normalize();
        dir *= headRealDist;
        Vector3 head = kinect.transform.position + dir;
        Vector3 eye = head + headToEyeOffset;
        transform.position = eye;
        transform.LookAt(Vector3.zero);
    }
    private void Awake()
    {
        cam = GetComponent<Camera>();
        headToEyeOffset = new Vector3(0f, -0.1f, 0f);
    }

    private void Update()
    {
        headProjToGroundDebugger.transform.position = headProjToGround;
    }
}