using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PanelController : MonoBehaviour
{
    GameObject gotPointDisplay;
    TextMeshProUGUI pointText;
    bool firstContact = true;
    float time = 0;
    float maxDistance;
    [HideInInspector] public bool isSelected = false;
    [SerializeField] GameObject eye, pointDisplayPrefab;
    [SerializeField] AudioSource collisionSE, highPointSE, middlePointSE, lowPointSE;
    [SerializeField] Color highPointColor, middlePointColor, lowPointColor;
    [SerializeField] GrabController grabController;
    [SerializeField] float multiplier;  //吹き飛ばし倍率
    [SerializeField] float moveValue;   //得点Animationの動く幅

    void Awake()
    {
        var collider = GetComponent<BoxCollider>();
        //Panelの中心から角までの距離
        maxDistance = Mathf.Sqrt(Mathf.Pow(collider.size.z * transform.localScale.z * transform.parent.localScale.z, 2) + Mathf.Pow(collider.size.y * transform.localScale.y * transform.parent.localScale.y, 2)) / 2;
    }

    void FixedUpdate()
    {
        if (gotPointDisplay)
        {
            //GotPointAnimation();
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Grabable") && firstContact)
        {
            firstContact = false;
            // float maxDistance = Mathf.Sqrt(Mathf.Pow(GetComponent<BoxCollider>().size.y * transform.localScale.y, 2) + Mathf.Pow(GetComponent<BoxCollider>().size.z * transform.localScale.z, 2)) / 2;
            // float distance = Mathf.Lerp(0, maxDistance, Vector3.Distance(other.contacts[0].point, transform.position) / 0.36f);
            var distance = Vector3.Distance(other.contacts[0].point, transform.position);
            var distancePoint = Mathf.Lerp(0.0f, 50.0f, (1.0f - distance / maxDistance));
            int gotPoint = ScoreDisplay.pointDisplay(distancePoint, isSelected);

            GotPointDisplay(gotPoint);
            gotPointDisplay.GetComponent<Animation>().Play();
            ScoreDisplay.panel--;

            var ballRigidbody = other.gameObject.GetComponent<Rigidbody>();
            collisionSE.volume = ballRigidbody.velocity.magnitude / GrabController.maxVelocityMagnitude;
            collisionSE.PlayOneShot(collisionSE.clip);
            HitPanel(ballRigidbody);
        }
    }

    void HitPanel(Rigidbody ballRigidbody)
    {
        var panelRigidbody = GetComponent<Rigidbody>();
        panelRigidbody.isKinematic = false;
        panelRigidbody.useGravity = true;
        panelRigidbody.AddForce(ballRigidbody.velocity * multiplier);

        Destroy(ballRigidbody.gameObject, 2.5f);
        Destroy(gameObject, 2.5f);
    }

    void GotPointDisplay(int gotPoint)
    {
        gotPointDisplay = Instantiate(pointDisplayPrefab);
        pointText = gotPointDisplay.GetComponent<TextMeshProUGUI>();
        gotPointDisplay.transform.position = transform.position;
        //gotPointDisplay.transform.LookAt(eye.transform.position, Vector3.up);

        pointText.text = "" + gotPoint;
        if (gotPoint >= 80)
        {
            pointText.color = highPointColor;
            highPointSE.PlayOneShot(highPointSE.clip);
            gotPointDisplay.GetComponentInChildren<ParticleSystem>().Play();
        }
        else if (gotPoint >= 50)
        {
            pointText.color = middlePointColor;
            middlePointSE.PlayOneShot(middlePointSE.clip);
        }
        else
        {
            pointText.color = lowPointColor;
            lowPointSE.PlayOneShot(lowPointSE.clip);
        }
    }

    void GotPointAnimation()
    {
        time += Time.deltaTime;
        if (time < 0.1)
            gotPointDisplay.transform.position += Vector3.up * moveValue * Time.deltaTime;
        else if (time < 0.2)
            gotPointDisplay.transform.position -= Vector3.up * moveValue * Time.deltaTime;
        else if (0.6 < time && time < 1.0)
            pointText.color -= new Color(0, 0, 0, Time.deltaTime * 2.5f);
        else if (time >= 1.0)
            DestroyImmediate(gotPointDisplay);
    }
}
