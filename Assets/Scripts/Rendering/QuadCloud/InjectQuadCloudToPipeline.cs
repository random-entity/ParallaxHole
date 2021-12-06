using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class InjectQuadCloudToPipeline : MonoBehaviour
{
    private Camera cam;
    private CommandBuffer commandBuffer;
    private CameraEvent cameraEvent;
    [SerializeField] private Material quadCloudMaterial;

    private void OnEnable()
    {
        cam = GetComponent<Camera>();
        cameraEvent = CameraEvent.BeforeDepthTexture;
    }

    private void OnDisable()
    {
        ClearCommandBuffer();
    }

    private void OnPreRender()
    {
        // UpdateCommandBuffer();
    }

    private void InitCommandBuffer()
    {
        if (commandBuffer == null)
        {
            commandBuffer = new CommandBuffer();
            commandBuffer.name = "QuadCloudInjection";
            commandBuffer.DrawProcedural(Matrix4x4.identity, quadCloudMaterial, 0, MeshTopology.Triangles, 6, 512 * 424, QuadCloud.properties);

            cam.AddCommandBuffer(cameraEvent, commandBuffer);

            Debug.LogFormat("Added CommandBuffer in {0}", cameraEvent);
        }
        else
        {
            Debug.Log("CommandBuffer already is != null. Use UpdateCommandBuffer to update pre-existing commandBuffer");
        }
    }

    // private void UpdateCommandBuffer()
    // {
    //     if(commandBuffer == null) {

    //     }
    // }

    private void ClearCommandBuffer()
    {
        if (commandBuffer != null)
        {
            cam.RemoveCommandBuffer(cameraEvent, commandBuffer);
            commandBuffer.Clear();
            commandBuffer.Dispose();
            commandBuffer = null;

            Debug.LogFormat("Removed CommandBuffer in {0}", cameraEvent);
        }
    }
}