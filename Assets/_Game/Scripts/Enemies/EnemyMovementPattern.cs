using UnityEngine;

public class EnemyMovementPattern : MonoBehaviour
{
    public enum PatternType
    {
        LightVerticalCircle,
        MediumDepthCircle,
        HeavyHorizontalSweep
    }

    [Header("Pattern")]
    public PatternType pattern = PatternType.LightVerticalCircle;

    [Header("Motion")]
    [Tooltip("Movement radius in world units.")]
    public float radius = 1.5f;

    [Tooltip("Cycles per second.")]
    public float frequency = 0.6f;

    [Tooltip("Randomize the starting phase so enemies don't sync.")]
    public bool randomizePhase = true;

    private Transform cameraTransform;
    private Vector3 basePosition;
    private Vector3 basisRight;
    private Vector3 basisUp;
    private Vector3 basisForward;
    private float phaseOffset;

    void Awake()
    {
        cameraTransform = Camera.main != null ? Camera.main.transform : null;
        basePosition = transform.position;
        CacheBasis();

        phaseOffset = randomizePhase ? Random.Range(0f, Mathf.PI * 2f) : 0f;
    }

    void Update()
    {
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
            CacheBasis();
        }

        float t = (Time.time * Mathf.PI * 2f * frequency) + phaseOffset;

        Vector3 offset = Vector3.zero;
        switch (pattern)
        {
            case PatternType.LightVerticalCircle:
                offset = (basisUp * Mathf.Sin(t) + basisRight * Mathf.Cos(t)) * radius;
                break;
            case PatternType.MediumDepthCircle:
                offset = (basisForward * Mathf.Sin(t) + basisRight * Mathf.Cos(t)) * radius;
                break;
            case PatternType.HeavyHorizontalSweep:
                offset = basisRight * Mathf.Sin(t) * radius;
                break;
        }

        transform.position = basePosition + offset;
    }

    private void CacheBasis()
    {
        if (cameraTransform != null)
        {
            basisRight = cameraTransform.right;
            basisUp = cameraTransform.up;
            basisForward = cameraTransform.forward;
        }
        else
        {
            basisRight = Vector3.right;
            basisUp = Vector3.up;
            basisForward = Vector3.forward;
        }
    }
}
