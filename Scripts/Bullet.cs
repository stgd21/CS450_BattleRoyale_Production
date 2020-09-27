using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private int damage;
    private int attackerId;
    private bool isMine;

    public Rigidbody rig;

    public void Initialize(int damage, int attackerId, bool isMine)
    {
        this.damage = damage;
        this.attackerId = attackerId;
        this.isMine = isMine;

        Destroy(gameObject, 5.0f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && isMine)
        {
            //if we hit a player and if this is the local player's bullet, damage the hit player
            //this is client side hit detection
            PlayerController player = GameManager.instance.GetPlayer(other.gameObject);

            if (player.id != attackerId)
                player.photonView.RPC("TakeDamage", player.photonPlayer, attackerId, damage);
        }
        Destroy(gameObject);
    }
}
