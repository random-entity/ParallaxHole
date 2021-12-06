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
    private Vector3 headPositionKinectSpace;
    private Vector3 prevHeadPositionKinectSpace;

    public float GetSpeed()
    {
        Vector3 deltaPos = headPositionKinectSpace - prevHeadPositionKinectSpace;
        
        return deltaPos.magnitude / Time.fixedDeltaTime;
    }
    public Vector3 GetHeadPositionUnityWorldSpace()
    {
        return unityRealKinectTransform.TransformPoint(headPositionKinectSpace);
    }
    public Vector3 GetHeadPositionKinectSpace() {
        return headPositionKinectSpace;
    }
    private void FixedUpdate()
    {
        simPlayer.GetComponent<PlayerMovement>().enabled = !simPlayerFollowRealHead;

        // prevHeadPositionKinectSpace = HeadPositionKinectSpace;
        prevHeadPositionKinectSpace.x = headPositionKinectSpace.x;
        prevHeadPositionKinectSpace.y = headPositionKinectSpace.y;
        prevHeadPositionKinectSpace.z = headPositionKinectSpace.z;

        if (useRealKinect)
        {
            Vector3 headPositionKinectSpaceCandidate = kinectBodyTracker.GetHeadPositionKinectSpace();

            if (headPositionKinectSpaceCandidate != Vector3.zero)
            {
                headPositionKinectSpace = headPositionKinectSpaceCandidate;

                if (simPlayerFollowRealHead)
                {
                    simPlayer.position = simulationHeadTracker.transform.TransformPoint(headPositionKinectSpace) + Vector3.up;
                    simPlayerCamera.LookAt(new Vector3(10f, 0f, 0f));
                }
            }
        }
        else
        {
            simPlayer.GetComponent<PlayerMovement>().enabled = true;

            headPositionKinectSpace = simulationHeadTracker.GetHeadPositionKinectSpace();
        }

        projectionCamera.position = GetHeadPositionUnityWorldSpace();
    }
}