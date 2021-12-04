using UnityEngine;

[ExecuteInEditMode]
public class HeadPositionManager : MonoBehaviour
{
    [SerializeField] private bool useRealKinect;
    [SerializeField] private KinectBodyTracker kinectBodyTracker;
    [SerializeField] private SimulationHeadTracker simulationHeadTracker;
    [SerializeField] private bool simPlayerFollowRealHead;
    [SerializeField] private Transform simPlayer;
    [SerializeField] private Transform simPlayerCamera;
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
        simPlayer.GetComponent<PlayerMovement>().enabled = !simPlayerFollowRealHead;

        // prevHeadPositionKinectSpace = HeadPositionKinectSpace;
        prevHeadPositionKinectSpace.x = HeadPositionKinectSpace.x;
        prevHeadPositionKinectSpace.y = HeadPositionKinectSpace.y;
        prevHeadPositionKinectSpace.z = HeadPositionKinectSpace.z;

        if (useRealKinect)
        {
            Vector3 headPositionKinectSpaceCandidate = kinectBodyTracker.GetHeadPositionKinectSpace();

            if (headPositionKinectSpaceCandidate != Vector3.zero)
            {
                HeadPositionKinectSpace = headPositionKinectSpaceCandidate;

                if (simPlayerFollowRealHead)
                {
                    simPlayer.position = simulationHeadTracker.transform.TransformPoint(HeadPositionKinectSpace) + Vector3.up;
                    simPlayerCamera.LookAt(new Vector3(10f, 0f, 0f));
                }
            }
        }
        else
        {
            simPlayer.GetComponent<PlayerMovement>().enabled = true;

            HeadPositionKinectSpace = simulationHeadTracker.GetHeadPositionKinectSpace();
        }

        projectionCamera.position = GetHeadPositionUnityWorldSpace();
    }
}