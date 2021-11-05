using System.Collections;
using UnityEngine;

public class WaveSource : MonoBehaviour
{
    private static readonly float maxTime = 4f;
    [SerializeField] private float timeSinceEnabled; // 0에서 시작하는 게 있는 게 파동 계산에 편하니까

    public float GetProgress()
    {
        return timeSinceEnabled / maxTime;
    }

    private void OnEnable()
    {
        timeSinceEnabled = 0f;
        StartCoroutine(CountdownDisable());
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

    public Vector4 GetFloat4Info()
    {
        return new Vector4
        (
            transform.position.x,
            transform.position.z,
            timeSinceEnabled / maxTime,
            gameObject.activeInHierarchy ? 1f : 0f
        );
    }

    private void Update()
    {
        transform.localScale = Vector3.one * (1f - timeSinceEnabled / maxTime);
    }
}