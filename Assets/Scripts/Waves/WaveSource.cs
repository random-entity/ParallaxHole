using System.Collections;
using UnityEngine;

public class WaveSource : MonoBehaviour
{
    private static readonly float maxTime = 8f;
    [SerializeField] private float timeSinceEnabled; // 0에서 시작하는 게 있는 게 파동 계산에 편하니까

    public float GetProgress()
    {
        return timeSinceEnabled / maxTime;
    }

    private void OnEnable()
    {
        WaveSourceManager.activeWaveSourceCount++;
        timeSinceEnabled = 0f;
        StartCoroutine(CountdownDisable());
    }
    private void OnDisable()
    {
        timeSinceEnabled = maxTime;
        WaveSourceManager.activeWaveSourceCount--;
    }
    private IEnumerator CountdownDisable()
    {
        while (timeSinceEnabled < maxTime)
        {
            timeSinceEnabled += Time.deltaTime;
            yield return null;
        }
        gameObject.SetActive(false);
    }

    public Vector4 GetDataFloat4()
    {
        return new Vector4
        (
            transform.position.x,
            WaterSurfaceY.waterSurfaceY,
            transform.position.z,
            GetProgress()
        );
    }

    private void Update()
    {
        transform.localScale = Vector3.one * (1f - timeSinceEnabled / maxTime);
    }
}