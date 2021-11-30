using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssultRiffle : Firearm
{
    [SerializeField] private ParticleSystem hitPointSystem;


    public override void Shoot()
    {
        setMuzzleFlash();
        RaycastHit hit;
        if (Physics.Raycast(this.GetFPSCam().transform.position, this.GetFPSCam().transform.forward, out hit))
        {

            if (hit.transform == gunHolder.transform) 
            {
                // exit if the raycast hits the gun holder, important for when player is jumping around so they can't shoot themselves
                return;
            }
            Debug.Log(hit.transform.name);
            Damageable target = hit.transform.GetComponent<Damageable>();
            if (target != null) 
            {
                Debug.Log("Target takes " + this.GetDamage() +" damage");
                target.TakeDamage(this.GetDamage());
            }

            ParticleSystem impact = Instantiate(hitPointSystem, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(impact, 2f);
        }
    }
}
