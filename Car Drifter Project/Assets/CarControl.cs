using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CarControl : MonoBehaviour
{
    public float motorTorque = 2000;
    public float brakeTorque = 2000;
    public float maxSpeed = 1;
    public float steeringRange = 30;
    public float steeringRangeAtMaxSpeed = 10;
    public float centreOfGravityOffset = 0;
    public Camera firstPerson;
    public AudioListener firstPersonA;
    public Camera thirdPerson;
    public AudioListener thirdPersonA;
    public ParticleSystem smokeRR;
    public ParticleSystem smokeRL;
    public ParticleSystem smokeFR;
    public ParticleSystem smokeFL;
    public float curSpeed;
    public float brakeTime = 0.5f;
    private float nextBrake = 0f;
    private float stopBrake = 0f;
    public float torqueRN = 0f;
    public float returnedTorque = 0f;
    public float braking = 0f;
    public bool accelerating = false;
    public Color smokeColor;
    public float curStiffMot = 1.5f;

    WheelControl[] wheels;
    Rigidbody rigidBody;
    ParticleSystem.MainModule mainRR;
    ParticleSystem.MainModule mainRL;
    ParticleSystem.MainModule mainFR;
    ParticleSystem.MainModule mainFL;

    // Start is called before the first frame update
    void Start()
    {
        motorTorque = PlayerPrefs.GetFloat("engineTorque");
        maxSpeed = PlayerPrefs.GetFloat("maxSpeed");

        mainRR = smokeRR.main;
        mainRL = smokeRL.main;
        mainFR = smokeFR.main;
        mainFL = smokeFL.main;
        rigidBody = GetComponent<Rigidbody>();

        // Adjust center of mass vertically, to help prevent the car from rolling
        rigidBody.centerOfMass += Vector3.up * centreOfGravityOffset;

        // Find all child GameObjects that have the WheelControl script attached
        wheels = GetComponentsInChildren<WheelControl>();

        // Start with third person camera
        thirdPerson.enabled = true;
        thirdPersonA.enabled = true;
        firstPerson.enabled = false;
        firstPersonA.enabled = false;
    }

    public float getTorque(float speed)
    {
        float curTorque = ((0.1f * maxSpeed * motorTorque) / (speed - (maxSpeed / 20f))) * ((-1 * (float)Math.Pow((speed / maxSpeed), 10f)) + 1);
        if (curTorque > motorTorque || curTorque < 0f)
        {
            returnedTorque = motorTorque;
            return motorTorque;
        }
        if (speed >= maxSpeed)
        {
            return 0;
        }
        returnedTorque = curTorque;
        return curTorque;
    }

    public float tractionControl(float mt, WheelControl wheel)
    {
        float mtf;
        wheel.WheelCollider.GetGroundHit(out WheelHit f);
        if (Math.Abs(f.forwardSlip) >= 0.25f)
        {
            mtf = 0f;
        }
        else
        {
            mtf = Mathf.Lerp(mt, 0, f.forwardSlip * 4);
        }

        return mtf;
    }

    public float antilockBrakeSystem(float bt, WheelControl wheel)
    {
        float btf;
        wheel.WheelCollider.GetGroundHit(out WheelHit f);
        if (f.forwardSlip > 0)
        {
            btf = bt;
        }
        else if (f.forwardSlip <= -0.667f)
        {
            btf = 0f;
        }
        else
        {
            btf = Mathf.Lerp(bt, 0, f.forwardSlip * 1.5f);
        }
        if (Time.time < stopBrake || rigidBody.velocity.magnitude < 0.5)
        {
        }
        else if (Time.time > nextBrake)
        {
            stopBrake = Time.time + (brakeTime / 1.5f);
            nextBrake = Time.time + brakeTime;
        }
        else
        {
            btf = 0f;
        }
        return btf;
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.C))
        {
            firstPerson.enabled = !firstPerson.enabled;
            thirdPerson.enabled = !thirdPerson.enabled;
            firstPersonA.enabled = !firstPersonA.enabled;
            thirdPersonA.enabled = !thirdPersonA.enabled;
        }

        float vInput = Input.GetAxis("Vertical");
        float hInput = Input.GetAxis("Horizontal");

        // Calculate current speed in relation to the forward direction of the car
        // (this returns a negative number when traveling backwards)
        float forwardSpeed = Vector3.Dot(transform.forward, rigidBody.velocity);

        // Calculate how close the car is to top speed
        // as a number from zero to one
        float speedFactor = Mathf.InverseLerp(0, maxSpeed, forwardSpeed);

        // Use that to calculate how much torque is available 
        // (zero torque at top speed)
        float currentMotorTorque = getTorque(GetComponent<SpeedDisplay>().speedCalc);

        // …and to calculate how much to steer 
        // (the car steers more gently at top speed)
        float currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, speedFactor);
        //float currentSteerRange = steeringRange;

        bool isAccelerating = (Mathf.Sign(vInput) == Mathf.Sign(forwardSpeed));

        accelerating = isAccelerating;


        float avgtorque = 0f;
        float avgSlipM = 0f;
        float avgSlipS = 0f;

        foreach (var wheel in wheels)
        {
            float rpm = wheel.WheelCollider.rpm;
            wheel.WheelCollider.GetGroundHit(out WheelHit wh);
            // Apply steering to Wheel colliders that have "Steerable" enabled
            if (wheel.steerable)
            {
                wheel.WheelCollider.steerAngle = hInput * currentSteerRange;
                avgSlipS += wh.sidewaysSlip;
            }

            if (wheel.motorized)
            {
                avgSlipM += (float)Math.Sqrt(Math.Pow(wh.forwardSlip, 2) + Math.Pow(wh.sidewaysSlip, 2));
            }

            if (isAccelerating)
            {
                // Apply torque to Wheel colliders that have "Motorized" enabled
                if (wheel.motorized)
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        wheel.WheelCollider.motorTorque = vInput * currentMotorTorque;
                    }
                    else
                    {
                        wheel.WheelCollider.motorTorque = vInput * tractionControl(currentMotorTorque, wheel);
                    }
                    //if(Mathf.Abs(wheel.WheelCollider.rpm) > 1000)
                    //{
                    //    wheel.WheelCollider.motorTorque = 0;
                    //}
                }
                wheel.WheelCollider.brakeTorque = 0;
            }
            else
            {
                // If the user is trying to go in the opposite direction
                // apply brakes to all wheels
                wheel.WheelCollider.motorTorque = 0;
                wheel.WheelCollider.brakeTorque = Mathf.Abs(vInput) * antilockBrakeSystem(brakeTorque, wheel);
                braking = Mathf.Abs(vInput) * antilockBrakeSystem(brakeTorque, wheel);
            }
            WheelFrictionCurve side_ = wheel.WheelCollider.sidewaysFriction;
            if (Input.GetKey(KeyCode.LeftShift) && wheel.motorized)
            {
                side_.stiffness = 0.7f * (1f - PlayerPrefs.GetFloat("driftFactor"));
                wheel.WheelCollider.sidewaysFriction = side_;
            }
            else if (wheel.motorized)
            {
                side_.stiffness = curStiffMot * (1f - PlayerPrefs.GetFloat("driftFactor"));
                wheel.WheelCollider.sidewaysFriction = side_;

            }
            avgtorque += wheel.WheelCollider.motorTorque;
        }
        avgSlipM /= 2;
        avgSlipS /= 2;
        smokeColor = new Color(1, 1, 1, Math.Abs(avgSlipM * avgSlipM));
        mainRR.startColor = new Color(1, 1, 1, Math.Abs((0.75f) * (float)Math.Pow(avgSlipM, 3)));
        mainRL.startColor = new Color(1, 1, 1, Math.Abs((0.75f) * (float)Math.Pow(avgSlipM, 3)));
        mainFR.startColor = new Color(1, 1, 1, Math.Abs((0.75f) * (float)Math.Pow(avgSlipS, 3)));
        mainFL.startColor = new Color(1, 1, 1, Math.Abs((0.75f) * (float)Math.Pow(avgSlipS, 3)));

        curSpeed = forwardSpeed;
    }
}