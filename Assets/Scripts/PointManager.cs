using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Required for LINQ
using System.Reflection;
using UnityEngine;

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

    public List<ControlledScript> controlledScripts = new List<ControlledScript>();
    public int point = 100;

    [Header("Dependencies")]
    public AttemptManager attemptManager;
    public DialogManager endManager;

    [Header("Food Id")]
    public string foodId = "30045842-6118-4539-8577-07181b09dfc9";

    [Header("Correct Minute Timer (for reference)")]
    public List<int> correctMinuteTimer = new List<int>();

    // Tracks which index of the correctMinuteTimer list we should compare against next
    private int timerCheckIndex = 0;

    private bool gameEnded = false;
    private HashSet<MonoBehaviour> processedScriptsThisFrame = new HashSet<MonoBehaviour>();

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
        // Get all entries that use this specific script
        var group = controlledScripts.Where(x => x.script == activeScript).OrderBy(x => x.step).ToList();

        // 1. SEARCH FOR A VALID SUCCESSFUL STEP
        // We look for a step that is NOT done, and where ALL prerequisites are met.
        var validStep = group.FirstOrDefault(x => !x.isDone && ArePrerequisitesMet(x));

        if (validStep != null)
        {
            // --- TIMER VALIDATION LOGIC ---
            // Check if this script is a TimerController by name
            if (activeScript.GetType().Name == "TimerController")
            {
                // Reflection to get 'minuteInput'
                FieldInfo minuteField = activeScript.GetType().GetField("minuteInput");

                if (minuteField != null)
                {
                    int actualMinutes = (int)minuteField.GetValue(activeScript);

                    // Ensure we haven't run out of correct times to check
                    if (timerCheckIndex < correctMinuteTimer.Count)
                    {
                        int expectedMinutes = correctMinuteTimer[timerCheckIndex];

                        if (actualMinutes != expectedMinutes)
                        {
                            Debug.LogWarning($"Penalty: Timer set to {actualMinutes}, expected {expectedMinutes}.");
                            point -= 2;
                        }
                        else
                        {
                            Debug.Log($"Timer Correct: {actualMinutes} minutes.");
                        }

                        // Increment index so the next timer event checks the next number in the list
                        timerCheckIndex++;
                    }
                }
            }
            // ------------------------------

            // SUCCESS
            validStep.isDone = true;
            validStep.usedCounter++;
            stepFinishedFlag = true;

            Debug.LogWarning($"Success: Step {validStep.step} finished.");

            field.SetValue(activeScript, false);
            return;
        }

        // 2. SEARCH FOR PREREQUISITE FAILURE
        // Is there a step waiting to be done, but blocked by prerequisites?
        var blockedStep = group.FirstOrDefault(x => !x.isDone && !ArePrerequisitesMet(x));

        if (blockedStep != null)
        {
            int missingPrereq = GetMissingPrerequisite(blockedStep);
            Debug.LogWarning($"Penalty: Step {blockedStep.step} failed. Prerequisite step {missingPrereq} not finished.");

            point -= 2;
            field.SetValue(activeScript, false);
            return;
        }

        // 3. SEARCH FOR "ONLY ONCE" FAILURE
        // If all steps are done, checking if the player is repeating an action they shouldn't
        var completedStep = group.LastOrDefault(x => x.isDone);

        if (completedStep != null && completedStep.isOnlyOnce)
        {
            Debug.LogWarning($"Penalty: Step {completedStep.step} performed more than once.");
            point -= 2;
            field.SetValue(activeScript, false);
            return;
        }

        // Safety reset
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