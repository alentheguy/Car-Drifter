using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SpeedDisplay : MonoBehaviour
{

    public Rigidbody car;
    public Text txt;

    WheelControl[] wheels;

    // Start is called before the first frame update
    void Start()
    {
        car = GetComponent<Rigidbody>();
        wheels = GetComponentsInChildren<WheelControl>();
    }

    // Update is called once per frame
    void Update()
    {
        float mWheels = 0f;
        float rpms = 0f;
        float rWheels = 0f;
        foreach (var wheel in wheels)
        {
            if (wheel.motorized)
            {
                mWheels++;
                rpms += wheel.WheelCollider.rpm;
                rWheels += wheel.WheelCollider.radius;
            }
        }
        rpms /= mWheels;
        rWheels /= mWheels;
        var calcSpeed = 2 * Math.PI * rWheels * (rpms / 60f) * 2.2369;
        txt.text = Math.Abs(Math.Round(calcSpeed)).ToString();
    }
}
