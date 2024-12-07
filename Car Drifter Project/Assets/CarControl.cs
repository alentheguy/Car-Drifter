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
    public float centreOfGravityOffset = -0.2f;
    public Camera firstPerson;
    public AudioListener firstPersonA;
    public Camera thirdPerson;
    public AudioListener thirdPersonA;
    public ParticleSystem smokeRR;
    public ParticleSystem smokeRL;
    public ParticleSystem smokeFR;
    public ParticleSystem smokeFL;
    public float brakeTime = 0.5f;
    private float nextBrake = 0f;
    private float stopBrake = 0f;
    public float curStiffMot = 2f;

    WheelControl[] wheels;
    Rigidbody rigidBody;
    ParticleSystem.MainModule mainRR;
    ParticleSystem.MainModule mainRL;
    ParticleSystem.MainModule mainFR;
    ParticleSystem.MainModule mainFL;

    // Start is called before the first frame update
    void Start()
    {
        motorTorque = PlayerPrefs.GetFloat("engineTorque", 500);
        brakeTorque = motorTorque * 0.4f;
        float pulledSpeed = PlayerPrefs.GetFloat("maxSpeed", 200);
        pulledSpeed /= (30.46619f * (float)Math.Pow(motorTorque, 0.450991)) / 500;
        maxSpeed = pulledSpeed;

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

    public float getTorque(float speed, float curMaxSpeed)
    {
        float curTorque = ((0.1f * curMaxSpeed * motorTorque) / (speed - (curMaxSpeed / 20f))) * ((-1 * (float)Math.Pow((speed / curMaxSpeed), 10f)) + 1);
        if (curTorque > motorTorque || speed < (0.15f * curMaxSpeed))
        {
            return motorTorque;
        }
        if (speed >= curMaxSpeed)
        {
            return 0;
        }
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
        if (Time.time < stopBrake)
        {
        } 
        else if (rigidBody.velocity.magnitude < 0.5)
        {
            btf = bt;
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

        float spin = rigidBody.angularVelocity.y;
        if (Mathf.Sign(spin) != MathF.Sign(hInput) && spin > 1f && Mathf.Sign(vInput) == Mathf.Sign(1))
        {
            rigidBody.AddTorque(transform.up * hInput * 100 * (float)Math.Abs(Math.Pow(spin, 2)));
        }
        else
        {
            rigidBody.AddTorque(transform.up * hInput * 50 * vInput);
        }

        // Calculate current speed in relation to the forward direction of the car
        // (this returns a negative number when traveling backwards)
        float forwardSpeed = Vector3.Dot(transform.forward, rigidBody.velocity);

        // Calculate how close the car is to top speed
        // as a number from zero to one
        float speedFactor = Mathf.InverseLerp(0, maxSpeed, forwardSpeed);

        // Use that to calculate how much torque is available 
        // (zero torque at top speed)
        float curMaxSpeed = maxSpeed;
        if (Math.Sign(vInput) == Math.Sign(-1))
        {
            curMaxSpeed /= 4;
        }
        float currentMotorTorque = getTorque(GetComponent<SpeedDisplay>().speedCalc, curMaxSpeed);

        // …and to calculate how much to steer 
        // (the car steers more gently at top speed)
        float currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, speedFactor);
        //float currentSteerRange = steeringRange;

        bool isAccelerating = (Mathf.Sign(vInput) == Mathf.Sign(forwardSpeed));

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

            if (Input.GetKey(KeyCode.LeftControl))
            {
                wheel.WheelCollider.motorTorque = 0;
                wheel.WheelCollider.brakeTorque = brakeTorque / 2;
            }
            else if (isAccelerating)
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
                }
                wheel.WheelCollider.brakeTorque = 0;
            }
            else
            {
                // If the user is trying to go in the opposite direction
                // apply brakes to all wheels
                wheel.WheelCollider.motorTorque = 0;
                wheel.WheelCollider.brakeTorque = Mathf.Abs(vInput) * antilockBrakeSystem(brakeTorque, wheel);
            }
            WheelFrictionCurve side_ = wheel.WheelCollider.sidewaysFriction;
            if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftControl)) && wheel.motorized)
            {
                side_.stiffness = (1f - PlayerPrefs.GetFloat("driftFactor"));
                wheel.WheelCollider.sidewaysFriction = side_;
            }
            else if (wheel.motorized)
            {
                side_.stiffness = curStiffMot * (1f - PlayerPrefs.GetFloat("driftFactor"));
                wheel.WheelCollider.sidewaysFriction = side_;

            }
            
        }
        avgSlipM /= 2;
        avgSlipS /= 2;
        mainRR.startColor = new Color(1, 1, 1, Math.Abs((0.75f) * (float)Math.Pow(avgSlipM, 3)));
        mainRL.startColor = new Color(1, 1, 1, Math.Abs((0.75f) * (float)Math.Pow(avgSlipM, 3)));
        mainFR.startColor = new Color(1, 1, 1, Math.Abs((0.75f) * (float)Math.Pow(avgSlipS, 3)));
        mainFL.startColor = new Color(1, 1, 1, Math.Abs((0.75f) * (float)Math.Pow(avgSlipS, 3)));
    }
}