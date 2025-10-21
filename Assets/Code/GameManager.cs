using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public int score;
    public static GameManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);

    }
    void Update()
    {
        // Reiniciar con tecla R
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReiniciarNivel();
        }
    }

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f; // Despausa
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}