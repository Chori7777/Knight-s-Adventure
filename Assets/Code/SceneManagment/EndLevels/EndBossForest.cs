using UnityEngine;
using UnityEngine.SceneManagement;
public class EndBossForest : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        { 
        changeScene("ForestBoss");
        }
    }
    public void changeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
