using Assets.Scripts.UI;
using NCalc;
using UnityEngine;

namespace StationpediaCalculator;

public static class Functions {
    public static void CalculateSearch(ref Stationpedia stationpedia, string inputText) {
        try {
            var expression = new Expression(inputText, EvaluateOptions.IgnoreCase);
            expression.Parameters["Pi"] = Mathf.PI;

            if (expression.HasErrors()) {
                Data.CalculatorItem.gameObject.SetActive(false);
            }
            else {
                stationpedia.NoResultsFromSearchText.SetActive(false);

                Data.CalculatorItem.InsertImage.sprite = stationpedia.ImportantSearchImage;
                Data.CalculatorItem.SetSpecial();
                Data.CalculatorItem.InsertTitle.text = expression.Evaluate().ToString();
                Data.CalculatorItem.gameObject.SetActive(true);
                Data.CalculatorItem.transform.SetSiblingIndex(0);
            }
        }
        catch { }
    }
}