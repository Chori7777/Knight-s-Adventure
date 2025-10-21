using UnityEngine;
using UnityEngine.SceneManagement;

public class TestRestart : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            string currentScene = SceneManager.GetActiveScene().name;
            Debug.Log("Reiniciando: " + currentScene);
            Time.timeScale = 1f;
            SceneManager.LoadScene(currentScene);
        }
    }
}
