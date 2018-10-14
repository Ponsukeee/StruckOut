using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LaserController : MonoBehaviour
{
    SteamVR_Controller.Device device;
    LineRenderer lineRenderer;
    RaycastHit hitInfo, prevHitInfo;
    Vector3 originalScale, originalScaleScore;
    FixedJoint fixedJoint;
    Quaternion prevRotation;
    float prevAxis_x;
    bool isJointed;
    [SerializeField] float lineLength;
    [SerializeField] GameObject scoreBoard;  //遠近でスケールを操作するのに必要
    [SerializeField] Canvas reticle;  //reticle画像を子として持つCanvas
    [SerializeField] Material panelOriginalMat, panelSelectingMat;  //パネルの選択前、後のMaterial
    [SerializeField] Color originalLineColor, canSelectLineColor, jointedColor;  //LineRendererの通常状態、選択可能状態のColor

    void Start()
    {
        var trackedObject = transform.parent.GetComponent<SteamVR_TrackedObject>();
        device = SteamVR_Controller.Input((int)trackedObject.index);

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
        reticle.enabled = false;
        originalScale = reticle.transform.localScale;
        lineRenderer.material.color = originalLineColor;

        fixedJoint = transform.parent.gameObject.AddComponent<FixedJoint>();
        originalScaleScore = scoreBoard.transform.localScale;
    }

    void Update()
    {
        if (device.GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu))
        {
            toggleLineRenderer();
        }

        if (lineRenderer.enabled)
        {
            bool isHitting = Physics.Raycast(transform.position, transform.forward, out hitInfo, 10.0f);
            reticleDisplay(isHitting);
            
            if (isHitting)
            {
                lineRenderer.SetPosition(1, new Vector3(0, 0, Mathf.Clamp(hitInfo.distance, 0.0f, lineLength)));  //突き抜け防止

                if (hitInfo.collider.CompareTag("Panel"))
                {
                    lineRenderer.material.color = canSelectLineColor;
                    reticle.GetComponentInChildren<Image>().color = canSelectLineColor;

                    if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
                    {
                        PanelSelect();
                    }
                }
                else if (hitInfo.collider.CompareTag("ScoreBoard"))
                {
                    lineRenderer.material.color = canSelectLineColor;
                    reticle.GetComponentInChildren<Image>().color = canSelectLineColor;

                    ControlScoreBoard();
                }
                else
                {
                    lineRenderer.material.color = originalLineColor;
                    reticle.GetComponentInChildren<Image>().color = originalLineColor;
                }
            }
            else
            {
                lineRenderer.material.color = originalLineColor;
                reticle.GetComponentInChildren<Image>().color = originalLineColor;

                fixedJoint.connectedBody = null;
                isJointed = false;
                lineRenderer.SetPosition(1, new Vector3(0, 0, lineLength));  //突き抜け防止
            }
        }
    }

    public void toggleLineRenderer()
    {
        lineRenderer.enabled = !lineRenderer.enabled;
        if (reticle.enabled)
        {
            reticle.enabled = false;
        }

        if (transform.parent.GetComponent<FixedJoint>().connectedBody)
        {
            isJointed = false;
            fixedJoint.connectedBody = null;
        }
    }

    void PanelSelect()
    {
        device.TriggerHapticPulse(2500);

        if (prevHitInfo.collider)
        {
            //前回選択したパネルと今回選択するパネルが異なるとき
            if (prevHitInfo.collider != hitInfo.collider)
            {
                prevHitInfo.collider.GetComponent<Renderer>().material = panelOriginalMat;
                prevHitInfo.collider.GetComponent<PanelController>().isSelected = false;
                hitInfo.collider.GetComponent<Renderer>().material = panelSelectingMat;
                hitInfo.collider.GetComponent<PanelController>().isSelected = true;
            }
            //同じパネルを選択するとき
            else
            {
                //パネルが既に選ばれているとき
                if (hitInfo.collider.GetComponent<PanelController>().isSelected)
                {
                    hitInfo.collider.GetComponent<Renderer>().material = panelOriginalMat;
                    hitInfo.collider.GetComponent<PanelController>().isSelected = false;
                }
                //パネルが選択されていないとき
                else
                {
                    hitInfo.collider.GetComponent<Renderer>().material = panelSelectingMat;
                    hitInfo.collider.GetComponent<PanelController>().isSelected = true;
                }
            }
        }
        else
        {
            hitInfo.collider.GetComponent<Renderer>().material = panelSelectingMat;
            hitInfo.collider.GetComponent<PanelController>().isSelected = true;
        }
        prevHitInfo = hitInfo;
    }

    void ControlScoreBoard()
    {
        if (!isJointed && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            isJointed = true;
            fixedJoint.connectedBody = hitInfo.rigidbody;
            device.TriggerHapticPulse(2500);
        }
        else if (isJointed)
        {
            lineRenderer.material.color = jointedColor;
            reticle.GetComponentInChildren<Image>().color = jointedColor;
            if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
            {
                isJointed = false;
                fixedJoint.connectedBody = null;
                device.TriggerHapticPulse(2500);
            }
            else if (device.GetPress(SteamVR_Controller.ButtonMask.Touchpad))  //ScoreBoardの遠近
            {
                fixedJoint.connectedBody = null;
                var diffVector = hitInfo.point - hitInfo.transform.position;
                hitInfo.transform.position = transform.position - diffVector + transform.forward * Mathf.Clamp(hitInfo.distance + device.GetAxis().y * 0.05f, 0.5f, lineLength * 0.8f);

                hitInfo.transform.localScale = originalScaleScore * hitInfo.distance * 0.6f;
                fixedJoint.connectedBody = hitInfo.rigidbody;
            }
            else if (device.GetTouch(SteamVR_Controller.ButtonMask.Touchpad))  //ScoreBoardの回転
            {
                if(prevAxis_x != 0)
                {
                    fixedJoint.connectedBody = null;
                    hitInfo.transform.rotation = hitInfo.transform.rotation * Quaternion.AngleAxis((device.GetAxis().x - prevAxis_x) * 20f, Vector3.up);
                    if (Vector3.Angle(-transform.forward, hitInfo.normal) < 50)
                        prevRotation = hitInfo.transform.rotation;
                    else if(Vector3.Angle(-transform.forward, hitInfo.normal) > 65) //回転制限
                        hitInfo.transform.rotation = prevRotation;
                    fixedJoint.connectedBody = hitInfo.rigidbody;
                }
                prevAxis_x = device.GetAxis().x;
            }
            else if (device.GetTouchUp(SteamVR_Controller.ButtonMask.Touchpad))
            {
                prevAxis_x = 0f;
            }
        }
    }

    void reticleDisplay(bool isHitting)
    {
        if (isHitting)
        {
            reticle.enabled = true;
            reticle.transform.position = hitInfo.point;
            reticle.transform.localScale = originalScale * hitInfo.distance;
            reticle.transform.rotation = Quaternion.FromToRotation(Vector3.forward, hitInfo.normal);
        }
        else
        {
            reticle.enabled = false;
        }
    }
}
