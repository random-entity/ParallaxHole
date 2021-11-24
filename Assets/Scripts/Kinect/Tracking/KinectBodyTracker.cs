using System.Collections.Generic;
using UnityEngine;
using Kinect = Windows.Kinect;

public class KinectBodyTracker : MonoBehaviour
{
    [SerializeField] private BodySourceManager bodySourceManager;
    [SerializeField] private BodyObject bodyObjectPrefab;
    private Dictionary<ulong, BodyObject> bodyObjects = new Dictionary<ulong, BodyObject>();
    [SerializeField] private List<ulong> orderedListOfBodyIds = new List<ulong>();

    public Vector3 GetHeadPositionKinectSpace()
    {
        if (bodyObjects == null)
        {
            Debug.Log("GetHeadPosition() : bodyObjects == null");
            return Vector3.zero;
        }
        else if (bodyObjects.Count == 0)
        {
            Debug.Log("GetHeadPosition() : bodyObjects.Count == 0 : No tracked body");
            return Vector3.zero;
        }
        else if (bodyObjects.Count >= 2)
        {
            Debug.Log("GetHeadPosition() : bodyObjects.Count >= 2 : Two or more tracked body");
        }

        return bodyObjects[orderedListOfBodyIds[0]].GetHeadPositionKinectSpace();
    }

    private void FixedUpdate()
    {
        Kinect.Body[] currFrameBodiesData = bodySourceManager.GetData();
        if (currFrameBodiesData == null) return;

        // Get bodies tracked in current frame
        List<ulong> currFrameTrackedBodyIds = new List<ulong>();
        foreach (Kinect.Body body in currFrameBodiesData)
        {
            if (body != null && body.IsTracked)
            {
                currFrameTrackedBodyIds.Add(body.TrackingId);
            }
        }

        // First delete untracked bodies
        List<ulong> prevFrameTrackedBodyIds = new List<ulong>(bodyObjects.Keys);
        foreach (ulong prevFrameTrackedBodyId in prevFrameTrackedBodyIds)
        {
            if (!currFrameTrackedBodyIds.Contains(prevFrameTrackedBodyId))
            {
                Destroy(bodyObjects[prevFrameTrackedBodyId].gameObject);
                bodyObjects.Remove(prevFrameTrackedBodyId);

                orderedListOfBodyIds.Remove(prevFrameTrackedBodyId);
            }
        }

        // Add new bodies to Dictionary bodies and create GameObject
        // Updating joint positions will happen in each bodyObject's FixedUpdate()
        foreach (Kinect.Body body in currFrameBodiesData)
        {
            if (body != null && body.IsTracked && !bodyObjects.ContainsKey(body.TrackingId))
            {
                BodyObject bodyObject = Instantiate(bodyObjectPrefab);
                bodyObject.transform.SetParent(transform);

                bodyObject.InitializeBodyObject(body);

                bodyObjects[body.TrackingId] = bodyObject;

                orderedListOfBodyIds.Add(body.TrackingId);
            }
        }
    }
}