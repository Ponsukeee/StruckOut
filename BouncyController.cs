using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncyController : MonoBehaviour
{
    [SerializeField] float moveValue, grabDistance, pullDistance, maxThrowingScale, periodValue, maxPeriodTime, snapValue, maxVelocityMagnitude, explodeThreshold;
    Vector3 initialPosition, initialLocalPosition, initialScale, initialGrabPosition, prevGrabberPosition;
    GameObject grabber;
    Rigidbody rb;
    GrabController grabController;
    SteamVR_Controller.Device device;
    float period = 0f;
    bool preGrab = false;
    bool grabbing = false;

    void Awake()
    {
        initialScale = transform.localScale;
        initialLocalPosition = transform.localPosition;
        rb = GetComponent<Rigidbody>();
    }

    public void Start()
    {
        transform.localPosition = initialLocalPosition;
        initialPosition = transform.position;
    }

    void FixedUpdate()
    {
        if (!preGrab && !grabbing)
        {
            RegulationController();
        }

        if (preGrab)
        {
            if (device.GetTouchUp(SteamVR_Controller.ButtonMask.Trigger))
            {
                preGrab = false;
                grabbing = false;
            }
            PreGrabController();
        }

        if (grabbing)
        {
            if (device.GetTouchUp(SteamVR_Controller.ButtonMask.Trigger))
            {
                preGrab = false;
                grabbing = false;
            }
            GrabbingController();
        }
    }

    void RegulationController()
    {
        var toInitialVector = initialPosition - transform.position;
        if (grabber && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger) && toInitialVector.magnitude < grabDistance)  //ビヨンビヨン状態へ移行
        {
            preGrab = true;
            initialGrabPosition = grabber.transform.position;
            prevGrabberPosition = grabber.transform.position;
        }
        else if (grabber && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            Grab();
            rb.velocity = Vector3.zero;
        }
        else if (toInitialVector.magnitude < 0.05f/*transform.position == initialPosition*/)  //元の位置に戻った時
        {
            rb.velocity = Vector3.zero;
            transform.position = Vector3.MoveTowards(transform.position, initialPosition, moveValue);
            transform.localScale = Vector3.MoveTowards(transform.localScale, initialScale, moveValue);
        }
        else  //元の位置ではないとき
        {
            if (period > 0f)
            {
                var acceleration = (toInitialVector - rb.velocity * period) * 2f / (period * period);
                period -= Time.deltaTime;
                rb.velocity += acceleration * Time.deltaTime;
            }
            transform.LookAt(initialPosition);
            transform.localScale = initialScale + transform.InverseTransformDirection(transform.forward) * Mathf.Min(rb.velocity.magnitude * 0.03f, maxThrowingScale);
        }
    }

    void PreGrabController()
    {
        var toGrabberVector = grabber.transform.position - initialPosition;
        var diffGrabberPosition = grabber.transform.position - prevGrabberPosition;
        if (toGrabberVector.magnitude > grabDistance)
        {
            Grab();
            rb.velocity = Vector3.zero;
        }
        else if (toGrabberVector.magnitude > pullDistance)   //preGrab状態での掴む直前の、元に戻ろうとする処理
        {
            transform.position = Vector3.Lerp(initialPosition + toGrabberVector * 0.5f, initialPosition + toGrabberVector * 0.75f, (toGrabberVector.magnitude - pullDistance) / (grabDistance - pullDistance));
            transform.LookAt(grabber.transform.position);
            transform.localScale = initialScale + Vector3.forward * (grabber.transform.position - transform.position).magnitude;
        }
        else   //preGrab状態での位置と大きさの変化
        {
            transform.position = initialPosition + toGrabberVector / 2;
            transform.LookAt(grabber.transform.position);
            transform.localScale = initialScale + Vector3.forward * (grabber.transform.position - transform.position).magnitude;
        }
        prevGrabberPosition = grabber.transform.position;
    }

    void Grab()
    {
        preGrab = false;
        grabbing = true;
        prevGrabberPosition = grabber.transform.position;
        device.TriggerHapticPulse(2500);
    }

    void GrabbingController()
    {
        transform.position = Vector3.MoveTowards(transform.position, grabber.transform.position, moveValue * 0.4f);
        transform.localScale = Vector3.MoveTowards(transform.localScale, initialScale, moveValue * 0.4f);
        transform.position += grabber.transform.position - prevGrabberPosition;
        prevGrabberPosition = grabber.transform.position;

        if (device.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger).x < 0.25)
        {
            Throw();
        }
    }

    void Throw()
    {
        grabbing = false;

        rb.velocity = device.velocity;
        rb.angularVelocity = device.angularVelocity;
        period = Mathf.Min(rb.velocity.magnitude * periodValue + (initialPosition - transform.position).magnitude * periodValue, maxPeriodTime);
        if (Vector3.Dot(grabber.transform.forward, device.velocity) >= 0)
        {
            var matchDir = Mathf.Max(0.8f, Vector3.Dot(transform.forward, device.velocity.normalized));  //方向一致倍率
            var snapSpeed = Mathf.Max(0.6f, device.angularVelocity.magnitude * snapValue);  //スナップ倍率
            rb.velocity = Vector3.ClampMagnitude(device.velocity * matchDir * snapSpeed, maxVelocityMagnitude);  //速度制限
        }

        if(rb.velocity.y > explodeThreshold)
        {
            GetComponent<StageController>().Explosion();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Grabber") && grabber != other.gameObject)
        {
            grabber = other.gameObject;
            grabController = grabber.GetComponent<GrabController>();
            var trackedObject = grabber.GetComponent<SteamVR_TrackedObject>();
            device = SteamVR_Controller.Input((int)trackedObject.index);
            device.TriggerHapticPulse(2500);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == grabber)
        {
            if (period <= 0f)  //Throwせずにcollisionがなくなったとき
            {
                period = Mathf.Min(rb.velocity.magnitude * periodValue + (initialPosition - transform.position).magnitude * periodValue, maxPeriodTime);
            }
            grabber = null;
            grabController = null;
            device = null;
            preGrab = false;
            grabbing = false;
        }
    }
}

