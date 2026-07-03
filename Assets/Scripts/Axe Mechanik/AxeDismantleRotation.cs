using UnityEngine;

[DefaultExecutionOrder(100)]
public class AxeDismantleRotation : MonoBehaviour
{
    [Header("IMPORTANT: drag ONLY visual child here, not root axe object")]
    [SerializeField] private Transform axeVisual;

    [Header("Dismantle Rotation")]
    [SerializeField] private Vector3 dismantleRotationOffsetEuler = new Vector3(0f, 0f, 90f);
    [SerializeField] private float prepareRotationDuration = 0.25f;

    [Header("Swing Settings")]
    [SerializeField] private Vector3 swingEulerAxis = new Vector3(1f, 0f, 0f);
    [SerializeField] private float swingAngle = 45f;
    [SerializeField] private float swingSpeed = 10f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private Quaternion originalVisualLocalRotation;
    private Quaternion dismantleBaseRotation;

    private bool isPreparing;
    private bool isPrepared;
    private bool isSwinging;

    private float prepareTimer;
    private float swingTimer;
    private float swingDuration;

    public bool IsPrepared => isPrepared;

    private void Awake()
    {
        if (axeVisual == null)
        {
            Debug.LogError("[AXE ROTATION] Axe Visual is not assigned. Script disabled.");
            enabled = false;
            return;
        }

        originalVisualLocalRotation = axeVisual.localRotation;

        dismantleBaseRotation =
            originalVisualLocalRotation * Quaternion.Euler(dismantleRotationOffsetEuler);
    }

    private void LateUpdate()
    {
        if (isPreparing)
        {
            prepareTimer += Time.deltaTime;

            float t = Mathf.Clamp01(prepareTimer / prepareRotationDuration);

            axeVisual.localRotation = Quaternion.Slerp(
                originalVisualLocalRotation,
                dismantleBaseRotation,
                t
            );

            if (t >= 1f)
            {
                isPreparing = false;
                isPrepared = true;
                axeVisual.localRotation = dismantleBaseRotation;

                if (showDebugLogs)
                    Debug.Log("[AXE ROTATION] Axe prepared for dismantle.");
            }

            return;
        }

        if (isSwinging)
        {
            swingTimer += Time.deltaTime;

            float angle = Mathf.Sin(swingTimer * swingSpeed) * swingAngle;
            Vector3 swingOffset = swingEulerAxis.normalized * angle;

            axeVisual.localRotation =
                dismantleBaseRotation * Quaternion.Euler(swingOffset);

            if (swingTimer >= swingDuration)
            {
                StopDismantleSwing();
            }
        }
    }

    public void PrepareForDismantle()
    {
        if (axeVisual == null)
            return;

        isPreparing = true;
        isPrepared = false;
        isSwinging = false;

        prepareTimer = 0f;
        swingTimer = 0f;

        if (showDebugLogs)
            Debug.Log("[AXE ROTATION] Preparing axe rotation.");
    }

    public void PlayDismantleSwing(float duration)
    {
        if (axeVisual == null)
            return;

        isPreparing = false;
        isPrepared = true;
        isSwinging = true;

        swingTimer = 0f;
        swingDuration = duration;

        axeVisual.localRotation = dismantleBaseRotation;

        if (showDebugLogs)
            Debug.Log($"[AXE ROTATION] Swing started. Duration: {duration}");
    }

    public void StopDismantleSwing()
    {
        if (axeVisual == null)
            return;

        isPreparing = false;
        isPrepared = false;
        isSwinging = false;

        prepareTimer = 0f;
        swingTimer = 0f;

        axeVisual.localRotation = originalVisualLocalRotation;

        if (showDebugLogs)
            Debug.Log("[AXE ROTATION] Swing stopped.");
    }
}