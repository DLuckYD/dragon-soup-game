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
            return;
        }

        if (!IsHoldingRequiredItem())
        {
            ResetProgress();
            return;
        }

        DismantleTarget target = GetDismantleTargetInFront();

        if (target == null || target.Recipe == null)
        {
            ResetProgress();
            return;
        }

        if (currentTarget != target)
        {
            currentTarget = target;
            currentProgressTime = 0f;
            isDismantling = true;

            if (progressUI != null)
                progressUI.Show();

            if (showDebugLogs)
                Debug.Log($"[DISMANTLE] Started dismantling: {currentTarget.name}");
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
                if (showDebugLogs)
                    Debug.Log($"[DISMANTLE] Completed dismantling: {completedTarget.name}");

                completedTarget.Dismantle();
            }
        }
    }

    private bool IsHoldingRequiredItem()
    {
        if (playerInteraction == null)
            return false;

        InventoryItem heldItem = playerInteraction.getHeldItem();

        if (heldItem == null)
            return false;

        if (heldItem.itemData == null)
            return false;

        return heldItem.itemData.id == requiredHeldItemId;
    }

    private DismantleTarget GetDismantleTargetInFront()
    {
        if (raycastOrigin == null)
            return null;

        if (Physics.Raycast(
                raycastOrigin.position,
                raycastOrigin.forward,
                out RaycastHit hit,
                dismantleDistance,
                dismantleLayerMask))
        {
            return hit.collider.GetComponentInParent<DismantleTarget>();
        }

        return null;
    }

    private void ResetProgress()
    {
        if (!isDismantling && currentTarget == null && currentProgressTime <= 0f)
            return;

        currentTarget = null;
        currentProgressTime = 0f;
        isDismantling = false;

        if (progressUI != null)
            progressUI.Hide();
    }
}