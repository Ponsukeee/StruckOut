using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HitCount : MonoBehaviour {
    GameObject bonusDisplay;
	int hitCount = 0;
	bool firstContact = true;

    private void Start()
    {
        bonusDisplay = GameObject.Find("BonusDisplay");
        bonusDisplay.transform.localScale = Vector3.zero;
    }

    void OnCollisionEnter(Collision collision)
	{
		if(collision.collider.CompareTag("Panel"))
		{
			if(firstContact)
			{
				firstContact = false;
				StartCoroutine(DelayMethod(0.1f, () =>
				{
                    //bonusDisplay.GetComponent<BonusPoint>().BonusProcess(hitCount);
                    HitQuantityBonus(hitCount);
                }));
			}
			hitCount++;
		}
	}

	void HitQuantityBonus(int hitCount)
	{
        if(hitCount >= 2)
        {
            //演出
            var bonusAnimation = bonusDisplay.GetComponent<Animation>();
            bonusAnimation.Play();
            bonusDisplay.GetComponent<AudioSource>().Play();
            bonusDisplay.GetComponentInChildren<ParticleSystem>().Play();

            //score加算、得点表示
            var bonus = (int)Mathf.Pow(hitCount, 2) * 30;
            bonusDisplay.GetComponent<TextMeshProUGUI>().text = "+" + bonus;
            StartCoroutine(WaitWhilePlaying(bonusAnimation, bonus));
        }
    }

	IEnumerator DelayMethod(float waitTime, Action action)
	{
		yield return new WaitForSeconds(waitTime);
		action();
	}

    IEnumerator WaitWhilePlaying (Animation animation, int bonus)
    {
        yield return new WaitWhile(() => animation.isPlaying);
        ScoreDisplay.score += bonus;
    }
}
