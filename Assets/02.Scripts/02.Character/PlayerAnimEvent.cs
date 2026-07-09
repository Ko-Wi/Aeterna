using UnityEngine;

public class PlayerAnimEvent : MonoBehaviour
{
    public PlayerController playerController;

    void Awake()
    {
        if (playerController == null)
            playerController = GetComponentInParent<PlayerController>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EnemyHit()
    {
        playerController.EnemyHit();
    }
    public void EndAttack()
    {
        playerController.EndAttack();
    }
}
