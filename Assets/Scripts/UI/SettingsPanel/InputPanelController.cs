using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.InputSystem.InputActionRebindingExtensions;
public class InputPanelController : MonoBehaviour
{
    //reference to the InputActionAsset that contains the actions we want to rebind
    [SerializeField] private InputActionAsset inputActionsAsset;

    // references to the UI elements for each action
    private RebindItemUI[] rebindItems;

    [Header("Slider")]
    [SerializeField] private Slider mouseSensitivitySlider;

    [SerializeField] private MainMenuUIManager mainMenuManager;

    // currently active rebinding operation
    private RebindingOperation currentRebind;

    private void Start()
    {
        Debug.Log("InputPanelController Start");

        rebindItems = GetComponentsInChildren<RebindItemUI>(true);
        Debug.Log("Found rebind items: " + rebindItems.Length);

        foreach (var item in rebindItems)
        {
            Debug.Log($"Item: {item.name}, Button: {item.Button}, Label: {item.Label}, Action: {item.ActionReference}, BindingIndex: {item.BindingIndex}");

            // set up the button listener for this item
            if (item.Button != null)
            {
                RebindItemUI capturedItem = item;
                item.Button.onClick.AddListener(() => StartRebind(capturedItem));
            }

            RefreshLabel(item);
        }

        //// load any previously saved binding overrides
        //LoadBindingOverrides();

        mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
    }

    private void StartRebind(RebindItemUI item)
    {
        // get item action
        InputAction action = item.ActionReference.action;

        // check the binding index
        if (item.BindingIndex < 0 || item.BindingIndex >= action.bindings.Count)
        {
            Debug.LogWarning($"Invalid binding index {item.BindingIndex} for action {action.name}");
            return;
        }

        // get the binding key
        var binding = action.bindings[item.BindingIndex];

        // check if it's a composite binding (like "WASD" -> a lot included keys)
        if (binding.isComposite)
        {
            Debug.LogWarning($"Binding index {item.BindingIndex} is a COMPOSITE HEADER, not a real key binding.");
            return;
        }

        // cancel previous bindings
        currentRebind?.Cancel();
        currentRebind?.Dispose();

        item.Label.text = "Press a key...";
        action.Disable();

        // listen for a new key press and rebind the action
        currentRebind = action.PerformInteractiveRebinding(item.BindingIndex)
            // accept only keyboard keys
            .WithControlsHavingToMatchPath("<Keyboard>")
            // ignore mouse buttons
            .WithControlsExcluding("<Mouse>")
            // don't allow escape key
            .WithCancelingThrough("<Keyboard>/escape")
            .WithMatchingEventsBeingSuppressed()
            .OnPotentialMatch(op =>
            {
                Debug.Log("Potential match: " + op.selectedControl);
            })
            .OnComplete(op =>
            {
                Debug.Log("REBINDED TO: " + op.selectedControl.path);

                action.Enable();
                op.Dispose();
                currentRebind = null;

                RefreshLabel(item);
                SaveBindingOverrides();
            })
            .OnCancel(op =>
            {
                Debug.Log("REBIND CANCELED");

                action.Enable();
                op.Dispose();
                currentRebind = null;

                RefreshLabel(item);
            });

        currentRebind.Start();
    }

    private void RefreshLabel(RebindItemUI item)
    {
        if (item.ActionReference == null || item.ActionReference.action == null || item.Label == null)
            return;

        item.Label.text = item.ActionReference.action.GetBindingDisplayString(item.BindingIndex);
    }

    private void SaveBindingOverrides()
    {
        if (inputActionsAsset == null)
            return;

        string json = inputActionsAsset.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("InputRebinds", json);
        PlayerPrefs.Save();
    }

    //private void LoadBindingOverrides()
    //{
    //    if (inputActionsAsset == null || !PlayerPrefs.HasKey("InputRebinds"))
    //        return;

    //    string json = PlayerPrefs.GetString("InputRebinds");
    //    inputActionsAsset.LoadBindingOverridesFromJson(json);

    //    foreach (var item in rebindItems)
    //    {
    //        RefreshLabel(item);
    //    }
    //}

    private void OnMouseSensitivityChanged(float value)
    {
        Debug.Log("Mouse sensitivity changed: " + value);
    }

    private void OnDestroy()
    {
        currentRebind?.Dispose();
    }
}
