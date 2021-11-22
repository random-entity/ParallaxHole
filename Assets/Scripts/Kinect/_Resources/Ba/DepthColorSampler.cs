using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

public class DepthColorSampler : MonoBehaviour
{
    // Data containers
    private ushort[] depthData;
    private CameraSpacePoint[] cameraSpacePoints;
    private ColorSpacePoint[] colorSpacePoints;
    private List<SamplePoint> samplePoints;
    public static int sampleResolution = 1;

    // Kinect
    [SerializeField] private MultiSourceManager multiSourceManager;
    private KinectSensor sensor;
    private CoordinateMapper coordinateMapper;
    private readonly Vector2Int depthResolution = new Vector2Int(512, 424);

    private void Awake()
    {
        sensor = KinectSensor.GetDefault();
        coordinateMapper = sensor.CoordinateMapper;

        int arraySize = depthResolution.x * depthResolution.y;

        cameraSpacePoints = new CameraSpacePoint[arraySize];
        colorSpacePoints = new ColorSpacePoint[arraySize];
    }

    private void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Space))
        {
            samplePoints = sampleDepthColor();
        }
    }

    private List<SamplePoint> sampleDepthColor()
    {
        List<SamplePoint> samplePoints = new List<SamplePoint>();

        depthData = multiSourceManager.GetDepthData();

        coordinateMapper.MapDepthFrameToCameraSpace(depthData, cameraSpacePoints);
        coordinateMapper.MapDepthFrameToColorSpace(depthData, colorSpacePoints);

        #region Filter points and add passed points to list
        for (int x = 0; x < depthResolution.x; x += sampleResolution)
        {
            for (int y = 0; y < depthResolution.y; y += sampleResolution)
            {
                int sampleIndex = (y * depthResolution.x) + x;
                bool passed = true;

                if (passed)
                {
                    SamplePoint point = new SamplePoint(
                        cameraSpacePoints[sampleIndex],
                        colorSpacePoints[sampleIndex]
                    );

                    samplePoints.Add(point);
                }
            }
        }
        #endregion

        return samplePoints;
    }
}

public class SamplePoint
{
    public CameraSpacePoint cameraSpacePoint;
    public ColorSpacePoint colorSpacePoint;

    public SamplePoint(CameraSpacePoint cameraSpacePoint, ColorSpacePoint colorSpacePoint)
    {
        this.cameraSpacePoint = cameraSpacePoint;
        this.colorSpacePoint = colorSpacePoint;
    }
}