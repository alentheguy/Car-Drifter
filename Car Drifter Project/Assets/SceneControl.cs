using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneControl : MonoBehaviour
{
    public void Race()
    {
        SceneManager.LoadScene("Race");
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
