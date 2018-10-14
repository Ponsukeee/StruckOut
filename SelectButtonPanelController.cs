using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectButtonPanelController : MonoBehaviour
{
    [SerializeField] Vector3 startPosition, targetPosition;
    [SerializeField] GameObject headCamera;
    [SerializeField] float moveTime, scaleTime;
    public static GameObject currentPanel;
    Collider[] colliders;
    GameObject thisPanel;
    Vector3 defaultScale;
    bool isActive;

    void Awake()
    {
        thisPanel = this.gameObject;
        colliders = GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        defaultScale = transform.localScale;
        transform.localPosition = startPosition;
        transform.localScale = Vector3.zero;
    }

    void FixedUpdate()
    {
        transform.LookAt(headCamera.transform.position);
    }

    public void Activate()
    {
        isActive = true;
        currentPanel = thisPanel;
        iTween.MoveTo(thisPanel, iTween.Hash("position", targetPosition, "time", moveTime, "isLocal", true));
        iTween.ScaleTo(thisPanel, iTween.Hash("scale", defaultScale, "time", scaleTime));
        //誤操作防止
        StartCoroutine(DelayMethod(0.3f, () =>
        {
            if (isActive)
            {
                for (int i = 0; i < colliders.Length; i++)
                {
                    colliders[i].enabled = true;
                }
            }
        }));
    }

    public void Deactivate()
    {
        isActive = false;
        if (this.gameObject == currentPanel)
        {
            currentPanel = null;
        }
        iTween.MoveTo(thisPanel, iTween.Hash("position", startPosition, "time", moveTime, "isLocal", true));
        iTween.ScaleTo(thisPanel, iTween.Hash("scale", Vector3.zero, "time", moveTime * 0.5f));
        //誤操作防止
        StartCoroutine(DelayMethod(0.1f, () =>
        {
            if (!isActive)
            {
                for (int i = 0; i < colliders.Length; i++)
                {
                    colliders[i].enabled = false;
                }
            }
        }));
    }

    IEnumerator DelayMethod(float waitTime, Action action)
    {
        yield return new WaitForSeconds(waitTime);
        action();
    }
}