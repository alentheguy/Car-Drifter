using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarControl : MonoBehaviour
{
    public float motorTorque = 2000;
    public float brakeTorque = 2000;
    public float maxSpeed = 1;
    public float steeringRange = 30;
    public float steeringRangeAtMaxSpeed = 10;
    public float centreOfGravityOffset = 0;
    public Camera firstPerson;
    public Camera thirdPerson;
    public float curSpeed;
    public float brakeTime = 0.5f;
    private float nextBrake = 0f;
    private float stopBrake = 0f;
    public float torqueRN = 0f;
    public float returnedTorque = 0f;
    public float braking = 0f;
    public bool accelerating = false;

    WheelControl[] wheels;
    Rigidbody rigidBody;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();

        // Adjust center of mass vertically, to help prevent the car from rolling
        rigidBody.centerOfMass += Vector3.up * centreOfGravityOffset;

        // Find all child GameObjects that have the WheelControl script attached
        wheels = GetComponentsInChildren<WheelControl>();

        // Start with third person camera
        thirdPerson.enabled = true;
        firstPerson.enabled = false;
    }

    public float getTorque(float speed)
    {
        float curTorque = ((0.002f * maxSpeed * motorTorque) / (0.02f * (speed - (maxSpeed / 20f)))) + (motorTorque / 10);
        if (curTorque > motorTorque || curTorque < 0f)
        {
            returnedTorque = motorTorque;
            return motorTorque;
        }
        returnedTorque = curTorque;
        return curTorque;
    }

    public float tractionControl(float mt, WheelControl wheel)
    {
        float mtf;
        wheel.WheelCollider.GetGroundHit(out WheelHit f);
        if (f.forwardSlip < 0)
        {
            mtf = mt;
        }
        else if (f.forwardSlip >= 0.5f)
        {
            mtf = 0f;
        }
        else
        {
            mtf = Mathf.Lerp(mt, 0, f.forwardSlip * 2);
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
        float currentMotorTorque = getTorque(forwardSpeed);

        // …and to calculate how much to steer 
        // (the car steers more gently at top speed)
        float currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, speedFactor);
        //float currentSteerRange = steeringRange;

        bool isAccelerating = (Mathf.Sign(vInput) == Mathf.Sign(forwardSpeed));

        accelerating = isAccelerating;


        float avgrpm = 0f;
        float avgtorque = 0f;

        foreach (var wheel in wheels)
        {
            float rpm = wheel.WheelCollider.rpm;
            // Apply steering to Wheel colliders that have "Steerable" enabled
            if (wheel.steerable)
            {
                wheel.WheelCollider.steerAngle = hInput * currentSteerRange;
            }

            if (wheel.motorized)
            {
                avgrpm += rpm;
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
                    if(Mathf.Abs(wheel.WheelCollider.rpm) > 1000)
                    {
                        wheel.WheelCollider.motorTorque = 0;
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
                braking = Mathf.Abs(vInput) * antilockBrakeSystem(brakeTorque, wheel);
            }
            wheel.WheelCollider.GetGroundHit(out WheelHit wh);
            WheelFrictionCurve side_ = wheel.WheelCollider.sidewaysFriction;
            side_.stiffness = 1f - (Mathf.Abs(wh.forwardSlip) / 2);
            if (Input.GetKey(KeyCode.LeftShift) && wheel.motorized)
            {
                side_.stiffness /= 1.25f;
            }
            avgtorque += wheel.WheelCollider.motorTorque;
        }
        avgrpm /= 2;
        torqueRN = avgtorque / 2;
        float circumFerence = 2.0f * 3.14f * 0.375f; // Finding circumFerence 2 Pi R
        float speedOnKmh = (circumFerence * avgrpm) / 100; // finding kmh
        float speedOnMph = speedOnKmh;// * 0.62f; // converting kmh to mph
        curSpeed = speedOnMph;
    }
}