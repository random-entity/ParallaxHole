using UnityEngine;

[ExecuteInEditMode]
public class SimulationHeadTracker : MonoBehaviour
// simulated head의 position, projection RT plane, projection plane의 position, rotation, scale만 알고, 
// projection RT plane 기준 simulated head의 local position을 산출해서
// projection plane의 같은 local space position에 그 점이 있다고 했을 때 그 점의 world space를 산출해서
// projection cam의 world position을 거기로 설정해준다.
{
    [SerializeField] private Transform projectionPlane;
    [SerializeField] private Transform projectionRenderTexturePlane;
    [SerializeField] private Transform simulatedEye;
    [SerializeField] private Transform projectionCamera;
    public Vector3 EyePosition;
    private Vector3 prevEyePosition;
    private Vector3 GetDeltaPosition()
    {
        return EyePosition - prevEyePosition;
    }
    public float GetSqrSpeed()
    {
        return GetDeltaPosition().sqrMagnitude;
    }

    private void FixedUpdate()
    {
        prevEyePosition = EyePosition;

        // Vector3 projectionRenderTexturePlaneLocalHeadPosition = projectionRenderTexturePlane.InverseTransformPoint(simulatedEye.position);
        // EyePosition = projectionPlane.TransformPoint(projectionRenderTexturePlaneLocalHeadPosition);

        EyePosition = simulatedEye.position - projectionRenderTexturePlane.position + projectionPlane.position;

        projectionCamera.position = EyePosition;
    }
}