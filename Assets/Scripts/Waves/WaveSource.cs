using System.Collections;
using UnityEngine;

public class WaveSource : MonoBehaviour
{
    public static readonly float MaxTime = 4f;
    [SerializeField] private float timeSinceEnabled; // 0에서 시작하는 게 있는 게 파동 계산에 편하니까
    private MeshRenderer meshRenderer;

    public float GetProgress()
    {
        return timeSinceEnabled / MaxTime;
    }

    private void OnEnable()
    {
        WaveSourceManager.activeWaveSourceCount++;
        timeSinceEnabled = 0f;
        StartCoroutine(CountdownDisable());
    }
    private void OnDisable()
    {
        timeSinceEnabled = MaxTime;
        WaveSourceManager.activeWaveSourceCount--;
    }
    private IEnumerator CountdownDisable()
    {
        while (timeSinceEnabled < MaxTime)
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

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        if (meshRenderer.enabled)
        {
            transform.localScale = Vector3.one * (1f - timeSinceEnabled / MaxTime);
        }
    }
}