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

                    // Process the group of steps associated with this script
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
            // SUCCESS! 
            // We found a step that is ready to be done. We execute this one and ignore any potential errors in other steps.
            validStep.isDone = true;
            validStep.usedCounter++;
            stepFinishedFlag = true;

            Debug.LogWarning($"Success: Step {validStep.step} finished.");

            // Reset script and exit
            field.SetValue(activeScript, false);
            return;
        }

        // 2. SEARCH FOR PREREQUISITE FAILURE
        // If we are here, it means NO step was ready.
        // Is there a step that is waiting to be done, but is blocked by a prerequisite?
        var blockedStep = group.FirstOrDefault(x => !x.isDone && !ArePrerequisitesMet(x));

        if (blockedStep != null)
        {
            // Find the specific missing prerequisite for the log
            int missingPrereq = GetMissingPrerequisite(blockedStep);
            Debug.LogWarning($"Penalty: Step {blockedStep.step} failed. Prerequisite step {missingPrereq} not finished.");

            point -= 2;
            field.SetValue(activeScript, false);
            return;
        }

        // 3. SEARCH FOR "ONLY ONCE" FAILURE
        // If we are here, it means all steps for this script are likely already done.
        var completedStep = group.LastOrDefault(x => x.isDone);

        if (completedStep != null && completedStep.isOnlyOnce)
        {
            Debug.LogWarning($"Penalty: Step {completedStep.step} performed more than once.");
            point -= 2;
            field.SetValue(activeScript, false);
            return;
        }

        // Safety reset if nothing matched (rare)
        field.SetValue(activeScript, false);
    }

    // Helper to check if all prerequisites for a specific entry are done
    bool ArePrerequisitesMet(ControlledScript cs)
    {
        foreach (int reqStep in cs.prerequisiteSteps)
        {
            if (reqStep == 0) continue; // Ignore 0

            var prereqScript = controlledScripts.Find(x => x.step == reqStep);

            // If the prerequisite step exists and is NOT done, return false
            if (prereqScript != null && !prereqScript.isDone)
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
            var prereqScript = controlledScripts.Find(x => x.step == reqStep);
            if (prereqScript != null && !prereqScript.isDone) return reqStep;
        }
        return -1;
    }

    void CheckIfAllComplete()
    {
        bool allFinished = true;
        foreach (var cs in controlledScripts)
        {
            if (!cs.isDone)
            {
                allFinished = false;
                break;
            }
        }

        if (allFinished)
        {
            gameEnded = true;
            Debug.Log("All steps completed. Waiting to finish...");
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
        point = Math.Max(point, 0);
        if (attemptManager != null) attemptManager.SendAttempt(foodId, point, "Standard");
        if (endManager != null) endManager.OpenDialog("EndGame");
    }
}