using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DialogueTrigger : MonoBehaviour
{
    [Header("Prompt (on stop)")]
    [TextArea] public string question;
    public string yesText = "(Yes)";
    public string noText = "(No)";
    public Transform speaker;
    public float choiceDelayOverride = -1f;

    [Header("Follow-up Responses (per selection)")]
    [TextArea] public string yesResponse;
    [TextArea] public string noResponse;
    public float responseHoldTime = 1.5f;

    [Header("References")]
    public DialogueSystem dialogueSystem;
    public BoatController boat;

    private bool fired = false;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (fired) return;
        if (other.GetComponent<BoatController>() == null) return;
        if (boat == null || dialogueSystem == null) return;

        fired = true;

        boat.RequestSlowStop(onStopped: () =>
        {
            Transform speakerOrBoat = (speaker != null) ? speaker : boat.transform;

            dialogueSystem.StartYesNoDialogue(
                question,
                yesText,
                noText,
                speakerOrBoat,
                onCompleteCallback: (choseYes) => OnChoiceComplete(choseYes, speakerOrBoat),
                customDelay: choiceDelayOverride
            );
        });
    }

    private void OnChoiceComplete(bool choseYes, Transform speakerOrBoat)
    {
        string line = choseYes ? yesResponse : noResponse;

        if (!string.IsNullOrWhiteSpace(line))
        {
            dialogueSystem.ShowResponseLine(
                line,
                speakerOrBoat,
                responseHoldTime,
                onDone: () =>
                {
                    boat.ResumeControl();
                    var col = GetComponent<Collider2D>();
                    if (col != null) col.enabled = false;
                });
        }
        else
        {
            boat.ResumeControl();
            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }
    }
}
