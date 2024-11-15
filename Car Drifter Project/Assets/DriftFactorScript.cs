using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DriftFactorScript : MonoBehaviour
{
    public Text label;
    public Slider driftS;

    private void Start()
    {
        label.text = "Drift Factor: (" + Math.Round(PlayerPrefs.GetFloat("driftFactor"), 2).ToString() + ")";
        driftS.value = PlayerPrefs.GetFloat("driftFactor");

    }
    public void changeDrift(float num)
    {
        PlayerPrefs.SetFloat("driftFactor", num);
        Debug.Log("changed drift: " + num.ToString());
        label.text = "Drift Factor: (" + Math.Round(num * 4, 2).ToString() + ")";
    }
}
