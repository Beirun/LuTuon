using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Required for LINQ
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class PointManager : MonoBehaviour
{
    [System.Serializable]
    public class ControlledScript
    {
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
    public DragManager manager;

    [Header("Dependencies")]
    public AttemptManager attemptManager;
    public DialogManager endManager;

    [Header("Food Id")]
    public string foodId = "30045842-6118-4539-8577-07181b09dfc9";

    [Header("Correct Minute Timer (for reference)")]
    public List<int> correctMinuteTimer = new List<int>();

    [Header("Progress Bar")]
    public ProgressBarManager progressBarManager;
    public List<ProgressStep> stepsForProgressBar = new List<ProgressStep>(); //if a finished step is in this list, progress bar will start

    // Tracks which index of the correctMinuteTimer list we should compare against next
    private int timerCheckIndex = 0;

    private bool gameEnded = false;
    private HashSet<MonoBehaviour> processedScriptsThisFrame = new HashSet<MonoBehaviour>();

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

        if (aStepWasFinishedThisFrame)
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

            // SUCCESS
            validStep.isDone = true;
            validStep.usedCounter++;
            stepFinishedFlag = true;

            // ---- PROGRESS BAR STARTER ----
            if (progressBarManager != null)
            {
                var ps = stepsForProgressBar
                    .FirstOrDefault(x => x.step == validStep.step);

                if (ps != null && !ps.progressDone && ps.objectToAttach != null)
                {
                    ps.progressDone = true;

                    progressBarManager.StartProgress(
                        ps.objectToAttach,
                        ps.progressValue
                    );
                }
            }
            // ------------------------------


            field.SetValue(activeScript, false);
            return;
        }

        var blockedStep = group.FirstOrDefault(x => !x.isDone && !ArePrerequisitesMet(x));
        if (blockedStep != null)
        {
            point -= 2;
            field.SetValue(activeScript, false);
            return;
        }

        var completedStep = group.LastOrDefault(x => x.isDone);
        if (completedStep != null && completedStep.isOnlyOnce)
        {
            point -= 2;
            field.SetValue(activeScript, false);
            return;
        }

        field.SetValue(activeScript, false);
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

        if (allFinished)
        {
            gameEnded = true;
            Debug.Log("All mandatory steps completed. Waiting to finish...");
            StartCoroutine(WaitAndFinish());
        }
    }

    IEnumerator WaitAndFinish()
    {
        yield return new WaitForSeconds(1f);
        SendAttemptOnComplete();
    }

    void SendAttemptOnComplete()
    {
        if (attemptManager != null) attemptManager.SendAttempt(foodId, point, "Standard");
        if (endManager != null) endManager.OpenDialog("EndGame");
    }
   
}