using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class TutorialManager : MonoBehaviour
{
    [System.Serializable]
    public class StepControlledScript
    {
        public MonoBehaviour script;
        public int activeStep = -1;
        public string isFinishedBoolName = "isFinished";
        public bool keepEnabled = false;
    }

    public int stepIndex = 1;
    public string dialogPanelName;
    public GameObject showStepButton;
    public TMP_Text uiText;
    public DialogManager dialogManager;
    public AttemptManager attemptManager;  // Assign in inspector

    public List<string> steps = new List<string>();
    public List<StepControlledScript> controlledScripts = new List<StepControlledScript>();

    bool waiting;
    bool isClosed;
    bool isTyping;
    bool fastForward;
    Coroutine typeRoutine;
    Dictionary<MonoBehaviour, Coroutine> blinkCoroutines = new Dictionary<MonoBehaviour, Coroutine>();

    const int LAYER_DEFAULT = 0;
    const int LAYER_OUTLINE = 6;

    void Start()
    {
        if (!dialogManager) dialogManager = FindObjectOfType<DialogManager>();
        if (steps.Count < 1) return;

        DisableAllControlledScripts();
        NextStep();
        showStepButton.SetActive(false);
    }

    void Update()
    {
        HandleBlinking();
        if (!waiting) CheckStepCompletion();
    }

    void CheckStepCompletion()
    {
        bool allFinished = true;

        foreach (var e in controlledScripts)
        {
            if (!e.script) continue;
            if (e.activeStep != stepIndex) continue;

            if (!e.script.enabled)
            {
                allFinished = false;
                break;
            }

            var t = e.script.GetType();
            var field = t.GetField(e.isFinishedBoolName);
            if (field != null && field.FieldType == typeof(bool))
            {
                if (!(bool)field.GetValue(e.script))
                {
                    allFinished = false;
                    break;
                }
            }
        }

        if (allFinished)
        {
            DisableAllControlledScripts();
            StopBlinking();

            if (stepIndex >= steps.Count)
            {
                SendAttemptOnTutorialComplete();
            }
            else
            {
                NextStep();
            }
        }
    }

    void SendAttemptOnTutorialComplete()
    {
        if (attemptManager == null) return;

        string foodId = "tutorialDish";   // Replace with actual ID
        int points = 100;                  // Assign earned points
        string type = "tutorial";          // Attempt type

        attemptManager.SendAttempt(foodId, points, type);
        Debug.Log("Tutorial complete attempt sent.");
    }

    void HandleBlinking()
    {
        foreach (var e in controlledScripts)
        {
            if (e.activeStep != stepIndex) continue;
            if (!e.script) continue;
            if (!e.script.enabled) continue;

            if (e.script is TimerController)
            {
                StopBlink(e.script);
                continue;
            }

            var t = e.script.GetType();
            var f = t.GetField("isDragging");
            if (f == null || f.FieldType != typeof(bool)) continue;

            bool dragging = (bool)f.GetValue(e.script);

            if (!dragging)
            {
                if (!blinkCoroutines.ContainsKey(e.script))
                {
                    Coroutine c = StartCoroutine(BlinkOutline(e.script.gameObject));
                    blinkCoroutines.Add(e.script, c);
                }
            }
            else
            {
                StopBlink(e.script);
            }
        }
    }

    IEnumerator BlinkOutline(GameObject root)
    {
        while (true)
        {
            SetLayerRecursive(root, LAYER_OUTLINE);
            yield return new WaitForSeconds(0.3f);
            SetLayerRecursive(root, LAYER_DEFAULT);
            yield return new WaitForSeconds(0.3f);
        }
    }

    void StopBlink(MonoBehaviour script)
    {
        if (!blinkCoroutines.ContainsKey(script)) return;
        StopCoroutine(blinkCoroutines[script]);
        SetLayerRecursive(script.gameObject, LAYER_DEFAULT);
        blinkCoroutines.Remove(script);
    }

    void StopBlinking()
    {
        foreach (var kv in blinkCoroutines)
        {
            if (kv.Value != null) StopCoroutine(kv.Value);
            if (kv.Key != null) SetLayerRecursive(kv.Key.gameObject, LAYER_DEFAULT);
        }
        blinkCoroutines.Clear();
    }

    void SetLayerRecursive(GameObject obj, int layer)
    {
        if (!obj) return;
        obj.layer = layer;
        foreach (Transform child in obj.transform) SetLayerRecursive(child.gameObject, layer);
    }

    public void ShowStep()
    {
        dialogManager.OpenDialog(dialogPanelName);
        showStepButton.SetActive(false);
        waiting = true;
        isClosed = false;
        StartTypewriter(steps[stepIndex - 1]);
    }

    void StartTypewriter(string text)
    {
        if (typeRoutine != null) StopCoroutine(typeRoutine);
        typeRoutine = StartCoroutine(TypeRoutine(text));
    }

    IEnumerator TypeRoutine(string text)
    {
        isTyping = true;
        fastForward = false;
        uiText.text = "";

        int i = 0;
        while (i < text.Length)
        {
            if (fastForward)
            {
                uiText.text = text;
                break;
            }

            uiText.text += text[i];
            i++;
            yield return new WaitForSeconds(0.02f);
        }

        isTyping = false;
    }

    void NextStep()
    {
        StopBlinking();
        stepIndex++;
        if (stepIndex <= steps.Count)
            ShowStep();
    }

    void DisableAllControlledScripts()
    {
        foreach (var e in controlledScripts)
        {
            if (!e.script) continue;
            var t = e.script.GetType();
            e.script.enabled = false;

            var field = t.GetField(e.isFinishedBoolName);
            if (field != null && field.FieldType == typeof(bool))
                field.SetValue(e.script, false);

            if (e.activeStep <= stepIndex && e.keepEnabled)
                e.script.enabled = true;
        }
    }

    public void CloseStep()
    {
        if (isTyping)
        {
            fastForward = true;
            return;
        }
        if (isClosed) return;

        dialogManager.CloseDialog(dialogPanelName);
        showStepButton.SetActive(true);
        EnableScriptsForStep(stepIndex);
        waiting = false;
        isClosed = true;
    }

    void EnableScriptsForStep(int step)
    {
        foreach (var e in controlledScripts)
        {
            if (!e.script) continue;
            if (e.activeStep != step) continue;

            if (!e.script.enabled) e.script.enabled = true;

            if (e.script is TimerController)
            {
                int time = Int32.Parse(steps[stepIndex - 1].Split(" ")[2]);
                (e.script as TimerController).StartTimer(time);
            }
        }
    }
}
