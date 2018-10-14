using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonusPoint : MonoBehaviour {
    Animation bonusAnimation;
    AudioSource bonusSE;
    ParticleSystem particle;

    private void Awake()
    {
        bonusAnimation = GetComponent<Animation>();
        bonusSE = GetComponent<AudioSource>();
        particle = GetComponentInChildren<ParticleSystem>();
    }

    public void BonusProcess(int hitCount)
    {
        if(hitCount >= 2)
        {
            particle.Play();
            bonusSE.Play();
            bonusAnimation.Play();

            StartCoroutine(WaitWhilePlaying(hitCount));
        }
    }

    IEnumerator WaitWhilePlaying(int hitCount)
    {
        yield return new WaitWhile(() => bonusAnimation.isPlaying);
        ScoreDisplay.score += (int)Mathf.Pow(hitCount, 2) * 30;
    }
}
