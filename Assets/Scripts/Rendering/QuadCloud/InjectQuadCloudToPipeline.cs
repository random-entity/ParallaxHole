using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class InjectQuadCloudToPipeline : MonoBehaviour
{
    [SerializeField] private Camera cam;
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

    }

    private void OnPreRender()
    {
        UpdateCommandBuffer();
    }

    private void UpdateCommandBuffer()
    {
        commandBuffer = new CommandBuffer();
        commandBuffer.name = "QuadCloudInjection";
        // commandBuffer.DrawProcedural(Matrix4x4.identity, quadCloudMaterial, 0, MeshTopology.Triangles, 6, 512 * 424, QuadCloud.materialPropertyBlock);

        cam.AddCommandBuffer(cameraEvent, commandBuffer);
    }

    private void ClearCommandBuffer()
    {
        if (commandBuffer != null)
        {
            cam.RemoveCommandBuffer(cameraEvent, commandBuffer);
            commandBuffer.Clear();
            commandBuffer.Dispose();
            commandBuffer = null;
        }
    }
}