using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialInputDetector : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference lookAction;

    [Header("Detection Settings")]
    [SerializeField] private float movementThreshold = 0.2f;
    [SerializeField] private float lookThreshold = 10f;

    private float accumulatedMovement;
    private float accumulatedLook;

    private void Update()
    {
        if (DynamicTutorialManager.Instance == null)
            return;

        CheckMovement();
        CheckCameraLook();
    }

    private void CheckMovement()
    {
        if (DynamicTutorialManager.Instance.IsCompleted(TutorialID.Movement))
            return;

        if (moveAction == null || moveAction.action == null)
            return;

        Vector2 movement = moveAction.action.ReadValue<Vector2>();

        // Count movement input over time
        accumulatedMovement += movement.magnitude * Time.deltaTime;

        if (accumulatedMovement >= movementThreshold)
        {
            DynamicTutorialManager.Instance.CompleteTutorial(
                TutorialID.Movement
            );
        }
    }

    private void CheckCameraLook()
    {
        if (!DynamicTutorialManager.Instance.IsCompleted(TutorialID.Movement))
            return;

        if (DynamicTutorialManager.Instance.IsCompleted(TutorialID.CameraLook))
            return;

        if (lookAction == null || lookAction.action == null)
            return;

        Vector2 look = lookAction.action.ReadValue<Vector2>();

        // Accumulate mouse/controller look input
        accumulatedLook += look.magnitude;

        if (accumulatedLook >= lookThreshold)
        {
            DynamicTutorialManager.Instance.CompleteTutorial(
                TutorialID.CameraLook
            );
        }
    }
}