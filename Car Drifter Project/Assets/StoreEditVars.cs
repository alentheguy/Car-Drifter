using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreEditVars : MonoBehaviour
{
    public InputField torque;
    public Text torquePH;
    public InputField speed;
    public Text speedPH;
    // Start is called before the first frame update
    private void Start()
    {
        torque.onEndEdit.AddListener(editTorque);
        torquePH.text = PlayerPrefs.GetFloat("engineTorque").ToString();
        speed.onEndEdit.AddListener(editSpeed);
        float curSpeed = PlayerPrefs.GetFloat("maxSpeed");
        if(curSpeed <= 84.69784)
        {
            speedPH.text = Math.Round(curSpeed).ToString();
        }
        else
        {
            speedPH.text = Math.Round(7.5 * Math.Pow(curSpeed, 0.5461)).ToString();
        }
    }

    private void editTorque(string nums)
    {
        float motorTorque = float.Parse(nums);
        if(motorTorque < 0)
        {
            motorTorque = 0;
        }
        if(motorTorque > 2000)
        {
            motorTorque = 2000;
        }
        torque.text = "";
        PlayerPrefs.SetFloat("engineTorque", motorTorque);
        torquePH.text = PlayerPrefs.GetFloat("engineTorque").ToString();
    }

    private void editSpeed(string nums)
    {
        speed.text = "";
        float maxSpeedB = float.Parse(nums);
        if (maxSpeedB > 500)
        {
            maxSpeedB = 500;
        }
        float maxSpeed;
        if (maxSpeedB <= 84.69784)
        {
            maxSpeed = maxSpeedB;
            PlayerPrefs.SetFloat("maxSpeed", maxSpeed);
            speedPH.text = Math.Round(PlayerPrefs.GetFloat("maxSpeed")).ToString();
        }
        else
        {
            maxSpeed = (float)Math.Pow(maxSpeedB / 7.5f, (1f / 0.5461f));
            PlayerPrefs.SetFloat("maxSpeed", maxSpeed);
            speedPH.text = Math.Round(7.5f * Math.Pow(PlayerPrefs.GetFloat("maxSpeed"), 0.5461f)).ToString();
        }
        Debug.Log("speed now: " + maxSpeed);
    }
}
