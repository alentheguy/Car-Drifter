using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelControl : MonoBehaviour
{
    public Transform wheelModel;

    [HideInInspector] public WheelCollider WheelCollider;

    // Create properties for the CarControl script
    // (You should enable/disable these via the 
    // Editor Inspector window)
    public bool steerable;
    public bool motorized;
    private float firstPosition;

    Vector3 position;
    Quaternion rotation;

    // Start is called before the first frame update
    private void Start()
    {
        WheelCollider = GetComponent<WheelCollider>();
        firstPosition = WheelCollider.transform.position.y;

    }

    // Update is called once per frame
    void Update()
    {
        // Get the Wheel collider's world pose values and
        // use them to set the wheel model's position and rotation
        WheelCollider.GetWorldPose(out position, out rotation);
        float positionChange = firstPosition - position.y;
        //wheelModel.transform.position.Set(wheelModel.transform.position.x, firstPosition + positionChange, wheelModel.transform.position.z);
        wheelModel.transform.position = position;
        wheelModel.transform.rotation = rotation;
        //wheelModel.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
        //wheelModel.Rotate(rotation.x, rotation.y, rotation.z, Space.Self);
    }
}