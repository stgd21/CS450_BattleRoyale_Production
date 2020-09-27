using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Motorcycle : MonoBehaviourPun, IPunObservable
{
    public Transform onBikePosition;
    public Transform offBikePosition;
    public Rigidbody rig;
    public float moveSpeed;
    public float z = -1f; //the correct player movement input on the bike
    public int motorcycleHitDamage = 500;
    public GameObject physicalComponentParent;
    public AudioSource bikeStartAudio;
    public AudioSource bikeIdleAudio;
    public AudioSource bikeRevAudio;
    public AudioSource explosionAudio;

    private GameObject playerOnBike;
    
    private bool isOnBike = false;

    private void OnCollisionEnter(Collision other)
    {
        //Debug.Log("Collided with " + other.gameObject.name + "at speed of " + rig.velocity.magnitude);
        //Getting on the bike
        if (other.gameObject.tag == "Player" && isOnBike == false)
        {
            //If the player hits the motorcycle
            if (other.gameObject.GetComponent<PlayerController>().photonView.IsMine)
            {
                    photonView.RPC("GetOnBike", RpcTarget.All, GameManager.instance.GetPlayer(other.gameObject).id);
            }
        }

        //killing a player
        else if (other.gameObject.tag == "Player" && isOnBike == true)
        {
            if (rig.velocity.magnitude > 40f)
            {
                PhotonView hitPlayer = other.gameObject.GetPhotonView();
                hitPlayer.RPC("TakeDamage", hitPlayer.GetComponent<PlayerController>().photonPlayer, playerOnBike.GetComponent<PlayerController>().id, motorcycleHitDamage);
            }
        }
        
        //Explode if it hits obstacles at high speed
        else if ( other.gameObject.tag != "Terrain" && other.gameObject.tag != "Player")
        {
            photonView.RPC("Explode", RpcTarget.All);
        }
    }

    [PunRPC]
    public void Explode()
    {
        bikeStartAudio.Stop();
        bikeIdleAudio.Stop();
        bikeRevAudio.Stop();
        explosionAudio.Play();
        if (isOnBike == true)
        {
            //playerOnBike.GetPhotonView().RPC("TakeDamage", playerOnBike.GetComponent<PlayerController>().photonPlayer, playerOnBike.GetComponent<PlayerController>().id, motorcycleHitDamage);
            playerOnBike.GetComponent<PlayerController>().TakeDamage(playerOnBike.GetComponent<PlayerController>().id, motorcycleHitDamage);
            playerOnBike.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            GetOffBike(); 
        }
        foreach (Transform child in physicalComponentParent.transform)
        {
            child.GetComponent<Rigidbody>().isKinematic = false;
        }
        Destroy(this);
    }

    private void LateUpdate()
    {
        if (isOnBike)
        {
            if (Input.GetKeyDown(KeyCode.E))
                photonView.RPC("GetOffBike", RpcTarget.All);
            else
            {
                Move();
                if (bikeStartAudio.isPlaying || bikeRevAudio.isPlaying)
                    bikeIdleAudio.Stop();
                else if (!bikeIdleAudio.isPlaying)
                {
                    bikeIdleAudio.Play();
                }
            }
                
        }
    }

    private void Move()
    {
        //Just look where the player is looking, controlled by camera
        transform.rotation = playerOnBike.transform.rotation;

        //get the input axis
        if (playerOnBike.GetPhotonView().IsMine)
        {
            z = Input.GetAxis("Vertical");
            //stream this to everone
        }
        //calculate direction relative to where we're facing
        if (z != -1)
        {
            Vector3 dir = (transform.forward * z) * moveSpeed;
            dir.y = rig.velocity.y;

            //set that as our velocity
            rig.velocity = dir;

            if (!bikeRevAudio.isPlaying && !bikeStartAudio.isPlaying)
                bikeRevAudio.Play();
            if (z == 0)
                bikeRevAudio.Stop();
        }
        
    }

    [PunRPC]
    private void GetOffBike()
    {
        playerOnBike.transform.parent = null;
        playerOnBike.transform.position = offBikePosition.position;
        playerOnBike.GetComponent<Rigidbody>().isKinematic = false;
        playerOnBike.GetComponent<CapsuleCollider>().isTrigger = false;
        playerOnBike = null;
        isOnBike = false;
    }

    [PunRPC]
    private void GetOnBike(int newPlayerId)
    {
        //if its me that hits the motorcycle
        playerOnBike = GameManager.instance.GetPlayer(newPlayerId).gameObject;

        //put the player on the bike and make it kinematic
        playerOnBike.GetComponent<CapsuleCollider>().isTrigger = true;
        playerOnBike.GetComponent<Rigidbody>().isKinematic = true;
        playerOnBike.transform.SetParent(gameObject.transform);
        playerOnBike.transform.position = onBikePosition.position;

        //at this point we are frozen on the bike, no need to handle playercontroller movement
        isOnBike = true;
        rig.isKinematic = false;

        bikeStartAudio.Play();
       
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            if (playerOnBike != null && playerOnBike.GetPhotonView().IsMine)
                stream.SendNext(z);
            //stream.SendNext(curHatTime);
        }
        else if (stream.IsReading)
        {
            z = (float)stream.ReceiveNext();
            //curHatTime = (float)stream.ReceiveNext();
        }
    }
}
