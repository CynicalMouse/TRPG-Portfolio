using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GridDebugObject : MonoBehaviour
{
    private object gridObject;

    [SerializeField] private TextMeshPro text;

    private void Awake()
    {
        text = GetComponentInChildren<TextMeshPro>();
    }

    protected virtual void Update()
    {
        DisplayGridPosition();
    }

    public virtual void SetGridObject(object gridObject)
    {
        this.gridObject = gridObject;
    }

    public void DisplayGridPosition()
    {
        text.text = gridObject.ToString();
    }
}
