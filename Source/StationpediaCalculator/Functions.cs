#region

using Cysharp.Threading.Tasks;
using NCalc;
using UnityObject = UnityEngine.Object;

#endregion

namespace StationpediaCalculator;

internal static class Functions {
    internal static void CalculateSearch(string searchText) {
        Expression expression = new(searchText, EvaluateOptions.IgnoreCase);

        if (expression.HasErrors()) {
            Data.CalculatorItem.gameObject.SetActive(false);
        }
        else {
            string result = "";
            try {
                result = expression.Evaluate().ToString();
            }
            catch (EvaluationException) { } // only catch EvaluationExceptions

            Data.CalculatorItem.gameObject.SetActive(true);
            Data.CalculatorItem.transform.SetSiblingIndex(0);

            Data.CalculatorItem.InsertTitle.text = result;
            Data.CalculatorItem.InsertImage.sprite = Stationpedia.Instance.ImportantSearchImage;
            Data.CalculatorItem.SetSpecial();

            Data.CalculatorItem.InsertsButton.onClick.AddListener(async () => {
                Stationpedia.Instance.BaseAnimator.SetBool("Copied", true);
                GameManager.Clipboard = result;
                await UniTask.Delay(750);
                Stationpedia.Instance.ResetClipboardNotification();
            });

            Stationpedia.Instance.NoResultsFromSearchText.SetActive(false);
        }
    }

    internal static void CreateCalculator(ref List<SPDAListItem> items) {
        SPDAListItem calculatorItem = UnityObject.Instantiate(Stationpedia.Instance.ListInsertPrefab, Stationpedia.Instance.SearchContents);
        calculatorItem.gameObject.SetActive(false);

        Data.CalculatorItem = calculatorItem;
        items.Add(calculatorItem);
    }
}