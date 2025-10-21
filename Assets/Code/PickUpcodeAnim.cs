using UnityEngine;

public class PickUpcodeAnim : MonoBehaviour
{
    Animator anim;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        anim=GetComponent<Animator>();
    }
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Player")
        { 
            anim.SetTrigger("PickUp");

        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            anim.SetTrigger("PickUp");

        }
    }
    public void Destroy()
    {
        Destroy(gameObject);
    }
}
