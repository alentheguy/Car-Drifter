using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FinishLine : MonoBehaviour
{
    public Text curTime;
    public Text winTime;
    public Text highTime;
    public Text youWinText;
    public GameObject panel;
    public int finished; //  1 -> in progress, 2 -> finished
    public float startTime;
    public float endTime;
    public GameObject checkPoint;
    
    // Start is called before the first frame update
    void Start()
    {
        panel.SetActive(false);
        finished = 0;
        curTime.text = "0:00";
        checkPoint.SetActive(false);
    }
    void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Player" && finished == 1 && checkPoint.GetComponent<CheckPointScript>().passed)
        {
            finished = 2;
            endTime = Time.time - startTime;
        } 
        else if (col.tag =="Player" && finished != 1)
        {
            finished = 1;
            startTime = Time.time;
            checkPoint.SetActive(true);
        }
    }

    string convertTime(float timeIn)
    {
        if(timeIn == 0)
        {
            return "0:00";
        }
        string timeOut = (Math.Round(timeIn % 1, 2)).ToString().Substring(1);
        int timeSeconds = (int)timeIn;
        if(timeSeconds % 60 < 10)
        {
            timeOut = ":0" + timeSeconds % 60 + timeOut;
        }
        else
        {
            timeOut = ":" + timeSeconds % 60 + timeOut;
        }
        timeOut = timeSeconds / 60 + timeOut;
        return timeOut;
    }

    private void Update()
    {
        if (finished == 1)
        {
            curTime.text = convertTime(Time.time - startTime);
        } 
        else if (finished == 2)
        {
            if (endTime < PlayerPrefs.GetFloat("highScore", int.MaxValue))
            {
                PlayerPrefs.SetFloat("highScore", endTime);
                youWinText.text = "High Score!!!!!!";
            }
            string win = convertTime(endTime);
            curTime.text = win;
            winTime.text = win;
            highTime.text = convertTime(PlayerPrefs.GetFloat("highScore"));
            panel.SetActive(true);

        }
        if (panel.active)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }

}
