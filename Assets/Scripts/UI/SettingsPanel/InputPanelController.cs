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

    [Header("Slider")]
    [SerializeField] private Slider mouseSensitivitySlider;

    [SerializeField] private MainMenuUIManager mainMenuManager;

    // references to the UI elements for each action 
    private RebindItemUI[] rebindItems;
    // currently active rebinding operation
    private RebindingOperation currentRebind;

    private void Start()
    {
        LoadInputSettings();

        rebindItems = GetComponentsInChildren<RebindItemUI>(true);

        foreach (var item in rebindItems)
        {
            //Debug.Log($"Item: {item.name}, Button: {item.Button}, Label: {item.Label}, Action: {item.ActionReference}, BindingIndex: {item.BindingIndex}");

            // set up the button listener for this item
            if (item.Button != null)
            {
                RebindItemUI capturedItem = item;
                item.Button.onClick.AddListener(() => StartRebind(capturedItem));
            }

            RefreshLabel(item);
        }

        mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
    }

    private void LoadInputSettings()
    {
       if(SettingsSaveLoadManager.Instance == null || SettingsSaveLoadManager.Instance.CurrentSettings == null)
       {
            Debug.LogWarning("SettingsManager or CurrentSettings is missing.");
            return;
        }

       SettingsData settings = SettingsSaveLoadManager.Instance.CurrentSettings;

        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.SetValueWithoutNotify(settings.mouseSensitivity);
        }

        LoadBindingOverridesFromSettings();
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
                SaveBindingOverridesToSettings();
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

    private void SaveBindingOverridesToSettings()
    {
        if (inputActionsAsset == null)
        {
            Debug.LogWarning("InputActionsAsset is missing.");
            return;
        }

        string json = inputActionsAsset.SaveBindingOverridesAsJson();
        Debug.Log("Saved binding overrides JSON: " + json);

        SettingsSaveLoadManager.Instance.CurrentSettings.inputBindingOverridesJson = json;
        SettingsSaveLoadManager.Instance.SaveSettings();

    }

    private void LoadBindingOverridesFromSettings()
    {
        if (inputActionsAsset == null)
            return;

        if (SettingsSaveLoadManager.Instance == null || SettingsSaveLoadManager.Instance.CurrentSettings == null)
            return;

        string json = SettingsSaveLoadManager.Instance.CurrentSettings.inputBindingOverridesJson;

        if (string.IsNullOrEmpty(json))
        {
            Debug.Log("No saved input binding overrides found.");
            return;
        }

        inputActionsAsset.LoadBindingOverridesFromJson(json);

        Debug.Log("Input binding overrides loaded from settings.json");
    }

    private void OnMouseSensitivityChanged(float value)
    {
        SettingsSaveLoadManager.Instance.CurrentSettings.mouseSensitivity = value;
        Debug.Log("Mouse sensitivity changed: " + value);

        SettingsSaveLoadManager.Instance.SaveSettings();
    }

    private void OnDestroy()
    {
        currentRebind?.Dispose();
    }
}
