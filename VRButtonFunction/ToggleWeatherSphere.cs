using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleWeatherSphere : VRButtonController
{
    BouncyController[] childrenBouncy;
    StageController[] childrenStageController;
    Animation[] childrenAnimation;
    [HideInInspector] public bool isVisible;
    [SerializeField] GameObject sphereGroup, cameraHead;
    [SerializeField] float offset_y, offset_z;

    override protected void Awake()
    {
        base.Awake();

        childrenBouncy = sphereGroup.GetComponentsInChildren<BouncyController>();
        childrenStageController = sphereGroup.GetComponentsInChildren<StageController>();
        childrenAnimation = sphereGroup.GetComponentsInChildren<Animation>();
    }

    //override protected void FixedUpdate()
    //{
    //    base.FixedUpdate();
    //}

    override protected void ButtonFunction()
    {
        if (!isVisible)
        {
            isVisible = true;
            sphereGroup.transform.position = cameraHead.transform.position + Vector3.Scale(cameraHead.transform.forward, new Vector3(1, 0, 1)).normalized * offset_z + new Vector3(0, offset_y, 0);
            sphereGroup.transform.LookAt(cameraHead.transform.position + new Vector3(0, offset_y, 0));
            sphereGroup.SetActive(true);
            for (int i = 0; i < childrenBouncy.Length; i++)
            {
                childrenBouncy[i].enabled = true;
                childrenBouncy[i].Start();
            }
            for (int i = 0; i < childrenAnimation.Length; i++)
            {
                if (!childrenStageController[i].isReactiving)
                {
                    childrenAnimation[i].Play("ScaleUp");
                }
            }
        }
        else
        {
            isVisible = false;
            for (int i = 0; i < childrenAnimation.Length; i++)
            {
                childrenBouncy[i].enabled = false;
                if (childrenAnimation[i].transform.localScale != Vector3.zero)
                {
                    childrenAnimation[i].Play("ScaleDown");
                }
            }
            StartCoroutine("TogglerSphere");
        }
    }

    IEnumerator TogglerSphere()
    {
        yield return new WaitWhile(() => StageController.changing || childrenAnimation[0].IsPlaying("ScaleDown"));
        if (!isVisible)
            sphereGroup.SetActive(false);
    }

}
