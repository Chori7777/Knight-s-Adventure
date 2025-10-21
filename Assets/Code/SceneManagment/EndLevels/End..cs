using UnityEngine;
using UnityEngine.SceneManagement;
public class End : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        { 
        changeScene("TheForest");
        }
    }
    public void changeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
