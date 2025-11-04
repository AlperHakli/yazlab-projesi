using UnityEngine;

public class PlayerActions : MonoBehaviour
{
    [Header("Silah Referanslarý")]
    public GameObject weaponModel;
    public HitscanGun playerGunScript;
    public KeyCode toggleWeaponKey = KeyCode.Q; 

    private bool isWeaponActive = false;

    void Start()
    {

        weaponModel.SetActive(false);
        playerGunScript.enabled = false;
    }

    void Update()
    {

        if (Input.GetKeyDown(toggleWeaponKey))
        {
            isWeaponActive = !isWeaponActive;

            weaponModel.SetActive(isWeaponActive);
            playerGunScript.enabled = isWeaponActive;
        }


        if (isWeaponActive && Input.GetButtonDown("Fire1")) 
        {
            playerGunScript.TryToShoot();

        }
    }
}