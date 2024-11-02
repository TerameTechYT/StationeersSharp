#region

using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.UI;
using Cysharp.Threading.Tasks;
using NCalc;
using UnityObject = UnityEngine.Object;

#endregion

namespace StationpediaCalculator;

public static class Functions {
    public static void CalculateSearch(ref SPDAListItem calculatorItem, string inputText) {
        try {
            if (Stationpedia.Instance != null && calculatorItem != null && !string.IsNullOrEmpty(inputText)) {
                var expression = new Expression(inputText, EvaluateOptions.IgnoreCase);

                if (expression.HasErrors()) {
                    calculatorItem.gameObject.SetActive(false);
                }
                else {
                    var result = expression.Evaluate().ToString();

                    calculatorItem.gameObject.SetActive(true);
                    calculatorItem.transform.SetSiblingIndex(0);

                    calculatorItem.InsertTitle.text = result;
                    calculatorItem.InsertImage.sprite = Stationpedia.Instance.ImportantSearchImage;
                    calculatorItem.SetSpecial();

                    calculatorItem.InsertsButton.onClick.AddListener(async () => {
                        Stationpedia.Instance.BaseAnimator.SetBool("Copied", true);
                        GameManager.Clipboard = result;
                        await UniTask.Delay(750);
                        Stationpedia.Instance.ResetClipboardNotification();
                    });

                    Stationpedia.Instance.NoResultsFromSearchText.SetActive(false);
                }
            }
        }
        catch { }
    }

    public static void CreateCalculator(ref List<SPDAListItem> items) {
        if (Stationpedia.Instance != null && items != null) {
            var calculatorItem = UnityObject.Instantiate(Stationpedia.Instance.ListInsertPrefab,
                Stationpedia.Instance.SearchContents);
            calculatorItem.gameObject.SetActive(false);

            Data.CalculatorItem = calculatorItem;
            items.Add(calculatorItem);
        }
    }
}