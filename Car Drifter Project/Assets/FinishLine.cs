using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FinishLine : MonoBehaviour
{
    public Text winText;
    public bool finished;
    public float startTime;
    public float endTime;
    // Start is called before the first frame update
    void Start()
    {
        finished = true;
        winText.text = "0:00";
    }
    void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Player" && !finished)
        {
            finished = true;
            endTime = Time.time - startTime;
        } 
        else if (col.tag =="Player" && finished)
        {
            finished = false;
            startTime = Time.time;
        }
    }

    string convertTime(float timeIn)
    {
        if(timeIn == 0)
        {
            return "0:00";
        }
        string timeOut = "." + (Math.Round(timeIn % 1, 2)).ToString().Substring(2);
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
        if (!finished)
        {
            winText.text = convertTime(Time.time - startTime);
        } 
        else
        {
            winText.text = convertTime(endTime);
        }
    }

}
