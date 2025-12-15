using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
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

        [HideInInspector] public int basePeople;
        [HideInInspector] public List<float> baseAmounts = new List<float>();
        [HideInInspector] public List<string> suffixes = new List<string>();
    }

    public List<IngredientData> ingredientDataList = new List<IngredientData>();

    static readonly Regex lineRegex = new Regex(
        @"^\s*([0-9]*\.?[0-9]+)\s*(.*)$",
        RegexOptions.Compiled
    );

    void Start()
    {
        foreach (var d in ingredientDataList)
        {
            if (!int.TryParse(d.numberOfPeople.text, out d.basePeople))
                d.basePeople = 1;

            d.baseAmounts.Clear();
            d.suffixes.Clear();

            foreach (var t in d.descriptionText)
            {
                var m = lineRegex.Match(t.text);
                if (!m.Success)
                {
                    d.baseAmounts.Add(0f);
                    d.suffixes.Add(t.text);
                    continue;
                }

                float amt = float.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
                string suffix = m.Groups[2].Value;

                d.baseAmounts.Add(amt);
                d.suffixes.Add(suffix);
            }

            d.addButton.onClick.AddListener(() => ChangePeople(d, 1));
            d.minusButton.onClick.AddListener(() => ChangePeople(d, -1));
        }
    }

    void ChangePeople(IngredientData d, int delta)
    {
        int current = int.Parse(d.numberOfPeople.text);
        current = Mathf.Max(1, current + delta);
        d.numberOfPeople.text = current.ToString();

        float factor = (float)current / d.basePeople;

        for (int i = 0; i < d.descriptionText.Count; i++)
        {
            float newAmt = d.baseAmounts[i] * factor;
            string formatted = newAmt.ToString("0.##", CultureInfo.InvariantCulture);
            d.descriptionText[i].text = formatted + " " + d.suffixes[i];
        }
    }
}
