using System.Collections.Generic;
using UnityEngine;
using Kinect = Windows.Kinect;

public class KinectBodyTracker : MonoBehaviour
{
    [SerializeField] private BodySourceManager bodySourceManager;
    [SerializeField] private BodyObject bodyObjectPrefab;
    private Dictionary<ulong, BodyObject> bodyObjects = new Dictionary<ulong, BodyObject>();

    public Vector3 GetHeadPosition()
    {
        if (bodyObjects != null)
        {
            Debug.Log("GetHeadPosition() : bodyObjects == null");
            return Vector3.zero;
        }
        else if (bodyObjects.Count == 0)
        {
            Debug.Log("GetHeadPosition() : bodyObjects.Count == 0\nNo tracked body");
            return Vector3.zero;
        }
        else if (bodyObjects.Count >= 2)
        {
            Debug.Log("GetHeadPosition() : bodyObjects.Count >= 2\nTwo or more tracked body");
        }

        return Vector3.zero;

        // return bodyObjects[bodyObjects.Keys.]
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
            }
        }

        // Add new bodies to Dictionary bodies and create GameObject
        // Updating joint positions will happen in each bodyObject's FixedUpdate()
        foreach (Kinect.Body body in currFrameBodiesData)
        {
            if (body != null && body.IsTracked && !bodyObjects.ContainsKey(body.TrackingId))
            {
                BodyObject bodyObject = Instantiate(bodyObjectPrefab);
                bodyObject.InitializeBodyObject(body);

                bodyObjects[body.TrackingId] = bodyObject;
            }
        }
    }
}