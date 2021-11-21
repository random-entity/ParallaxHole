using UnityEngine;

[ExecuteInEditMode]
public class SimulationHeadTracker : MonoBehaviour
{
    [SerializeField] private Transform simulationHead;
    [SerializeField] private Transform simulationKinect;

    public Vector3 GetHeadPosKinectSpace()
    {
        return simulationKinect.InverseTransformPoint(simulationHead.position);
    }
}