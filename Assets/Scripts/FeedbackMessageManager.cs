using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

[Serializable]
public class FeedbackEntry
{
    public string message;
    public string emotion;
    public string ingredient;
}
// Start is called once before the first execution of Update after the MonoBehaviour is created
[Serializable]
public class FeedbackEmotion
{
    public string name;
    public Sprite sprite;
}
public class FeedbackMessageManager : MonoBehaviour
{
    
    public List<FeedbackEmotion> goodEmotions = new List<FeedbackEmotion>();
    public List<FeedbackEmotion> badEmotions = new List<FeedbackEmotion>();
    [SerializeField] private TextAsset jsonFile;
    public List<FeedbackEntry> feedbackEntries;

    void Start()
    {
        LoadJson();
    }
    void LoadJson()
    {
        if (jsonFile == null)
        {
            Debug.LogError("JSON file not assigned");
            return;
        }

        try
        {
            feedbackEntries = JsonHelper.FromJson<FeedbackEntry>(jsonFile.text);
            Debug.Log($"Loaded {feedbackEntries.Count} feedback entries");
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }
   

    public FeedbackEmotion GetFeedbackEmotion(int value)
    {
        if (value == 1)
        {
            int index = UnityEngine.Random.Range(0, goodEmotions.Count);
            return goodEmotions[index];
        }
        else if (value == -1)
        {
            int index = UnityEngine.Random.Range(0, badEmotions.Count);
            return badEmotions[index];
        }
        return null;
    }

    public string GetFeedbackBasedOnEmotionAndIngredient(string emotion, string ingredient = "none")
    {
        List<FeedbackEntry> filteredEntries = feedbackEntries.FindAll(entry =>
            entry.emotion.ToLower().Trim() == emotion.ToLower().Trim() &&
            (ingredient.ToLower().Trim() == "none".ToLower().Trim() ? true: entry.ingredient.ToLower().Trim() == ingredient.ToLower().Trim())
        );
        if (filteredEntries.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, filteredEntries.Count);
            return filteredEntries[randomIndex].message;
        }
        else
        {
            return "You did great! Probably?";
        }
    }
}
public static class JsonHelper
{
    public static List<T> FromJson<T>(string json)
    {
        string wrapped = $"{{\"Items\":{json}}}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(wrapped);
        return wrapper.Items;
    }

    [Serializable]
    private class Wrapper<T>
    {
        public List<T> Items;
    }
}