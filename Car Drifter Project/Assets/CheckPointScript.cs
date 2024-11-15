using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointScript : MonoBehaviour
{
    public bool passed;
    // Start is called before the first frame update
    void Start()
    {
        passed = false;
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Player")
        {
            passed = true;
        }
    }
}
