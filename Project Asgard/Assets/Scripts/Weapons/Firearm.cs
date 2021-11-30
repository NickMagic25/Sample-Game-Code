using UnityEngine;

public abstract class Firearm : MonoBehaviour
{

    [SerializeField] private Camera fpsCam;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float range = 100f;
    [SerializeField] private float fireRate = 300f;
    [SerializeField] private float magSize = 30f;
    [SerializeField] private ParticleSystem muzzleFlash;

    public Damageable gunHolder;

    private float nextTimeToFire = 0f;
    private Weapon_Recoil recoil;

    private void Start()
    {
        recoil = this.GetComponent<Weapon_Recoil>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            if (recoil != null)
            {
                recoil.Fire();
            }
            Shoot();
        }
    }

    public abstract void Shoot();

    public Camera GetFPSCam() {
        return fpsCam;
    }

    public float GetDamage() 
    {
        return damage;
    }

    public float GetRange() 
    {
        return range;
    }

    public float getMagSize() 
    {
        return magSize;
    }

    public void SetGunHolder(Damageable entity) 
    {
        gunHolder = entity;
    }

    public void setMuzzleFlash() 
    {
        muzzleFlash.Play();
    }

}
