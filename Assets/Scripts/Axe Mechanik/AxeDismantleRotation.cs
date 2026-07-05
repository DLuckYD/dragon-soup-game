using UnityEngine;

[DefaultExecutionOrder(100)]
public class AxeDismantleRotation : MonoBehaviour
{
    [SerializeField] private Transform axeVisual;

    [Header("Dismantle Rotation")]
    [SerializeField] private Vector3 dismantleRotationOffsetEuler = new Vector3(90f, 0f, 0f);
    [SerializeField] private float prepareRotationDuration = 0.5f;

    [Header("Swing Settings")]
    [SerializeField] private Vector3 swingEulerAxis = new Vector3(1f, 0f, 0f);
    [SerializeField] private float swingAngle = 45f;
    [SerializeField] private float swingSpeed = 10f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private Quaternion originalVisualLocalRotation;
    private Quaternion prepareStartRotation;
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
    }

    private void LateUpdate()
    {
        if (isPreparing)
        {
            prepareTimer += Time.deltaTime;

            float t = Mathf.Clamp01(prepareTimer / prepareRotationDuration);

            axeVisual.localRotation = Quaternion.Slerp(
                prepareStartRotation,
                dismantleBaseRotation,
                t
            );

            if (showDebugLogs)
            {
                Debug.Log($"[AXE ROTATION] Preparing t={t}, currentEuler={axeVisual.localEulerAngles}");
            }

            if (t >= 1f)
            {
                isPreparing = false;
                isPrepared = true;
                axeVisual.localRotation = dismantleBaseRotation;

                if (showDebugLogs)
                    Debug.Log($"[AXE ROTATION] Axe prepared. Final Euler: {axeVisual.localEulerAngles}");
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

        prepareStartRotation = axeVisual.localRotation;

        dismantleBaseRotation =
            prepareStartRotation * Quaternion.Euler(dismantleRotationOffsetEuler);

        if (showDebugLogs)
        {
            Debug.Log($"[AXE ROTATION] Preparing axe rotation.");
            Debug.Log($"[AXE ROTATION] Start Euler: {prepareStartRotation.eulerAngles}");
            Debug.Log($"[AXE ROTATION] Offset Euler: {dismantleRotationOffsetEuler}");
            Debug.Log($"[AXE ROTATION] Target Euler: {dismantleBaseRotation.eulerAngles}");
        }
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