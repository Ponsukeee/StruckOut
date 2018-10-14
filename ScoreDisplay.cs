using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreDisplay : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI scoreText, panelText, ballText, speedText;
    [SerializeField] GameObject rootFireWork, headCamera;
    [SerializeField] Vector3 offset, finishScale;
    ParticleSystem[] particles;
    AudioSource clearSE;
    Vector3 velocity;
    static String rank;
    int avgSpeed;
    int hitQuantity = 0;
    public static int score, ball, panel, speed;
    public static float ballDivider, selectingPanelBonus;
    public static List<int> speedList;
    bool finished = false;

    void Start()
    {
        score = 0;
        ball = 0;
        panel = 9;
        speed = 0;
        avgSpeed = 0;
        ballDivider = 1.0f;
        selectingPanelBonus = 1.5f;
        speedList = new List<int>();

        clearSE = GetComponent<AudioSource>();
        var fireWorks = rootFireWork.GetChildrenWithoutGrandchild();
        particles = new ParticleSystem[fireWorks.Length];
        for(int i = 0; i < fireWorks.Length; i++)
        {
            particles[i] = fireWorks[i].GetComponent<ParticleSystem>();
        }
    }

    void Update()
    {
        scoreText.text = "Score :  " + score;
        if (!finished)
        {
            panelText.text = "Panel :  " + panel;
            ballText.text = "Ball :  " + ball;
            speedText.text = "Speed :  " + speed + "km";
        }

        if(panel <= 0 && !finished)
        {
            GameClear();
        }
        else if (finished)
        {
            transform.position = Vector3.SmoothDamp(transform.position, headCamera.transform.position + offset, ref velocity, 0.6f);
            transform.localScale = Vector3.MoveTowards(transform.localScale, finishScale, 0.0003f);
            transform.LookAt(headCamera.transform.position);
        }
    }

    public static int pointDisplay(float distancePoint, bool selectingPanelHit)
    {
        float speedPoint = Mathf.Lerp(0.0f, 50.0f, speed / GrabController.maxSpeed);
        if (ball > 9)
        {
            ballDivider = Mathf.Min(1.0f - (0.02f * ball - 9), 0.8f);
        }

        int gotPoint;
        if (selectingPanelHit)
        {
            gotPoint = (int)((distancePoint + speedPoint) * ballDivider * selectingPanelBonus);
            score += gotPoint;
            selectingPanelBonus += 0.1f; //コンボボーナス
        }
        else
        {
            gotPoint = (int)((distancePoint + speedPoint) * ballDivider);
            score += gotPoint;
            selectingPanelBonus = 1.5f; //コンボボーナスリセット
        }

        return gotPoint;
    }

    void GameClear()
    {
        finished = true;
        rank = RankJudge();

        StartCoroutine(DelayMethod(0.5f, () =>
        {
            clearSE.Play();
        }));

        //球速平均計算
        foreach (int s in speedList)
        {
            avgSpeed += s;
        }
        avgSpeed /= ball;

        //Score等表示
        scoreText.text = "Score :  " + score; 
        panelText.text = "Rank :  " + rank;
        ballText.text = "Ball :  " + ball;
        speedText.text = "Avg.Speed :  " + avgSpeed + "km";

        //花火打ち上げ
        foreach(ParticleSystem n in particles)
        {
            StartCoroutine(DelayMethod(UnityEngine.Random.Range(0f, 1.5f), () =>
            {
                n.Play();
                StartCoroutine(DelayMethod(1.5f, () =>
                {
                    n.GetComponent<AudioSource>().Play();
                }));
            }));
        }
    }

    static String RankJudge()
    {
        if (score >= 900) return "SSS";
        else if (score >= 750) return "SS";
        else if (score >= 600) return "S";
        else if (score >= 450) return "A";
        else if (score >= 300) return "B";
        else return "C";
    }

    IEnumerator DelayMethod(float waitTime, Action action)
    {
        yield return new WaitForSeconds(waitTime);
        action();
    }
}
