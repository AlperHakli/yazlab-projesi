using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class HitscanGun : MonoBehaviour
{
    [Header("Silah Ayarlarý")]
    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 0.5f;

    [Header("Referanslar")]
    public Camera playerCamera;
    public Transform gunBarrelEnd;

    [Header("Ses")]
    public AudioClip shootSound;
    private AudioSource audioSource;

    private float nextFireTime = 0f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    public void TryToShoot()
    {
        if (Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }
    }

    private void Shoot()
    {
        Vector3 rayOrigin;
        Vector3 rayDirection;

        if (playerCamera != null)
        {
            rayOrigin = playerCamera.transform.position;
            rayDirection = playerCamera.transform.forward;
        }
        else if (gunBarrelEnd != null)
        {
            rayOrigin = gunBarrelEnd.position;
            rayDirection = gunBarrelEnd.forward;
        }
        else
        {
            return;
        }

        if (shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        RaycastHit hitInfo;
        if (Physics.Raycast(rayOrigin, rayDirection, out hitInfo, range))
    
        {
            Health targetHealth = hitInfo.transform.GetComponentInParent<Health>();

            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage);
            }
        }
    }
}