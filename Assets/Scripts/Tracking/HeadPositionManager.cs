using UnityEngine;

[ExecuteInEditMode]
public class HeadPositionManager : MonoBehaviour
{
    [SerializeField] private SimulationHeadTracker simulationHeadTracker;
    [SerializeField] private Transform unityKinect;
    [SerializeField] private Transform projectionCamera;
    public Vector3 HeadPosKinectSpace { get; private set; }
    private Vector3 prevHeadPosKinectSpace;

    public float GetSpeed()
    {
        Vector3 deltaPos = HeadPosKinectSpace - prevHeadPosKinectSpace;
        return deltaPos.magnitude / Time.fixedDeltaTime;
    }
    public Vector3 GetHeadPosUnitySpace()
    {
        return unityKinect.TransformPoint(HeadPosKinectSpace);
    }
    private void FixedUpdate()
    {
        prevHeadPosKinectSpace = HeadPosKinectSpace;
        HeadPosKinectSpace = simulationHeadTracker.GetHeadPosKinectSpace();
        projectionCamera.position = GetHeadPosUnitySpace();
    }
}