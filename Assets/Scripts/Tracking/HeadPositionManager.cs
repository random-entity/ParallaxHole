using UnityEngine;

[ExecuteInEditMode]
public class HeadPositionManager : MonoBehaviour
{
    [SerializeField] private Transform kinect;
    public Vector3 HeadPosKinectSpace { private get; set; }
    private Vector3 prevHeadPosKinectSpace;
    [SerializeField] private Transform projectionRenderTexturePlane;
    [SerializeField] private Transform projectionPlane;
    [SerializeField] private Transform projectionCamera;

    public float getSpeed()
    {
        Vector3 deltaPos = HeadPosKinectSpace - prevHeadPosKinectSpace;
        return deltaPos.magnitude / Time.fixedDeltaTime;
    }
    private void FixedUpdate()
    {
        prevHeadPosKinectSpace = HeadPosKinectSpace;

        // HeadPos = simulatedHead.position - projectionRenderTexturePlane.position + projectionPlane.position;

        // projectionCamera.position = HeadPos;
    }
}