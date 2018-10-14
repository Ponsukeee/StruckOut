using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultSettingsSkybox : MonoBehaviour {
    public Cubemap defaultSkyboxTexture;
    public Color defaultHorizonColor;

	void Awake () {
        var skyboxMaterial = RenderSettings.skybox;
        skyboxMaterial.SetTexture("_BeforeCubemap", defaultSkyboxTexture);
        skyboxMaterial.SetColor("_HorizonColor", defaultHorizonColor);
        skyboxMaterial.SetFloat("_ChangingValue", 1.0f);
	}
}
