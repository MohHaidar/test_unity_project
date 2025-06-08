
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MultipleChoiceManager : BaseActivityManager
{
    public Button[] optionButtons;

    protected override void RenderQuestionUI(Question q)
    {
        promptText.text = q.Prompt;
        moduleNameText.text = q.Type;
        
        for (int i = 0; i < optionButtons.Length; i++)
        {
            int idx = i;
            optionButtons[i].gameObject.SetActive(true);
            optionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = q.Options[i];
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() =>
            {
                currentActivity.SubmitAnswer(q.Options[idx]);


            });
        }
    }
}
