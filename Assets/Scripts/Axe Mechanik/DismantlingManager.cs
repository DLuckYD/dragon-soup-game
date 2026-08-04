using UnityEngine;

public class DismantlingManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private Transform raycastOrigin;
    [SerializeField] private DismantleProgressUI progressUI;

    [Header("Input")]
    [SerializeField] private KeyCode dismantleKey = KeyCode.K;

    [Header("Axe Check")]
    [SerializeField] private string requiredHeldItemId = "axe_breaker";

    [Header("Raycast")]
    [SerializeField] private float dismantleDistance = 2.5f;
    [SerializeField] private LayerMask dismantleLayerMask = ~0;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private DismantleTarget currentTarget;
    private float currentProgressTime;
    private bool isDismantling;
    private bool dismantleSoundStarted;
    private bool isPreparingAxe;
    private bool hasStartedAxeSwing;

    private AxeDismantleRotation currentAxeRotation;

    private string lastFailReason = "";

    private void Awake()
    {
        if (playerInteraction == null)
            playerInteraction = GetComponent<PlayerInteraction>();

        if (raycastOrigin == null && playerInteraction != null)
            raycastOrigin = playerInteraction.playerCameraTransform;

        if (progressUI != null)
            progressUI.Hide();
    }

    private void Update()
    {
        HandleDismantling();
    }

    private void HandleDismantling()
    {
        if (!Input.GetKey(dismantleKey))
        {
            ResetProgress();
            lastFailReason = "";
            return;
        }

        if (Input.GetKeyDown(dismantleKey))
        {
            Log("[DISMANTLE] K pressed. Trying to start dismantling.");
        }

        if (!IsHoldingRequiredItem(out string heldItemId))
        {
            LogFailOnce($"[DISMANTLE] Cannot start. Held item id='{heldItemId}', required='{requiredHeldItemId}'.");
            ResetProgress();
            return;
        }

        DismantleTarget target = GetDismantleTargetInFront();

        if (target == null)
        {
            ResetProgress();
            return;
        }

        if (target.Recipe == null)
        {
            LogFailOnce($"[DISMANTLE] Target '{target.name}' has no dismantle recipe.");
            ResetProgress();
            return;
        }

        lastFailReason = "";

        if (currentTarget != target)
        {
            currentTarget = target;
            currentProgressTime = 0f;

            if (!dismantleSoundStarted)
            {
                dismantleSoundStarted = true;

                if (WwiseAudioManager.Instance != null)
                    WwiseAudioManager.Instance.PostEvent("Axe_Use", gameObject);
            }

            isDismantling = false;
            isPreparingAxe = true;
            hasStartedAxeSwing = false;

            currentAxeRotation = GetHeldAxeRotation();

            if (currentAxeRotation == null)
            {
                LogFailOnce("[DISMANTLE] AxeDismantleRotation was not found on held axe.");
                ResetProgress();
                return;
            }

            currentAxeRotation.PrepareForDismantle();

            if (progressUI != null)
            {
                progressUI.Show();
                progressUI.SetProgress(0f);
            }

            Log($"[DISMANTLE] Found target '{currentTarget.name}'. Preparing axe.");
        }

        if (isPreparingAxe)
        {
            if (currentAxeRotation == null || currentAxeRotation.IsPrepared)
            {
                isPreparingAxe = false;
                isDismantling = true;

                if (currentAxeRotation != null && !hasStartedAxeSwing)
                {
                    currentAxeRotation.PlayDismantleSwing(currentTarget.DismantleTime);
                    hasStartedAxeSwing = true;
                }

                Log($"[DISMANTLE] Started dismantling target '{currentTarget.name}'. Time: {currentTarget.DismantleTime}");
            }
            else
            {
                return;
            }
        }

        currentProgressTime += Time.deltaTime;

        float normalizedProgress = Mathf.Clamp01(currentProgressTime / currentTarget.DismantleTime);

        if (progressUI != null)
            progressUI.SetProgress(normalizedProgress);

        if (normalizedProgress >= 1f)
        {
            DismantleTarget completedTarget = currentTarget;

            ResetProgress();

            if (completedTarget != null)
            {
                Log($"[DISMANTLE] Completed dismantling: {completedTarget.name}");

                if (WwiseAudioManager.Instance != null)
                    WwiseAudioManager.Instance.PostEvent("Item_Demolished", gameObject);

                completedTarget.Dismantle();
            }
        }
    }

    private bool IsHoldingRequiredItem(out string heldItemId)
    {
        heldItemId = "NULL";

        if (playerInteraction == null)
            return false;

        InventoryItem heldItem = playerInteraction.getHeldItem();

        if (heldItem == null)
            return false;

        if (heldItem.itemData == null)
        {
            heldItemId = "NO_ITEM_DATA";
            return false;
        }

        heldItemId = heldItem.itemData.id;

        return heldItem.itemData.id == requiredHeldItemId;
    }

    private AxeDismantleRotation GetHeldAxeRotation()
    {
        if (playerInteraction == null)
            return null;

        InventoryItem heldItem = playerInteraction.getHeldItem();

        if (heldItem == null)
            return null;

        AxeDismantleRotation rotation = heldItem.GetComponentInChildren<AxeDismantleRotation>(true);

        if (rotation != null)
        {
            Log($"[DISMANTLE] Axe rotation found on held item: {heldItem.name}");
            return rotation;
        }

        return null;
    }

    private DismantleTarget GetDismantleTargetInFront()
    {
        if (raycastOrigin == null)
        {
            LogFailOnce("[DISMANTLE] Raycast origin is missing.");
            return null;
        }

        if (Physics.Raycast(
                raycastOrigin.position,
                raycastOrigin.forward,
                out RaycastHit hit,
                dismantleDistance,
                dismantleLayerMask))
        {
            DismantleTarget target = hit.collider.GetComponentInParent<DismantleTarget>();

            if (target == null)
            {
                LogFailOnce($"[DISMANTLE] Raycast hit '{hit.collider.name}', but no DismantleTarget found in parents.");
                return null;
            }

            return target;
        }

        LogFailOnce("[DISMANTLE] Raycast did not hit anything.");
        return null;
    }

    private void ResetProgress()
    {
        if (!isDismantling && !isPreparingAxe && currentTarget == null && currentProgressTime <= 0f && currentAxeRotation == null)
            return;

        if (currentAxeRotation != null)
        {
            currentAxeRotation.StopDismantleSwing();
            currentAxeRotation = null;
        }

        currentTarget = null;
        currentProgressTime = 0f;
        isDismantling = false;
        dismantleSoundStarted = false;
        isPreparingAxe = false;
        hasStartedAxeSwing = false;

        if (WwiseAudioManager.Instance != null)
            WwiseAudioManager.Instance.PostEvent("Stop_Axe_Use", gameObject);

        if (progressUI != null)
            progressUI.Hide();

        Log("[DISMANTLE] Reset progress.");
    }

    private void Log(string message)
    {
        if (showDebugLogs)
            Debug.Log(message);
    }

    private void LogFailOnce(string message)
    {
        if (!showDebugLogs)
            return;

        if (lastFailReason == message)
            return;

        lastFailReason = message;
        Debug.LogWarning(message);
    }
}