using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RebindItemUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private InputActionReference actionReference;
    [SerializeField] private int bindingIndex;

    public Button Button => button;
    public TextMeshProUGUI Label => label;
    public InputActionReference ActionReference => actionReference;
    public int BindingIndex => bindingIndex;
}