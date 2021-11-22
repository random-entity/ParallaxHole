using System.Collections.Generic;
using UnityEngine;
using Kinect = Windows.Kinect;

public class BodyObject : MonoBehaviour
{
    public Kinect.Body body;
    private Dictionary<Kinect.JointType, Transform> jointTransforms;
    [SerializeField] private Material jointMaterial, boneMaterial;
    [SerializeField] private float jointScale;

    private void FixedUpdate()
    {
        updateJointsPosition();
    }

    public Vector3 GetHeadPosition()
    {
        return getVector3PositionFromJoint(body.Joints[Kinect.JointType.Head]);
    }

    public void InitializeBodyObject(Kinect.Body body)
    {
        this.body = body;
        gameObject.name = "BodyObject ID: " + body.TrackingId;

        jointTransforms = new Dictionary<Kinect.JointType, Transform>();

        for (Kinect.JointType jointType = Kinect.JointType.SpineBase; jointType <= Kinect.JointType.ThumbRight; jointType++)
        {
            GameObject jointObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            jointObject.GetComponent<MeshRenderer>().material = jointMaterial;

            bool isHead = (int)jointType == 3;
            bool isFinger = (int)jointType >= 21;

            if (isHead)
            {
                jointObject.GetComponent<MeshRenderer>().material.color = Color.red;
            }

            LineRenderer lineRenderer = jointObject.AddComponent<LineRenderer>();
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

            jointTransforms.Add(jointType, jointObject.transform);
        }
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

            LineRenderer lr = jointTransform.GetComponent<LineRenderer>();
            if (targetJoint.HasValue)
            {
                lr.SetPosition(0, jointTransform.localPosition);
                lr.SetPosition(1, getVector3PositionFromJoint(targetJoint.Value));
                lr.startColor = getColorFromState(sourceJoint.TrackingState);
                lr.endColor = getColorFromState(targetJoint.Value.TrackingState);
            }
            else
            {
                lr.enabled = false;
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