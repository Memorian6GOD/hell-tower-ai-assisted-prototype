using UnityEngine;

public class PlayerAttackAnimation : MonoBehaviour
{
    [SerializeField] private float lungeDistance = 0.2f;

    private Vector3 startLocalPosition;

    private float animationDuration;
    private float animationTimer;

    private bool isPlaying;

    void Awake()
    {
        startLocalPosition = transform.localPosition;
    }

    void Update()
    {
        if (!isPlaying)
            return;

        animationTimer += Time.deltaTime;

        float progress = animationTimer / animationDuration;
        progress = Mathf.Clamp01(progress);

        float lungeProgress = Mathf.Sin(progress * Mathf.PI);

        transform.localPosition =
            startLocalPosition +
            Vector3.forward * lungeDistance * lungeProgress;

        if (progress >= 1f)
        {
            ResetAnimation();
        }
    }

    public void Play(float duration)
    {
        animationDuration = Mathf.Max(duration, 0.01f);
        animationTimer = 0f;
        isPlaying = true;
    }

    public void ResetAnimation()
    {
        isPlaying = false;
        animationTimer = 0f;

        transform.localPosition = startLocalPosition;
    }
}