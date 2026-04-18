using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    private int player_health = 150;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void take_damage(int damage)
    {
        Debug.Log("Player took dmage");
        player_health -= damage;

        if (player_health <= 0)
        {
            Debug.Log("You have Die!!");
            // TODO add gameover scene
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }


}
