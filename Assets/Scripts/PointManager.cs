using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Required for LINQ
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PointManager : MonoBehaviour
{
    [System.Serializable]
    public class ControlledScript
    {
        public string name;
        public MonoBehaviour script;
        public bool isDone = false;
        public bool isOnlyOnce = true;

        [Tooltip("If true, the game can finish even if this step is not done.")]
        public bool isOptional = false;

        public int step = 0;
        public int usedCounter = 0;
        public string isFinishedBoolName = "isFinished";
        public List<int> prerequisiteSteps = new List<int>();
    }
    [System.Serializable]
    public class ProgressStep
    {
        public int step;
        public Transform objectToAttach;
        public float progressValue;
        public bool progressDone = false;
    }


    public List<ControlledScript> controlledScripts = new List<ControlledScript>();
    public int point = 100;
    private DragManager manager;
    public TMP_Text pointText;
    public TMP_Text gameFeedback;
    public Image mascot;

    [Header("Dependencies")]
    public AttemptManager attemptManager;
    public DialogManager endManager;

    [Header("Food Id")]
    public string foodId = "30045842-6118-4539-8577-07181b09dfc9";
    
    [Header("Progress Bar")]
    public ProgressBarManager progressBarManager;
    public List<ProgressStep> stepsForProgressBar = new List<ProgressStep>(); //if a finished step is in this list, progress bar will start

    TimerManager timerManager;
    private bool gameEnded = false;
    private HashSet<MonoBehaviour> processedScriptsThisFrame = new HashSet<MonoBehaviour>();

    private int lastFinishedStep = -1;
    private bool isStillRunning = false;
    FeedbackMessageManager feedbackMessageManager;
    void Start()
    {
        if (manager == null)
        {
            manager = FindFirstObjectByType<DragManager>();
        }
        if(progressBarManager == null)
        {
            progressBarManager = FindFirstObjectByType<ProgressBarManager>();
        }
        if (timerManager == null)
        {
            timerManager = FindFirstObjectByType<TimerManager>();
        }
        if(feedbackMessageManager == null)
        {
            feedbackMessageManager = FindFirstObjectByType<FeedbackMessageManager>();
        }

    }
    void Update()
    {
        if (!gameEnded)
        {
            CheckScriptsStatus();
        }
    }

    void CheckScriptsStatus()
    {
        bool aStepWasFinishedThisFrame = false;
        processedScriptsThisFrame.Clear();

        foreach (var cs in controlledScripts)
        {
            if (cs.script == null || !cs.script.enabled) continue;

            // 1. Avoid processing the same physical script multiple times in one frame
            if (processedScriptsThisFrame.Contains(cs.script)) continue;

            Type type = cs.script.GetType();
            FieldInfo field = type.GetField(cs.isFinishedBoolName);

            if (field != null && field.FieldType == typeof(bool))
            {
                bool isFinished = (bool)field.GetValue(cs.script);

                if (isFinished)
                {
                    processedScriptsThisFrame.Add(cs.script);
                    ProcessScriptGroup(cs.script, field, ref aStepWasFinishedThisFrame);
                }
            }
        }

        if (aStepWasFinishedThisFrame || isStillRunning)
        {
            CheckIfAllComplete();
        }
    }

    void ProcessScriptGroup(MonoBehaviour activeScript, FieldInfo field, ref bool stepFinishedFlag)
    {
        var group = controlledScripts.Where(x => x.script == activeScript).OrderBy(x => x.step).ToList();
        var validStep = group.FirstOrDefault(x => !x.isDone && ArePrerequisitesMet(x));
        if (validStep != null)
        {
            var validSteps = controlledScripts.Where(x => !x.isDone && ArePrerequisitesMet(x) && x.step == validStep.step).ToList();
            var sameStepScripts = controlledScripts.Where(x => x.step == validStep.step && x.script != activeScript && !x.isOptional && !x.isDone).ToList();
            // SUCCESS: valid step with prerequisites met
            if((lastFinishedStep <= validStep.step && lastFinishedStep != -1) || sameStepScripts.Count == 0)
            {
                Debug.LogWarning($"active script: {activeScript.name}, count: {validSteps.Count}");
                if (validSteps.Count == 1) StartProgressIfNeeded(validStep);
            }
            validStep.isDone = true;
            validStep.usedCounter++;
            stepFinishedFlag = true;

            lastFinishedStep = validStep.step;
            field.SetValue(activeScript, false);
            return;
        }

        // FALLBACK: mark first unfinished step as done even if blocked or prerequisites not met
        var fallbackStep = group.FirstOrDefault(x => !x.isDone);
        if (fallbackStep != null)
        {
            var fallBackSteps = controlledScripts.Where(x => !x.isDone && x.step == fallbackStep.step).ToList();
            var sameStepScripts = controlledScripts.Where(x => x.step == fallbackStep.step && x.script != activeScript && !x.isOptional && !x.isDone).ToList();

            if ((lastFinishedStep <= fallbackStep.step && lastFinishedStep != -1) || sameStepScripts.Count == 0)
            {

                Debug.LogWarning($"active script: {activeScript.name}, count: {fallBackSteps.Count}");
                if(fallBackSteps.Count == 1) StartProgressIfNeeded(fallbackStep);
            }
            fallbackStep.isDone = true;
            fallbackStep.usedCounter++;
            stepFinishedFlag = true;

            // Trigger progress bar
            lastFinishedStep = fallbackStep.step;
            field.SetValue(activeScript, false);
            point -= 2;
            return;
        }

        // All steps done, optionally deduct points if only-once
        var completedStep = group.LastOrDefault(x => x.isDone);
        if (completedStep != null && completedStep.isOnlyOnce)
        {
            completedStep.usedCounter++;
            point -= 2;
            field.SetValue(activeScript, false);
            return;
        }
        field.SetValue(activeScript, false);
    }

    // Helper method to trigger progress bar if step is in the list
    void StartProgressIfNeeded(ControlledScript step)
    {
        if (progressBarManager != null)
        {
            var ps = stepsForProgressBar.FirstOrDefault(x => x.step == step.step);

            if (ps != null && !ps.progressDone && ps.objectToAttach != null)
            {
                ps.progressDone = true;
                progressBarManager.StartProgress(ps.objectToAttach, ps.progressValue / 2);
            }
        }
    }


    // Helper to check if ALL entries for a specific step ID are done
    bool ArePrerequisitesMet(ControlledScript cs)
    {
        foreach (int reqStep in cs.prerequisiteSteps)
        {
            if (reqStep == 0) continue;

            // Check ALL scripts that share this step ID.
            // If ANY of them are not done, the prerequisite is not met.
            bool isStepFullyComplete = controlledScripts
                                       .Where(x => x.step == reqStep)
                                       .All(x => x.isDone);

            // If the step exists in the list but isn't fully complete, return false
            if (controlledScripts.Any(x => x.step == reqStep) && !isStepFullyComplete)
            {
                return false;
            }
        }
        return true;
    }

    // Helper to get the specific ID of a missing prerequisite (for logging)
    int GetMissingPrerequisite(ControlledScript cs)
    {
        foreach (int reqStep in cs.prerequisiteSteps)
        {
            if (reqStep == 0) continue;

            // Check if there is ANY script with this step ID that is not done
            bool isStepIncomplete = controlledScripts
                                    .Where(x => x.step == reqStep)
                                    .Any(x => !x.isDone);

            if (isStepIncomplete) return reqStep;
        }
        return -1;
    }

    void CheckIfAllComplete()
    {
        bool allFinished = true;
        foreach (var cs in controlledScripts)
        {
            // If a step is NOT done AND NOT optional, then the game isn't finished.
            if (!cs.isDone && !cs.isOptional)
            {
                allFinished = false;
                break;
            }
        }
        if (allFinished && progressBarManager.isRunning && !isStillRunning)
        {
            isStillRunning = true;
            return;
        }
        if (allFinished && !progressBarManager.isRunning)
        {
            
            gameEnded = true;
            Debug.Log("All mandatory steps completed. Waiting to finish...");
            StartCoroutine(WaitAndFinish());
        }
    }

    IEnumerator WaitAndFinish()
    {
        manager.DisableAllDragging();
        yield return new WaitForSeconds(1f);
        SendAttemptOnComplete();
    }

    void SendAttemptOnComplete()
    {
        if (timerManager != null) timerManager.StopTimer();

        int maxUsedCounter = 0;
        string maxScriptName = "";

        for (int i = 0; i < controlledScripts.Count; i++)
        {
            var cs = controlledScripts[i];
            if (!cs.isOnlyOnce || cs.script == null) continue;

            if (cs.usedCounter > maxUsedCounter)
            {
                maxUsedCounter = cs.usedCounter;
                maxScriptName = cs.name;
            }
        }

        pointText.text = point.ToString();
        gameFeedback.text = "Great job! You've completed the game.";

        Debug.Log("Highest usedCounter (isOnlyOnce): " + maxUsedCounter +
                  " | Script: " + maxScriptName);
        if(maxUsedCounter >= 2)
        {
            var feedBackEmotion = feedbackMessageManager.GetFeedbackEmotion(-1);
            Debug.Log("Name: " + feedBackEmotion.name);
            Debug.Log("Max: " + maxScriptName);
            Debug.Log("Value: " + maxScriptName == feedBackEmotion.name);
            mascot.sprite = feedBackEmotion.sprite;
            gameFeedback.text = feedbackMessageManager.GetFeedbackBasedOnEmotionAndIngredient(feedBackEmotion.name, maxScriptName);
        }
        else if (maxUsedCounter < 2 && point < 100)
        {
            var feedBackEmotion = feedbackMessageManager.GetFeedbackEmotion(1);
            mascot.sprite = feedBackEmotion.sprite;
            gameFeedback.text = feedbackMessageManager.GetFeedbackBasedOnEmotionAndIngredient("HAPPY","neutral");
        }
        else
        {
            var feedBackEmotion = feedbackMessageManager.GetFeedbackEmotion(1);
            mascot.sprite = feedBackEmotion.sprite; gameFeedback.text = feedbackMessageManager.GetFeedbackBasedOnEmotionAndIngredient("HAPPY");
        }
        if (attemptManager != null)
            attemptManager.SendAttempt(foodId, point, "Standard");

        if (endManager != null)
            endManager.OpenDialog("EndGame");
    }


}