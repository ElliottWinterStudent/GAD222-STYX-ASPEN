using UnityEngine;
using TMPro;
using System.Collections;

public class FloatingDialogueBubble : MonoBehaviour
{
    public RectTransform bubbleRoot;
    public CanvasGroup bubbleGroup;
    public TextMeshProUGUI bubbleText;
    public Vector3 worldOffset = new Vector3(0f, 1.5f, 0f);
    public float fadeDuration = 0.2f;

    private Transform followTarget;
    private Camera cam;

    void Awake()
    {
        cam = Camera.main;
        if (bubbleGroup != null)
        {
            bubbleGroup.alpha = 0f;
            bubbleGroup.interactable = false;
            bubbleGroup.blocksRaycasts = false;
        }

    }

    void LateUpdate()
    {
        if (followTarget == null) return;

        Vector3 worldPos = followTarget.position + worldOffset;
        var parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null && parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            Vector3 screenPos = cam.WorldToScreenPoint(worldPos);
            bubbleRoot.position = screenPos;
        }
        else
        {
            bubbleRoot.position = worldPos;
            if (cam != null) bubbleRoot.forward = cam.transform.forward;
        }
    }

    public void Show(string text, Transform target)
    {
        if (!isActiveAndEnabled)
        {
            Debug.LogError("[FloatingDialogueBubble] Error... Shit Nuggets.");
            return;
        }

        followTarget = target;
        bubbleText.text = text;

        if (bubbleRoot != null && !bubbleRoot.gameObject.activeSelf)
            bubbleRoot.gameObject.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(Fade(0f, 1f, fadeDuration, after =>
        {
            bubbleGroup.interactable = true;
            bubbleGroup.blocksRaycasts = true;
        }));
    }

    public void Hide()
    {
        StopAllCoroutines();
        bubbleGroup.interactable = false;
        bubbleGroup.blocksRaycasts = false;

        StartCoroutine(Fade(bubbleGroup.alpha, 0f, fadeDuration, after =>
        {

        }));
    }

    IEnumerator Fade(float from, float to, float duration, System.Action<bool> onEnd = null)
    {
        float t = 0f;
        bubbleGroup.alpha = from;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / duration);
            bubbleGroup.alpha = Mathf.Lerp(from, to, k);
            yield return null;
        }
        bubbleGroup.alpha = to;
        onEnd?.Invoke(true);
    }
}
