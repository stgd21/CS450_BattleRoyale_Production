using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerWeapon : MonoBehaviour
{
    [Header("Stats")]
    public int damage;
    public int curAmmo;
    public int maxAmmo;
    public float bulletSpeed;
    public float shootRate;

    private float lastShootTime;

    public GameObject bulletPrefab;
    public Transform bulletSpawnPos;

    private PlayerController player;

    private void Awake()
    {
        //get required components
        player = GetComponent<PlayerController>();
    }

    public void TryShoot()
    {
        //can we shoot?
        if (curAmmo <= 0 || Time.time - lastShootTime < shootRate)
            return;

        curAmmo--;
        lastShootTime = Time.time;

        //update the ammo UI
        GameUI.instance.UpdateAmmoText();

        //spawn the bullet
        player.photonView.RPC("SpawnBullet", RpcTarget.All, bulletSpawnPos.transform.position, Camera.main.transform.forward);
    }

    [PunRPC]
    void SpawnBullet(Vector3 pos, Vector3 dir)
    {
        //spawn and orientate it
        GameObject bulletObj = Instantiate(bulletPrefab, pos, Quaternion.identity);
        bulletObj.transform.forward = dir;

        //get bullet script
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();

        //initialize it and set velocity
        bulletScript.Initialize(damage, player.id, player.photonView.IsMine);

        if (player.transform.parent != null)
        {
            bulletScript.rig.velocity = dir * bulletSpeed + player.transform.parent.gameObject.GetComponent<Rigidbody>().velocity;
        }
        else
        {
            bulletScript.rig.velocity = dir * bulletSpeed + player.rig.velocity;
        }
        
    }

    [PunRPC]
    public void GiveAmmo(int ammoToGive)
    {
        curAmmo = Mathf.Clamp(curAmmo + ammoToGive, 0, maxAmmo);
        //update ammo text
        GameUI.instance.UpdateAmmoText();
    }
}
