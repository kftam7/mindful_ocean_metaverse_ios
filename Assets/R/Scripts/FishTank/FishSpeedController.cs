using UnityEngine;
using Random = UnityEngine.Random;

public class FishSpeedController : MonoBehaviour
{
    public FishEntity Entity { get; set; }
    public bool ChangeSpeedRandomly { get; set; }
    private const float MaxSpeedTimeScale = 1.2f;
    private const float MinSpeedTimeScale = 0.9f;
    private const float SpeedChangingDuration = 1f;
    private const float SpeedChangePeriod = 5f;
    private float originalSpeed;
    private float targetSpeed;
    private float currentSpeed;
    private float speedChangeProgress = 2;
    private float timeCount;
    private float speedScale = 1;

    private void OnEnable()
    {
        currentSpeed = Random.Range(Entity.MinSpeed, Entity.MaxSpeed);
    }

    private void Update()
    {
        if (ChangeSpeedRandomly)
        {
            AutoChangeSpeedRandomly();
        }

        if (speedChangeProgress <= 1)
        {
            speedChangeProgress += Time.deltaTime / SpeedChangingDuration;
            currentSpeed = Mathf.Lerp(originalSpeed, targetSpeed, speedChangeProgress);
            UpdateAnimationTimeScale();
        }
    }

    public void SetSpeedScale(float scale)
    {
        speedScale = scale;
    }

    public float GetSpeedScale()
    {
        return speedScale;
    }

    public void SetSpeed(float speed)
    {
        originalSpeed = currentSpeed;
        targetSpeed = speed;

        if (!targetSpeed.EqualCompare(originalSpeed))
        {
            speedChangeProgress = 0;
        }
    }

    public float GetSpeed()
    {
        return currentSpeed * speedScale;
    }

    private void AutoChangeSpeedRandomly()
    {
        timeCount += Time.deltaTime;
        if (!(timeCount >= SpeedChangePeriod)) return;
        SetRandomSpeed(Entity.MinSpeed, Entity.MaxSpeed);
        timeCount = 0;
    }

    private void UpdateAnimationTimeScale()
    {
        float max = Entity.MaxSpeed * speedScale;
        float min = Entity.MinSpeed * speedScale;

        float timeScale = (MaxSpeedTimeScale - MinSpeedTimeScale) / (max - min) * (currentSpeed * speedScale - min) + MinSpeedTimeScale;

        Entity.SetTimeScale(timeScale);
    }

    private void SetRandomSpeed(float min, float max)
    {
        SetSpeed(Random.Range(min, max));
    }
}