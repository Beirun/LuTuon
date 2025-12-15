using System;
using System.Collections.Generic;
using System.Text.RegularExpressions; // Required for finding numbers in text
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NumberOfPeopleManager : MonoBehaviour
{
    [Serializable]
    public class IngredientData
    {
        public TMP_Text numberOfPeople;
        public Button addButton;
        public Button minusButton;
        public List<TMP_Text> descriptionText;
    }

    public List<IngredientData> ingredientDataList = new List<IngredientData>();

    // We need a helper class to store the runtime state so we don't lose the original text
    private class RuntimeIngredientState
    {
        public int currentPeopleCount;
        public int basePeopleCount; // The count defined in the inspector at Start
        public List<string> originalDescriptionTemplates = new List<string>();
    }

    private List<RuntimeIngredientState> runtimeStates = new List<RuntimeIngredientState>();

    void Start()
    {
        InitializeIngredients();
    }

    void InitializeIngredients()
    {
        for (int i = 0; i < ingredientDataList.Count; i++)
        {
            int index = i; // Local copy for closure in lambda expressions
            IngredientData data = ingredientDataList[i];

            // 1. Create a state object to track this specific item
            RuntimeIngredientState state = new RuntimeIngredientState();

            // 2. Parse the starting number of people (Default to 1 if empty or invalid)
            if (int.TryParse(data.numberOfPeople.text, out int parsedCount))
            {
                state.currentPeopleCount = parsedCount;
                state.basePeopleCount = parsedCount;
            }
            else
            {
                state.currentPeopleCount = 1;
                state.basePeopleCount = 1;
                data.numberOfPeople.text = "1";
            }

            // 3. Store the original text of descriptions to use as a baseline for math
            foreach (var txt in data.descriptionText)
            {
                state.originalDescriptionTemplates.Add(txt.text);
            }

            runtimeStates.Add(state);

            // 4. Add Listeners to Buttons
            // We use lambda () => functions to pass the specific index
            if (data.addButton != null)
                data.addButton.onClick.AddListener(() => ModifyPeopleCount(index, 1));

            if (data.minusButton != null)
                data.minusButton.onClick.AddListener(() => ModifyPeopleCount(index, -1));
        }
    }

    // Called by buttons
    void ModifyPeopleCount(int index, int amount)
    {
        RuntimeIngredientState state = runtimeStates[index];
        IngredientData uiData = ingredientDataList[index];

        // Update count
        state.currentPeopleCount += amount;

        // Prevent going below 1 person
        if (state.currentPeopleCount < 1)
            state.currentPeopleCount = 1;
        if(state.currentPeopleCount > 50)
            state.currentPeopleCount = 50;

        // Update the visual Number Counter
        uiData.numberOfPeople.text = state.currentPeopleCount.ToString();

        // Recalculate the ingredients
        RecalculateIngredients(index);
    }

    void RecalculateIngredients(int index)
    {
        RuntimeIngredientState state = runtimeStates[index];
        IngredientData uiData = ingredientDataList[index];

        // Calculate the ratio (New Count / Original Count)
        float ratio = (float)state.currentPeopleCount / (float)state.basePeopleCount;

        // Loop through each text block in this ingredient group
        for (int t = 0; t < uiData.descriptionText.Count; t++)
        {
            string originalText = state.originalDescriptionTemplates[t];

            // USE REGEX: Find any number (integer or decimal)
            // Pattern: [0-9]+ followed optionally by .[0-9]+
            string newText = Regex.Replace(originalText, @"\d+(\.\d+)?", (Match match) =>
            {
                // Parse the found number
                if (float.TryParse(match.Value, out float originalValue))
                {
                    // Calculate new value
                    float newValue = originalValue * ratio;

                    // formatting: if it was a whole number originally, keep it whole. 
                    // Otherwise limit decimals to 2 to avoid "907.18999999"
                    if (originalValue % 1 == 0 && newValue % 1 == 0)
                        return newValue.ToString("0"); // Integer format
                    else
                        return newValue.ToString("0.##"); // Max 2 decimal places
                }
                return match.Value; // Fallback if parse fails
            });

            // Apply new text to UI
            uiData.descriptionText[t].text = newText;
        }
    }
}