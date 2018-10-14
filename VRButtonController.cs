using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Button側にアタッチ
public class VRButtonController : MonoBehaviour
{
    SteamVR_Controller.Device device;
    GameObject collidingFinger;
    Vector3 initialButtonPosition;
    Color defaultColor;
    AudioSource clickSE;
    ToggleWeatherSphere buttonFunction;
    bool colliding = false;
    bool pressed = false;
    [SerializeField] float step, clickJudgeDistance;
    [SerializeField] Color clickingColor;

    virtual protected void Awake()
    {
        buttonFunction = GetComponent<ToggleWeatherSphere>();
        clickSE = GetComponent<AudioSource>();
        defaultColor = GetComponent<Image>().color;

        initialButtonPosition = transform.localPosition;
    }

    virtual protected void FixedUpdate()
    {
        //collisionが発生してからボタンが元の位置に戻るまで
        if (collidingFinger)
        {
            //ボタンが押せる状態の時&ボタンとプレートのローカル座標が一致するときに、ボタンが押された判定にする
            if (!pressed && clickJudgeDistance < (transform.localPosition - initialButtonPosition).magnitude)
            {
                ButtonClick();
            }
            //ボタンが押せない状態&ボタンが1/4以上元の位置に戻った時に、ボタンを再び押せる状態にする
            else if (pressed && clickJudgeDistance * 0.75 > (transform.localPosition - initialButtonPosition).magnitude)
            {
                StillNotPress();
            }
        }

        if (colliding)
        {
            var fromButtonToFinger = transform.parent.InverseTransformPoint(collidingFinger.transform.position) - initialButtonPosition;

            //ボタンを押し込む方向の時のみ移動させる
            if (Vector3.Dot(Vector3.forward, fromButtonToFinger) > 0)
            {
                transform.localPosition = Vector3.Project(fromButtonToFinger, Vector3.forward);
                //transform.localPosition = Vector3.Project(fromButtonToFinger, Quaternion.Euler(transform.rotation.x, transform.rotation.y, transform.rotation.z) * Vector3.forward);
                //transform.localPosition = Vector3.Scale(fromButtonToFinger, Vector3.forward);
            }
        }
        //collisionが発生しておらず、ボタンが元の位置に戻るまで
        else
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, initialButtonPosition, step);

            //collisionが発生してからボタンが元の位置に戻った時に初期化する
             if (transform.localPosition == initialButtonPosition)
             {
                collidingFinger = null;
             }
        }
    }

    //ボタン押下時の動作
    void ButtonClick()
    {
        pressed = true;
        GetComponent<Image>().color = clickingColor;

        var trackedObject = collidingFinger.transform.parent.GetComponent<SteamVR_TrackedObject>();
        device = SteamVR_Controller.Input((int)trackedObject.index);
        device.TriggerHapticPulse(2500);

        ButtonFunction();
        clickSE.PlayOneShot(clickSE.clip);
    }

    virtual protected void ButtonFunction()
    {
        //サブクラス内でoverrideして、ボタン押下時の機能追加
    }

    //再びボタンを押せる状態にする
    void StillNotPress()
    {
        GetComponent<Image>().color = defaultColor;
        pressed = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("VRButton"))
        {
            colliding = true;
            //前回とcollisionしたObjectが同じ場合、初期値を変更しない
            if (collidingFinger != other.gameObject)
            {
                collidingFinger = other.gameObject;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        colliding = false;
    }
}