#region

using NCalc;
using UnityObject = UnityEngine.Object;

#endregion

namespace StationpediaCalculator;

internal static class Functions {
    internal static void CalculateSearch(string searchText) {
        Expression expression = new(searchText, EvaluateOptions.IgnoreCase);

        if (expression.HasErrors()) {
            ConfigData.CalculatorItem.gameObject.SetActive(false);
        }
        else {
            object result = "";
            try {
                result = expression.Evaluate();
            }
            catch (EvaluationException) { } // only catch EvaluationException

            string text = result?.ToString() ?? "invalid";

            ConfigData.CalculatorItem.gameObject.SetActive(true);
            ConfigData.CalculatorItem.transform.SetSiblingIndex(0);

            ConfigData.CalculatorItem.InsertTitle.text = text;
            ConfigData.CalculatorItem.InsertImage.sprite = Stationpedia.Instance.ImportantSearchImage;
            ConfigData.CalculatorItem.SetSpecial();

            ConfigData.CalculatorItem.InsertsButton.onClick.AddListener(async () => {
                Stationpedia.Instance.BaseAnimator.SetBool("Copied", true);
                GameManager.Clipboard = text;
                await UniTask.Delay(750);
                Stationpedia.Instance.ResetClipboardNotification();
            });

            Stationpedia.Instance.NoResultsFromSearchText.SetActive(false);
        }
    }

    internal static void CreateCalculator(ref List<SPDAListItem> items) {
        SPDAListItem calculatorItem = UnityObject.Instantiate(Stationpedia.Instance.ListInsertPrefab, Stationpedia.Instance.SearchContents);
        calculatorItem.gameObject.SetActive(false);

        ConfigData.CalculatorItem = calculatorItem;
        items.Add(calculatorItem);
    }
}