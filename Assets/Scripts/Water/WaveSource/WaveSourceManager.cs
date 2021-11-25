using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSourceManager : MonoBehaviour
{
    private static int objectPoolSize = 200; // Water.shader의 _WaveSources array size와 동일해야 함.
    [SerializeField] private WaveSource waveSourcePrefab;
    public Queue<WaveSource> waveSourceQueue;
    public static int activeWaveSourceCount = 0; // WaveSource.OnEnable(), OnDisable()에서 increment, decrement
    private static float activeWaveSourceCountSmooth = 1f;
    [SerializeField] private HeadPositionManager headPositionManager;
    [SerializeField] private float headSpeedThreshold;
    [SerializeField] private float speedCheckInterval;
    [SerializeField] private MeshRenderer waterMeshRenderer;
    private Material waterMaterial;

    private void Awake()
    {
        waveSourceQueue = new Queue<WaveSource>();

        for (int i = 0; i < objectPoolSize; i++)
        {
            WaveSource waveSource = Instantiate(waveSourcePrefab);
            waveSource.gameObject.SetActive(false);
            waveSource.transform.SetParent(this.transform);
            waveSourceQueue.Enqueue(waveSource);
        }

        waterMaterial = waterMeshRenderer.material;
        waterMaterial.SetFloat("_WaveSourceMaxTime", WaveSource.MaxTime);

        StartCoroutine(checkHeadSpeedAndSpawn());
    }

    private void FixedUpdate()
    {
        waterMaterial.SetVectorArray("_WaveSourcesData", getDataFloat4Array());

        activeWaveSourceCountSmooth = Mathf.Lerp(activeWaveSourceCountSmooth, (float)activeWaveSourceCount, 0.02f);
        activeWaveSourceCountSmooth = Mathf.Max(activeWaveSourceCountSmooth, 1f);
        waterMaterial.SetFloat("_ActiveWaveSourceCountSmooth", Mathf.Pow(activeWaveSourceCountSmooth, 0.25f));
    }

    private void spawn(Vector3 worldPosition)
    {
        WaveSource waveSource = waveSourceQueue.Dequeue();
        waveSourceQueue.Enqueue(waveSource); // 다시 맨 뒤에 놓기. empty queue에 dequeue 불렀다가 InvalidOperation exception 뜨지 않게.
        if (waveSource.gameObject.activeInHierarchy) waveSource.gameObject.SetActive(false);

        waveSource.transform.position = worldPosition;
        waveSource.gameObject.SetActive(true);
    }

    private IEnumerator checkHeadSpeedAndSpawn()
    {
        while (true)
        {
            if (headPositionManager.GetSpeed() > headSpeedThreshold)
            {
                spawn(headPositionManager.GetHeadPositionUnityWorldSpace());
            }
            yield return new WaitForSeconds(speedCheckInterval);
        }
    }

    private Vector4[] getDataFloat4Array()
    {
        Vector4[] data = new Vector4[objectPoolSize];

        int index = 0;
        foreach (WaveSource waveSource in waveSourceQueue)
        {
            data[index++] = waveSource.GetDataFloat4();
        }

        return data;
    }
}