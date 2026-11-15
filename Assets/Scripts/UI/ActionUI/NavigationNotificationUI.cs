
using System.Collections;
using TMPro;
using UnityEngine;

public class NavigationNotificationUI : MonoBehaviour
{
    [Header("Notification settings")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TMP_Text prefixText;
    [SerializeField] private TMP_Text keyText;
    [SerializeField] private TMP_Text actionText;

    [Header("Fade Settings")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeInDuration = 0.25f;
    [SerializeField] private float fadeOutDuration = 0.4f;

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        canvasGroup.alpha = 0f;
        notificationPanel.SetActive(false);
    }

    public void ShowMessage(string prefix, string key, string action)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(
            ShowRoutine(prefix, key, action)
        );
    }

    private IEnumerator ShowRoutine(string prefix, string key, string action)
    {
        // Fade out the previous message before changing text
        if (notificationPanel.activeSelf && canvasGroup.alpha > 0f)
        {
            yield return Fade(0f, fadeOutDuration);
        }

        prefixText.text = prefix;
        keyText.text = key;
        actionText.text = action;

        notificationPanel.SetActive(true);

        yield return Fade(1f, fadeInDuration);

        fadeCoroutine = null;
    }

    public void HideMessage()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(HideRoutine());
    }

    private IEnumerator HideRoutine()
    {
        yield return Fade(0f, fadeOutDuration);

        notificationPanel.SetActive(false);
        fadeCoroutine = null;
    }

    private IEnumerator Fade(float targetAlpha, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        if (duration <= 0f)
        {
            canvasGroup.alpha = targetAlpha;
            yield break;
        }

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            canvasGroup.alpha = Mathf.Lerp(
                startAlpha,
                targetAlpha,
                Mathf.Clamp01(elapsed / duration)
            );

            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }
}
