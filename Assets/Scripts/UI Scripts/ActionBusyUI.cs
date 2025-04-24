using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionBusyUI : MonoBehaviour
{
    [SerializeField] private GameObject image;
    [SerializeField] private GameObject text;

    // Start is called before the first frame update
    void Start()
    {
        UnitActionSystem.Instance.OnBusyChange += UnitActionSystem_OnBusyChange;
    }

    private void UnitActionSystem_OnBusyChange(object sender, bool active)
    {
        image.gameObject.SetActive(active);
        text.gameObject.SetActive(active);
    }
}
