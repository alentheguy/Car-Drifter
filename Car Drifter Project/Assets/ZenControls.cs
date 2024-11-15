using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZenControls : MonoBehaviour
{
    public GameObject car;
    Vector3 startPos;
    Quaternion startRot;
    // Start is called before the first frame update
    void Start()
    {
        startPos = car.transform.position;
        startRot = car.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("r"))
        {
            car.transform.position = startPos;
            car.transform.rotation = startRot;
            car.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
            car.GetComponent<Rigidbody>().angularVelocity = new Vector3(0, 0, 0);
        }
    }
}
