using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSourceManager : MonoBehaviour
{
    private static int size = 25; // Water.shader의 _WaveSources array size와 동일해야 함.
    [SerializeField] private WaveSource waveSourcePrefab;
    public Queue<WaveSource> waveSourceQueue;
    [SerializeField] private SimulationHeadTracker headTracker;
    [SerializeField] private float headSqrSpeedThreshold;
    [SerializeField] private float speedCheckInterval = 0.05f;
    [SerializeField] Material waterMaterial;

    private void Awake()
    {
        waveSourceQueue = new Queue<WaveSource>();

        for (int i = 0; i < size; i++)
        {
            WaveSource waveSource = Instantiate(waveSourcePrefab);
            waveSource.gameObject.SetActive(false);
            waveSource.transform.SetParent(this.transform);
            waveSourceQueue.Enqueue(waveSource);
        }

        StartCoroutine(checkHeadSqrSpeedAndSpawn());
    }

    private void Update()
    {
        waterMaterial.SetVectorArray("_WaveSourcesData", getDataFloat4Array());
    }

    public void Spawn(Vector3 position)
    {
        WaveSource waveSource = waveSourceQueue.Dequeue();
        waveSourceQueue.Enqueue(waveSource); // 다시 맨 뒤에 놓기. empty queue에 dequeue 불렀다가 InvalidOperation exception 뜨지 않게.
        if (waveSource.gameObject.activeInHierarchy) waveSource.gameObject.SetActive(false);

        waveSource.transform.position = position;
        waveSource.gameObject.SetActive(true);
    }

    private IEnumerator checkHeadSqrSpeedAndSpawn()
    {
        while (true)
        {
            if (headTracker.GetSqrSpeed() > headSqrSpeedThreshold)
            {
                Spawn(headTracker.EyePosition);
            }
            yield return new WaitForSeconds(speedCheckInterval);
        }
    }

    private Vector4[] getDataFloat4Array()
    {
        Vector4[] arr = new Vector4[size];

        int index = 0;
        foreach (WaveSource waveSource in waveSourceQueue)
        {
            arr[index++] = waveSource.GetDataFloat4();
        }

        return arr;
    }
}