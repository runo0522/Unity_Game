using UnityEngine;

public class MinigameScrollManager : MonoBehaviour
{
    [Header("Scroll Speed")]
    public float baseScrollSpeed = 1.5f;
    public float speedIncreasePerSecond = 0.02f;
    public float maxScrollSpeed = 3.0f;

    [Header("Stop Magic")]
    public float stopDuration = 1.0f;

    private float currentScrollSpeed;
    private float stopTimer;
    private bool isStopped;
    private float elapsedTime;

    public float CurrentScrollSpeed => currentScrollSpeed;
    public bool IsStopped => isStopped;

    private void Start()
    {
        currentScrollSpeed = baseScrollSpeed;
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        if (isStopped)
        {
            stopTimer -= Time.deltaTime;

            if (stopTimer <= 0f)
            {
                isStopped = false;
            }

            return;
        }

        currentScrollSpeed = Mathf.Min(
            baseScrollSpeed + elapsedTime * speedIncreasePerSecond,
            maxScrollSpeed
        );

        transform.position += Vector3.up * currentScrollSpeed * Time.deltaTime;
    }

    public bool TryStopScroll()
    {
        if (isStopped) return false;

        isStopped = true;
        stopTimer = stopDuration;
        return true;
    }

    public void ForceStopScroll(float duration)
    {
        isStopped = true;
        stopTimer = duration;
    }

    public void ResetScrollSpeed()
    {
        elapsedTime = 0f;
        currentScrollSpeed = baseScrollSpeed;
    }
}