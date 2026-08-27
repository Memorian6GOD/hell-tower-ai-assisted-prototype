using UnityEngine;

public class HitEffect : MonoBehaviour
{
    [SerializeField] private float lifetime = 0.15f;
    [SerializeField] private float startScale = 0.2f;
    [SerializeField] private float endScale = 0.6f;

    private float timer;

    void Start()
    {
        timer = 0f;

        transform.localScale = Vector3.one * startScale;
    }

    void Update()
    {
        timer += Time.deltaTime;

        float progress = timer / lifetime;

        transform.localScale = Vector3.Lerp(
            Vector3.one * startScale,
            Vector3.one * endScale,
            progress
        );

        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}