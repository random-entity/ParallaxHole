using UnityEngine;

[ExecuteInEditMode]
public class HeadPositionManager : MonoBehaviour
{
    [SerializeField] private bool useRealKinect;
    [SerializeField] private KinectBodyTracker kinectBodyTracker;
    [SerializeField] private SimulationHeadTracker simulationHeadTracker;
    [SerializeField] private Transform unityRealKinectTransform;
    [SerializeField] private Transform projectionCamera;
    public Vector3 HeadPositionKinectSpace { get; private set; }
    private Vector3 prevHeadPositionKinectSpace;

    public float GetSpeed()
    {
        Vector3 deltaPos = HeadPositionKinectSpace - prevHeadPositionKinectSpace;
        return deltaPos.magnitude / Time.fixedDeltaTime;
    }
    public Vector3 GetHeadPositionUnityWorldSpace()
    {
        return unityRealKinectTransform.TransformPoint(HeadPositionKinectSpace);
    }
    private void FixedUpdate()
    {
        prevHeadPositionKinectSpace = HeadPositionKinectSpace;

        if (useRealKinect)
        {
            Vector3 headPositionKinectSpaceCandidate = kinectBodyTracker.GetHeadPositionKinectSpace();

            if (headPositionKinectSpaceCandidate != Vector3.zero)
            {
                HeadPositionKinectSpace = headPositionKinectSpaceCandidate;
            }
        }
        else
        {
            HeadPositionKinectSpace = simulationHeadTracker.GetHeadPositionKinectSpace();
        }

        projectionCamera.position = GetHeadPositionUnityWorldSpace();
        Debug.Log(GetHeadPositionUnityWorldSpace());
    }
}