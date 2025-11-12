using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class DialogueSystem : MonoBehaviour
{
    [Header("Screen-space UI (Yes/No)")]
    public CanvasGroup choiceRoot;
    public TextMeshProUGUI yesLabel;
    public TextMeshProUGUI noLabel;
    public CanvasGroup yesGroup;
    public CanvasGroup noGroup;

    [Header("Floating Bubble (World-space or Screen-space)")]
    public FloatingDialogueBubble bubble;

    [Header("Timings")]
    public float delayBeforeChoices = 0.6f;
    public float fadeInDuration = 0.25f;
    public float fadeOutDurationChosen = 0.25f;
    public float fadeOutDurationOpposite = 0.1f;

    private bool isRunning = false;
    private bool inputEnabled = false;
    private Action<bool> onComplete;

    public void StartYesNoDialogue(
        string question,
        string yesText,
        string noText,
        Transform speaker,
        Action<bool> onCompleteCallback,
        float customDelay = -1f)
    {
        if (isRunning) return;

        onComplete = onCompleteCallback;
        yesLabel.text = yesText;
        noLabel.text = noText;

        if (choiceRoot != null)
        {
            choiceRoot.alpha = 0f;
            choiceRoot.gameObject.SetActive(true);
        }
        if (yesGroup != null) SetGroupAlpha(yesGroup, 0f);
        if (noGroup != null) SetGroupAlpha(noGroup, 0f);

        if (bubble != null)
        {
            bubble.gameObject.SetActive(true);
            bubble.Show(question, speaker);
        }

        float wait = (customDelay >= 0f) ? customDelay : delayBeforeChoices;
        StartCoroutine(RunPrompt(wait));
    }

    IEnumerator RunPrompt(float delay)
    {
        isRunning = true;
        inputEnabled = false;

        if (choiceRoot != null)
            yield return FadeCanvasGroup(choiceRoot, 0f, 1f, fadeInDuration);

        yield return new WaitForSeconds(delay);

        if (yesGroup != null)
            yield return FadeCanvasGroup(yesGroup, 0f, 1f, fadeInDuration);
        if (noGroup != null)
            yield return FadeCanvasGroup(noGroup, 0f, 1f, fadeInDuration);

        inputEnabled = true;

        bool? result = null;
        while (result == null)
        {
            if (inputEnabled)
            {
                if (Input.GetKeyDown(KeyCode.A)) result = true;
                if (Input.GetKeyDown(KeyCode.D)) result = false;
            }
            yield return null;
        }

        if (result.Value)
        {
            if (noGroup != null) yield return FadeCanvasGroup(noGroup, noGroup.alpha, 0f, fadeOutDurationOpposite);
            if (yesGroup != null) yield return FadeCanvasGroup(yesGroup, yesGroup.alpha, 0f, fadeOutDurationChosen);
        }
        else
        {
            if (yesGroup != null) yield return FadeCanvasGroup(yesGroup, yesGroup.alpha, 0f, fadeOutDurationOpposite);
            if (noGroup != null) yield return FadeCanvasGroup(noGroup, noGroup.alpha, 0f, fadeOutDurationChosen);
        }

        if (bubble != null) bubble.Hide();

        if (choiceRoot != null)
        {
            yield return FadeCanvasGroup(choiceRoot, choiceRoot.alpha, 0f, 0.15f);
            choiceRoot.gameObject.SetActive(false);
        }

        inputEnabled = false;
        isRunning = false;

        onComplete?.Invoke(result.Value);
    }

    public void ShowResponseLine(string text, Transform speaker, float holdSeconds = 1.5f, Action onDone = null)
    {
        StartCoroutine(ResponseRoutine(text, speaker, holdSeconds, onDone));
    }

    IEnumerator ResponseRoutine(string text, Transform speaker, float holdSeconds, Action onDone)
    {
        if (bubble != null)
        {
            bubble.gameObject.SetActive(true);
            bubble.Show(text, speaker);
        }

        yield return new WaitForSeconds(holdSeconds);

        if (bubble != null) bubble.Hide();

        onDone?.Invoke();
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        if (cg == null) yield break;

        cg.alpha = from;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / duration);
            cg.alpha = Mathf.Lerp(from, to, k);
            yield return null;
        }
        cg.alpha = to;
    }

    void SetGroupAlpha(CanvasGroup cg, float a)
    {
        if (cg == null) return;
        cg.alpha = a;
        cg.gameObject.SetActive(true);
    }
}
