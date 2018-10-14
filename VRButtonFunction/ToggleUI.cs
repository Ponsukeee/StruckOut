using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleUI : VRButtonController
{
    [SerializeField] GameObject parentObject, headCamera;
    [SerializeField] GameObject[] buttons;
    [SerializeField] Vector3[] targetLocalPosition, startLocalPosition;
    [SerializeField] Vector3 offset;
    [SerializeField] float chaseTime, moveTime;
    public static GameObject currentActivePanel;
    Collider[] colliders;
    Vector3[] initialScales;
    Vector3 velocity;
    bool isActive = false;

    override protected void Awake()
    {
        base.Awake();
        
        initialScales = new Vector3[buttons.Length];
        colliders = new Collider[buttons.Length];
        for (int i = 0; i < buttons.Length; i++)
        {
            initialScales[i] = buttons[i].transform.localScale;
            colliders[i] = buttons[i].GetComponentInChildren<Collider>();
            buttons[i].transform.localPosition = startLocalPosition[i];
            buttons[i].transform.localScale = Vector3.zero;
            colliders[i].enabled = false;
        }
    }

    override protected void FixedUpdate()
    {
        base.FixedUpdate();

        parentObject.transform.position = Vector3.SmoothDamp(parentObject.transform.position, transform.parent.position + offset, ref velocity, chaseTime);
        parentObject.transform.LookAt(headCamera.transform.position);
    }

    override protected void ButtonFunction()
    {
        if (!isActive)
        {
            isActive = true;
            for (int i = 0; i < buttons.Length; i++)
            {
                colliders[i].enabled = true;
                iTween.MoveTo(buttons[i].gameObject, iTween.Hash("position", targetLocalPosition[i], "time", moveTime, "isLocal", true));
                iTween.ScaleTo(buttons[i].gameObject, iTween.Hash("scale", initialScales[i], "time", moveTime * 0.5f));
            }
        }
        else
        {
            isActive = false;

            for (int i = 0; i < buttons.Length; i++)
            {
                iTween.MoveTo(buttons[i].gameObject, iTween.Hash("position", startLocalPosition[i], "time", moveTime, "isLocal", true));
                iTween.ScaleTo(buttons[i].gameObject, iTween.Hash("scale", Vector3.zero, "time", moveTime * 0.5f));
                colliders[i].enabled = false;
            }
        }
    }
}