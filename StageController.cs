using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageController : MonoBehaviour
{
    public static bool changing = false;
    static float time;
    static Color defaultLightColor, defaultHorizonColor, afterHorizonColor;
    static float defaultLightIntensity;
    static GameObject prevGameObject;
    Animation sphereAnimation;
    AudioSource[] audioSource;
    Material skyboxMaterial, setAfterMaterial;
    ParticleSystem particle;
    Light sceneLight;
    Rigidbody rb;
    BouncyController bouncyController;
    bool thisChanging = false;

    Quaternion rotation;
    [HideInInspector] public bool isReactiving = false;
    [SerializeField] ToggleWeatherSphere buttonFunction;
    [SerializeField] Color afterLightColor;
    [SerializeField] float afterIntensity, changingTime;

    void Awake()
    {
        sphereAnimation = GetComponent<Animation>();
        rb = GetComponent<Rigidbody>();
        particle = GetComponentInChildren<ParticleSystem>();
        setAfterMaterial = GetComponent<Renderer>().material;
        sceneLight = GameObject.Find("Directional Light").GetComponent<Light>();
        bouncyController = GetComponent<BouncyController>();
        skyboxMaterial = RenderSettings.skybox;
        time = changingTime;
        audioSource = GetComponents<AudioSource>();

        rotation = transform.rotation;
    }

    void Update()
    {
        if (thisChanging)
        {
            time -= Time.deltaTime;
            skyboxMaterial.SetFloat("_ChangingValue", time / changingTime);
            skyboxMaterial.SetColor("_HorizonColor", Color.Lerp(defaultHorizonColor, afterHorizonColor, 1 - time / changingTime));
            sceneLight.color = Color.Lerp(defaultLightColor, afterLightColor, 1 - time / changingTime);
            sceneLight.intensity = Mathf.Lerp(defaultLightIntensity, afterIntensity, 1 - time / changingTime);
            audioSource[1].volume = Mathf.Lerp(0.3f, 1f, 1 - time / changingTime);
        }

        if (time <= 0)
        {
            thisChanging = false;
        }
    }

    public void Explosion()
    {
        StartCoroutine("WeatherChange");
        StartCoroutine("ReactiveSphere");
        isReactiving = true;
        bouncyController.enabled = false;
    }

    IEnumerator WeatherChange()
    {
        //Sphereの演出
        yield return new WaitForSeconds(0.35f);
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.rotation = rotation;
        particle.Play();
        audioSource[0].Play();
        sphereAnimation.Play("ScaleDown");
        //yield return new WaitWhile(() => particle.isPlaying);

        skyboxMaterial.SetTexture("_AfterCubemap", setAfterMaterial.GetTexture("_Cubemap"));
        if (!changing && skyboxMaterial.GetTexture("_BeforeCubemap") == skyboxMaterial.GetTexture("_AfterCubemap"))
        {
            yield break;
        }
        afterHorizonColor = setAfterMaterial.GetColor("_TransmissionColor");
        thisChanging = true;
        if (prevGameObject && prevGameObject != this.gameObject)
        {
            prevGameObject.GetComponent<StageController>().thisChanging = false;
        }
        prevGameObject = this.gameObject;
        if (changing)
        {
            yield break;
        }
        defaultHorizonColor = skyboxMaterial.GetColor("_HorizonColor");
        defaultLightColor = sceneLight.color;
        defaultLightIntensity = sceneLight.intensity;
        audioSource[1].Play();
        changing = true;

        yield return new WaitWhile(() => time >= 0);
        skyboxMaterial.SetTexture("_BeforeCubemap", skyboxMaterial.GetTexture("_AfterCubemap"));
        skyboxMaterial.SetFloat("_ChangingValue", 1);
        time = changingTime;
        prevGameObject = null;
        audioSource[1].Stop();
        changing = false;
    }

    IEnumerator ReactiveSphere()
    {
        yield return new WaitForSeconds(2.0f);
        if (buttonFunction.isVisible)
        {
            bouncyController.enabled = true;
            bouncyController.Start();
            sphereAnimation.Play("ScaleUp");
        }
        isReactiving = false;
    }

}