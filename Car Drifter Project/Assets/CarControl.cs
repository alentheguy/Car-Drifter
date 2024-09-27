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
    public float centreOfGravityOffset = -1f;
    public int[] gears = { 0, 1, 2, 3, 4, 5, 6 };
    public float[] gearSpeed = { 0, 0, 0, 0, 0, 0, 0 };
    public float[] gearTorque = { 0, 0, 0, 0, 0, 0, 0 };
    public int gear;
    public float curSpeed;
    public float brakeTime = 0.5f;
    private float nextBrake = 0f;
    private float stopBrake = 0f;

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

        gear = 2;
        gearSpeed[0] = maxSpeed / 2;
        float curSpeed = 0;
        for (int i = 2; i <= 6; i++)
        {
            curSpeed += maxSpeed / 5;
            gearSpeed[i] = curSpeed;
        }
        gearTorque[0] = motorTorque;
        float curTorque = motorTorque;
        gearTorque[2] = curTorque;
        for (int i = 3; i <= 6; i++)
        {
            curTorque -= motorTorque / 6;
            gearTorque[i] = curTorque;
        }
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

        float vInput = Input.GetAxis("Vertical");
        float hInput = Input.GetAxis("Horizontal");

        // Calculate current speed in relation to the forward direction of the car
        // (this returns a negative number when traveling backwards)
        float forwardSpeed = Vector3.Dot(transform.forward, rigidBody.velocity);

        // Calculate how close the car is to top speed
        // as a number from zero to one
        float speedFactor = Mathf.InverseLerp(0, gearSpeed[gear] + maxSpeed / 2, forwardSpeed);
        float speedFactorB = Mathf.InverseLerp(0, gearSpeed[0], -forwardSpeed);

        // Use that to calculate how much torque is available 
        // (zero torque at top speed)
        float currentMotorTorque = Mathf.Lerp(gearTorque[gear], 0, speedFactor);
        float currentMotorTorqueB = Mathf.Lerp(motorTorque, 0, speedFactorB);

        // …and to calculate how much to steer 
        // (the car steers more gently at top speed)
        float currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, speedFactor);
        //float currentSteerRange = steeringRange;

        // Check whether the user input is in the same direction 
        // as the car's velocity
        bool isAcceleratingF = (gear >= 1) && (Mathf.Sign(vInput) == Mathf.Sign(1));
        bool isAcceleratingB = (gear == 0) && (Mathf.Sign(vInput) == Mathf.Sign(-1));

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            gear = 0;
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            gear = 2;
        }

        float avgrpm = 0f;

        foreach (var wheel in wheels)
        {
            wheel.WheelCollider.brakeTorque = 0;
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

            if (isAcceleratingF)
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
            else if (isAcceleratingB)
            {
                // Apply torque to Wheel colliders that have "Motorized" enabled
                if (wheel.motorized)
                {
                    wheel.WheelCollider.motorTorque = vInput * currentMotorTorqueB;
                }
                wheel.WheelCollider.brakeTorque = 0;
            }
            else
            {
                // If the user is trying to go in the opposite direction
                // apply brakes to all wheels
                wheel.WheelCollider.brakeTorque = Mathf.Abs(vInput) * antilockBrakeSystem(brakeTorque, wheel);
                wheel.WheelCollider.motorTorque = 0;
            }
            wheel.WheelCollider.GetGroundHit(out WheelHit wh);
            WheelFrictionCurve side_ = wheel.WheelCollider.sidewaysFriction;
            side_.stiffness = 1f - (Mathf.Abs(wh.forwardSlip) * wheel.WheelCollider.forwardFriction.stiffness / 2);
            if (Input.GetKey(KeyCode.LeftShift))
            {
                side_.stiffness /= 2;//----------------------------------------------------------------------------------------set to only motorized ones
            }

            //wheel.WheelCollider.sidewaysFriction = side_;
            //WheelFrictionCurve forward_ = wheel.WheelCollider.forwardFriction;
            //forward_.stiffness = 1f - (Mathf.Abs(wh.sidewaysSlip) * wheel.WheelCollider.sidewaysFriction.stiffness);
            //wheel.WheelCollider.forwardFriction = forward_;

        }
        avgrpm /= 2;
        float circumFerence = 2.0f * 3.14f * 0.375f; // Finding circumFerence 2 Pi R
        float speedOnKmh = (circumFerence * avgrpm) / 100; // finding kmh
        float speedOnMph = speedOnKmh;// * 0.62f; // converting kmh to mph
        curSpeed = speedOnMph;

        if (speedOnMph <= gearSpeed[2] && gear > 1)
        {
            gear = 2;
        }
        else if (speedOnMph <= gearSpeed[3] && gear > 1)
        {
            gear = 3;
        }
        else if (speedOnMph <= gearSpeed[4] && gear > 1)
        {
            gear = 4;
        }
        else if (speedOnMph <= gearSpeed[5] && gear > 1)
        {
            gear = 5;
        }
        else if (speedOnMph <= gearSpeed[6] && gear > 1)
        {
            gear = 6;
        }
    }
}