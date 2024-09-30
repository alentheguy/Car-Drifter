using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SpeedDisplay : MonoBehaviour
{

    public Rigidbody car;
    public Text txt;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var speed = Math.Round(car.velocity.magnitude * 2.2369, 1);
        txt.text = speed.ToString();
    }
}
