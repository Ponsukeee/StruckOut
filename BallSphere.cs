using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//BallSphereにアタッチ
public class BallSphere : MonoBehaviour
{
    [SerializeField] GameObject ball, headCamera;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] ToggleWeatherSphere weather;
    [SerializeField] Vector3 defaultOffset, grabbingOffset, preventGrabOffset;
    [SerializeField] float intervalTime;
    GameObject instantiatedBall;
    GrabController grabController;
    Animation scaleUp;
    Vector3 velocity;

    void Awake()
    {
        InstantiateAndInitialize();
    }

    void FixedUpdate()
    {
        if(grabController && grabController.Grabbing)
        {
            transform.position = Vector3.SmoothDamp(transform.position, headCamera.transform.position + grabbingOffset, ref velocity, 0.5f);
        }
        else if(lineRenderer.enabled || weather.isVisible)
        {
            transform.position = Vector3.SmoothDamp(transform.position, headCamera.transform.position + preventGrabOffset, ref velocity, 0.5f);
        }
        else
        {
            transform.position = Vector3.SmoothDamp(transform.position, headCamera.transform.position + defaultOffset, ref velocity, 0.5f); 
        }
        
        if (instantiatedBall)
        {
            instantiatedBall.transform.position = Vector3.MoveTowards(instantiatedBall.transform.position, transform.position, 0.01f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Grabber"))
        {
            var grabController = other.GetComponent<GrabController>();
            grabController.inSphere = true;

            var trackedObject = other.GetComponent<SteamVR_TrackedObject>();
            var device = SteamVR_Controller.Input((int)trackedObject.index);
            device.TriggerHapticPulse(1500);
        }
    }

    void OnTriggerExit(Collider other)
    {
        //BallSphereから手が出ていくとき
        if(other.CompareTag("Grabber"))
        {
            var grabController = other.GetComponent<GrabController>();
            grabController.inSphere = false;

            var trackedObject = other.GetComponent<SteamVR_TrackedObject>();
            var device = SteamVR_Controller.Input((int)trackedObject.index);
            device.TriggerHapticPulse(1500);
            
            //Ballを取り出すとき
            if(grabController.Grabbing) 
            {
                this.grabController = grabController;
                instantiatedBall.GetComponent<Rigidbody>().useGravity = true;
                instantiatedBall.GetComponent<Collider>().isTrigger = false;
                instantiatedBall = null;
                StartCoroutine(DelayMethod(intervalTime, () =>
                {
                    InstantiateAndInitialize();
                    scaleUp.Play();
                }));
            }
        }
    }

    void InstantiateAndInitialize()
    {
        instantiatedBall = Instantiate(ball);
        instantiatedBall.transform.position = transform.position;
        instantiatedBall.GetComponent<Rigidbody>().useGravity = false;
        instantiatedBall.GetComponent<Collider>().isTrigger = true;
        scaleUp = instantiatedBall.GetComponent<Animation>();
    }

    IEnumerator DelayMethod(float waitTime, Action action)
    {
        yield return new WaitForSeconds(waitTime);
        action();
    }
}