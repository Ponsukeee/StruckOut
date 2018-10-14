using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabController : MonoBehaviour
{
    SteamVR_Controller.Device device;
    GameObject collidingGameObject;
    Rigidbody grabbingRigidbody;
    FixedJoint fixedJoint;
    LineRenderer lineRenderer;  //Laser用のLineRenderer
    Vector2 prevAxis = Vector2.zero;
    [HideInInspector] public bool inSphere = false;

    bool grabbing = false;
    public bool Grabbing
    {
        get { return this.grabbing; }
        private set { this.grabbing = value; }
    }

    public static float maxVelocityMagnitude = 50f; /*実最大速度*/
    public static float maxSpeed = 165f; //表記上最大速度
    [SerializeField] float multiplier /*スナップ倍率*/;
    [SerializeField] GameObject instantiateBall, headCamera;  //Basketから生成するボールのPrefab
    [SerializeField] Animator grabAnimator;  //掴むモーション
    [SerializeField, Range(0, 1)] float interpolationLimit;

    void Start()
    {
        var trackedObject = GetComponent<SteamVR_TrackedObject>();
        device = SteamVR_Controller.Input((int)trackedObject.index);
        fixedJoint = gameObject.AddComponent<FixedJoint>();
        lineRenderer = GetComponentInChildren<LineRenderer>();
    }

    void FixedUpdate()
    {
        grabAnimator.SetFloat("Grab", device.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger).x);

        if (collidingGameObject && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger) && !grabbing)
        {
            Grab();
        }

        if (grabbing && device.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger).x < 0.25)
        {
            if(inSphere) Release();
            else Throw();
        }

        //掴んだオブジェクトの回転（回転方向に難あり）
        if (grabbing && device.GetTouch(SteamVR_Controller.ButtonMask.Touchpad))
        {
            if (prevAxis != Vector2.zero)
            {
                var diffAxis_x = device.GetAxis().x - prevAxis.x;
                var diffAxis_y = device.GetAxis().y - prevAxis.y;
                fixedJoint.connectedBody = null;
                grabbingRigidbody.transform.Rotate(new Vector3(0, 1, 0), Mathf.Lerp(-120, 120, (diffAxis_x + 2) / 4));
                grabbingRigidbody.transform.Rotate(new Vector3(1, 0, 0), Mathf.Lerp(-120, 120, (diffAxis_y + 2) / 4));
                grabbingRigidbody.transform.position = transform.position;
                fixedJoint.connectedBody = grabbingRigidbody;
            }
            prevAxis = device.GetAxis();
        }
        else if (device.GetTouchUp(SteamVR_Controller.ButtonMask.Touchpad))
        {
            prevAxis = Vector2.zero;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Grabable"))
        {
            collidingGameObject = other.gameObject;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Grabable"))
        {
            collidingGameObject = null;
        }
    }

    void Grab()
    {
        //掴むときLineRendererを消す
        if (lineRenderer && lineRenderer.enabled)
        {
            lineRenderer.transform.GetComponent<LaserController>().toggleLineRenderer();
        }

        if (collidingGameObject.CompareTag("Grabable"))
        {
            grabbing = true;
            grabbingRigidbody = collidingGameObject.GetComponent<Rigidbody>();
            fixedJoint.connectedBody = grabbingRigidbody;
            device.TriggerHapticPulse(2500);
        }
    }

    void Throw()
    {
        grabbing = false;
        fixedJoint.connectedBody = null;

        grabbingRigidbody.velocity = device.velocity;
        grabbingRigidbody.angularVelocity = device.angularVelocity;
        Destroy(grabbingRigidbody.gameObject, 5.0f);

        if (Vector3.Dot(transform.forward, device.velocity.normalized) >= 0 && grabbingRigidbody.CompareTag("Grabable"))
        {
            var matchDir = Mathf.Max(0.8f, Vector3.Dot(transform.forward.normalized, device.velocity.normalized));  //デバイス方向一致倍率
            var snapSpeed = Mathf.Max(0.6f, device.angularVelocity.magnitude * multiplier);  //スナップ倍率
            grabbingRigidbody.velocity = Vector3.ClampMagnitude(device.velocity * matchDir * snapSpeed, maxVelocityMagnitude);  //速度制限
            var velocityMagnitude = grabbingRigidbody.velocity.magnitude;

            //視線による軌道補正
            RaycastHit hitInfo;
            var hit = Physics.Raycast(headCamera.transform.position, headCamera.transform.forward, out hitInfo, 30f);
            var toTargetDir = hitInfo.point - transform.position;
            var interpolationRatio = velocityMagnitude / maxVelocityMagnitude;
            if (hit && hitInfo.collider.CompareTag("Panel"))
                grabbingRigidbody.velocity = velocityMagnitude * Vector3.Lerp(grabbingRigidbody.velocity.normalized, toTargetDir.normalized, interpolationRatio * interpolationLimit);

            //軌道表示
            var trail = grabbingRigidbody.GetComponent<TrailRenderer>();
            var particleNoise = grabbingRigidbody.GetComponentInChildren<ParticleSystem>().noise;
            trail.enabled = true;
            trail.startWidth = Mathf.Lerp(0.01f, 0.1f, interpolationRatio);  //ボールの速度でtrailの幅を調整
            particleNoise.strength = Mathf.Lerp(0f, 0.6f, interpolationRatio);

            //球速表示
            var speedParameter = (int)Mathf.Lerp(0, maxSpeed, interpolationRatio);
            ScoreDisplay.speed = speedParameter;

            //球速の平均の為のデータ
            ScoreDisplay.speedList.Add(speedParameter);

            //投球数増加
            ScoreDisplay.ball++;
        }
    }

    void Release()
    {
        grabbing = false;
        fixedJoint.connectedBody = null;
    }
}
