using System.Collections.Generic;
using UnityEngine;
using Kinect = Windows.Kinect;

public class BodyObject : MonoBehaviour
{
    public Kinect.Body body;
    private Dictionary<Kinect.JointType, Transform> jointTransforms;
    [SerializeField] private Material jointMaterial, boneMaterial;
    [SerializeField] private GameObject jointObjectPrefab;
    [SerializeField] private float jointScale;
    [SerializeField] private bool doRenderJoints;

    public Vector3 GetHeadPositionKinectSpace()
    {
        return getVector3PositionFromJoint(body.Joints[Kinect.JointType.Head]);
    }

    private void FixedUpdate()
    {
        updateJointsPosition();
    }

    private void setRenderJointsEnabled(bool on)
    {
        foreach (var jointType in jointTransforms.Keys)
        {
            jointTransforms[jointType].GetComponent<MeshRenderer>().enabled = on;
            jointTransforms[jointType].GetComponent<LineRenderer>().enabled = on;
        }
    }

    public void InitializeBodyObject(Kinect.Body body) // This method is called by KinectBodyTracker.cs whenever new body is tracked
    {
        this.body = body;
        gameObject.name = "BodyObject ID: " + body.TrackingId;

        jointTransforms = new Dictionary<Kinect.JointType, Transform>();

        for (Kinect.JointType jointType = Kinect.JointType.SpineBase; jointType <= Kinect.JointType.ThumbRight; jointType++)
        {
            GameObject jointObject = Instantiate(jointObjectPrefab);
            jointObject.GetComponent<MeshRenderer>().material = jointMaterial;

            bool isHead = (int)jointType == 3;
            bool isFinger = (int)jointType >= 21;

            if (isHead)
            {
                jointObject.GetComponent<MeshRenderer>().material.color = Color.red;
            }

            LineRenderer lineRenderer = jointObject.GetComponent<LineRenderer>();
            lineRenderer.material = boneMaterial;
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = 0.0125f;
            lineRenderer.endWidth = 0.025f;

            jointObject.name = jointType.ToString();

            Vector3 localScale = Vector3.one * jointScale;
            if (isFinger) localScale *= 0.25f;
            if (isHead) localScale *= 2f;
            jointObject.transform.localScale = localScale;
            jointObject.transform.parent = transform;

            jointObject.layer = 7; // "Debugger";
            jointTransforms.Add(jointType, jointObject.transform);
        }

        setRenderJointsEnabled(doRenderJoints);
    }

    private void updateJointsPosition()
    {
        for (Kinect.JointType jointType = Kinect.JointType.SpineBase; jointType <= Kinect.JointType.ThumbRight; jointType++)
        {
            Kinect.Joint sourceJoint = body.Joints[jointType];

            Kinect.Joint? targetJoint = null;
            if (boneMap.ContainsKey(jointType)) targetJoint = body.Joints[boneMap[jointType]];

            Transform jointTransform = jointTransforms[jointType];
            jointTransform.localPosition = getVector3PositionFromJoint(sourceJoint);

            LineRenderer lineRenderer = jointTransform.GetComponent<LineRenderer>();
            if (targetJoint.HasValue)
            {
                lineRenderer.SetPosition(0, jointTransform.position);
                lineRenderer.SetPosition(1, transform.TransformPoint(getVector3PositionFromJoint(targetJoint.Value)));
                lineRenderer.startColor = getColorFromState(sourceJoint.TrackingState);
                lineRenderer.endColor = getColorFromState(targetJoint.Value.TrackingState);
            }
            else
            {
                lineRenderer.enabled = false;
            }
        }
    }

    private static Vector3 getVector3PositionFromJoint(Kinect.Joint joint)
    {
        return new Vector3(-joint.Position.X, joint.Position.Y, joint.Position.Z);
    }

    private static Color getColorFromState(Kinect.TrackingState state)
    {
        switch (state)
        {
            case Kinect.TrackingState.Tracked:
                return Color.green;

            case Kinect.TrackingState.Inferred:
                return Color.red;

            default:
                return Color.black;
        }
    }

    public static readonly Dictionary<Kinect.JointType, Kinect.JointType> boneMap = new Dictionary<Kinect.JointType, Kinect.JointType>()
    {
        { Kinect.JointType.FootLeft, Kinect.JointType.AnkleLeft },
        { Kinect.JointType.AnkleLeft, Kinect.JointType.KneeLeft },
        { Kinect.JointType.KneeLeft, Kinect.JointType.HipLeft },
        { Kinect.JointType.HipLeft, Kinect.JointType.SpineBase },

        { Kinect.JointType.FootRight, Kinect.JointType.AnkleRight },
        { Kinect.JointType.AnkleRight, Kinect.JointType.KneeRight },
        { Kinect.JointType.KneeRight, Kinect.JointType.HipRight },
        { Kinect.JointType.HipRight, Kinect.JointType.SpineBase },

        { Kinect.JointType.HandTipLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.ThumbLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.HandLeft, Kinect.JointType.WristLeft },
        { Kinect.JointType.WristLeft, Kinect.JointType.ElbowLeft },
        { Kinect.JointType.ElbowLeft, Kinect.JointType.ShoulderLeft },
        { Kinect.JointType.ShoulderLeft, Kinect.JointType.SpineShoulder },

        { Kinect.JointType.HandTipRight, Kinect.JointType.HandRight },
        { Kinect.JointType.ThumbRight, Kinect.JointType.HandRight },
        { Kinect.JointType.HandRight, Kinect.JointType.WristRight },
        { Kinect.JointType.WristRight, Kinect.JointType.ElbowRight },
        { Kinect.JointType.ElbowRight, Kinect.JointType.ShoulderRight },
        { Kinect.JointType.ShoulderRight, Kinect.JointType.SpineShoulder },

        { Kinect.JointType.SpineBase, Kinect.JointType.SpineMid },
        { Kinect.JointType.SpineMid, Kinect.JointType.SpineShoulder },
        { Kinect.JointType.SpineShoulder, Kinect.JointType.Neck },
        { Kinect.JointType.Neck, Kinect.JointType.Head },
    };
}