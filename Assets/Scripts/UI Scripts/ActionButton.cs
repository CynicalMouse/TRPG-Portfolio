using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ActionButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI actionNameText;
    [SerializeField] private Button button;
    [SerializeField] private Image selectedSprite;
    private BaseAction buttonAction;

    public void SetBaseAction(BaseAction baseAction)
    {
        buttonAction = baseAction;
        actionNameText.text = baseAction.GetActionName().ToUpper();
        button.onClick.AddListener(() =>
        {
            UnitActionSystem.Instance.SetSelectedAction(baseAction);
        });
    }

    public void UpdateSelectedVisual(bool enabled)
    {
        selectedSprite.enabled = enabled;
    }

    public BaseAction GetBaseAction()
    {
        return buttonAction;
    }
}
