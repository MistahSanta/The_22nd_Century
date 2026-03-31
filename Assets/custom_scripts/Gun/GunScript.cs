using NUnit.Framework;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class GunScript : MonoBehaviour
{
    private bool gun_equip = false;
    private float equip_speed = 5f; 
    public Transform main_camera; 
    public GameObject bullet; 
    public Transform gun_barrel;
    public AudioSource audio_source;
    public AudioClip audio_clip;
    public ParticleSystem muzzle_flash;
    private Animator gun_animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {   
        gun_animator = GetComponentInChildren<Animator>();

        if (gun_animator == null)
        {
            Debug.Log("Forgot to add animator for gun!");
        }
    }

    public void SetGunEquip()
    {
        gun_equip = true;
    }

    public void Fire()
    {
        bullet.SetActive(true);
        GameObject spawn_bullet = Instantiate(bullet, gun_barrel.position, gun_barrel.rotation * Quaternion.Euler(90,0,0) );
        spawn_bullet.GetComponent<Rigidbody>().linearVelocity = gun_barrel.forward * 10.0f;
        
        // Play sound

        gun_animator.SetTrigger("Fire");

        if (muzzle_flash != null)
        {
            Debug.Log("Playing muzzle");
            muzzle_flash.Play();
        }

        Destroy(spawn_bullet, 4);
        bullet.SetActive(false);
    }


    // Update is called once per frame
    void LateUpdate()
    {
        if (gun_equip == false) return; 


        // // Gun is equip so follow the player 
        // Rigidbody rb = GetComponent<Rigidbody>();
        // rb.isKinematic = true;

        Vector3 infront_player_position = main_camera.position + (main_camera.forward * 0.5f) + (main_camera.right * 0.25f) - (main_camera.up * 0.25f);
        transform.position = Vector3.Lerp( transform.position, infront_player_position, equip_speed);
        transform.rotation = main_camera.rotation * Quaternion.Euler(0, 0, 0);
        
        if (Input.GetButtonDown("js0") || Input.GetButtonDown("js7"))
        { // OK button is pressed or 'h' button keyboard
            Fire();
        }
            
    }
}
