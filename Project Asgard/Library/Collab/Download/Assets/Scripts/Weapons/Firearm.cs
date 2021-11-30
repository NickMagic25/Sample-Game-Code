using UnityEngine;

public abstract class Firearm : MonoBehaviour
{

    [SerializeField] private Camera fpsCam;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float range = 100f;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("Fire1")) 
        {
            Shoot();
        }
    }

    void Shoot() 
    {
        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit))
        {
            Debug.Log(hit.transform.name);
            Damageable target=hit.transform.GetComponent<Damageable>();
            if (target != null) { }
        }
    
    }
}
