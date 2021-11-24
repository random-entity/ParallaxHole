using UnityEngine;

[ExecuteInEditMode]
public class HeadPositionManager : MonoBehaviour
{
    [SerializeField] private bool isReal;
    [SerializeField] private KinectBodyTracker kinectBodyTracker;
    [SerializeField] private SimulationHeadTracker simulationHeadTracker;
    [SerializeField] private PlayerMovement simulationPlayer;
    [SerializeField] private Transform projectionCamera;
    public Vector3 HeadPositionKinectSpace { get; private set; }
    private Vector3 prevHeadPositionKinectSpace;

    public float GetSpeed()
    {
        Vector3 deltaPos = HeadPositionKinectSpace - prevHeadPositionKinectSpace;
        return deltaPos.magnitude / Time.fixedDeltaTime;
    }
    public Vector3 GetHeadPosUnityWorldSpace()
    {
        return kinectBodyTracker.transform.TransformPoint(HeadPositionKinectSpace);
    }
    private void FixedUpdate()
    {
        prevHeadPositionKinectSpace = HeadPositionKinectSpace;

        if (isReal)
        {
            Vector3 headPositionKinectSpaceCandidate = kinectBodyTracker.GetHeadPositionKinectSpace();

            if (headPositionKinectSpaceCandidate != Vector3.zero)
            {
                HeadPositionKinectSpace = headPositionKinectSpaceCandidate;

                // simulationPlayer.transform.position = simulationHeadTracker.transform.TransformPoint(headPositionKinectSpaceCandidate);
                // simulationPlayer.playerCamera.transform.LookAt(new Vector3(10f, 0, 0));
            }
        }
        else
        {
            HeadPositionKinectSpace = simulationHeadTracker.GetHeadPosKinectSpace();
        }

        projectionCamera.position = GetHeadPosUnityWorldSpace();
    }
}