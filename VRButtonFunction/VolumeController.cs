using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeController : VRButtonController {
	[SerializeField] UnityEngine.Audio.AudioMixer mixer;
	[SerializeField] Sprite muteSprite;
	Sprite defaultSprite;
	Image buttonImage;
	bool isMute;
	float initialVolume;

	override protected void Awake ()
	{
		base.Awake ();

		mixer.GetFloat("Master", out initialVolume);
		buttonImage = GetComponent<Image>();
		defaultSprite = GetComponent<Image>().sprite;
	}
	
	// override protected void FixedUpdate ()
	// {
	// 	base.FixedUpdate ();
	// }

	override protected void ButtonFunction()
	{
		if (!isMute)
		{
			isMute = true;
			mixer.SetFloat("Master", -80f);
            buttonImage.sprite = muteSprite;
            Debug.Log("OK");
		}
		else
		{
			isMute = false;
			mixer.SetFloat("Master", initialVolume);
			buttonImage.sprite = defaultSprite;
		}
	}
}
