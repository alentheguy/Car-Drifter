using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TogglePanelScript : MonoBehaviour
{
    public GameObject panel;
    public GameObject otherButton;

    // Start is called before the first frame update
    void Start()
    {
        panel.SetActive(false);
    }

    public void displayPanel()
    {
        panel.SetActive(true);
        otherButton.GetComponent<Button>().enabled = false;
    }

    public void hidePanel()
    {
        panel.SetActive(false);
        otherButton.GetComponent<Button>().enabled = true;
    }


}
