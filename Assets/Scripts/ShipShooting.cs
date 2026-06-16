using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShipShooting : MonoBehaviour
{
    [SerializeField] ParticleSystem laserVFX;
    [SerializeField] float fireRate = 0.2f;
    [SerializeField] float range = 1000f;
    [SerializeField] Transform cameraTransform;
    [SerializeField] GameObject explosionVFX;

    float nextFireTime;
    bool isShooting;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    
    // Update is called once per frame
    void Update()
    {
        if (!isShooting)
        {
            nextFireTime = Time.time;
            return;
        }
        if(Time.time >= nextFireTime)
        {
        Shoot();
        nextFireTime = Time.time + fireRate;    
        }
    }
    void Shoot()
    {
        laserVFX.Play();

        Transform shootPoint = laserVFX.transform;

        Vector3 targetPoint;

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        if(Physics.Raycast(ray, out RaycastHit hit, range))
        {
            Debug.Log("Hit: " + hit.collider.name);
            
            GameObject vfx = Instantiate(explosionVFX, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(vfx, 1f);
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.origin + ray.direction * range;
        }
        Vector3 shootDirection = (targetPoint - shootPoint.position).normalized;
        Debug.DrawRay(shootPoint.position, shootDirection*100f, Color.red, 1f);
    }
    public void OnShoot(InputAction.CallbackContext context)
    {
        isShooting = context.ReadValueAsButton();
        if (context.started)
        {
            isShooting = true;
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
        if (context.canceled)
        {
            isShooting = false;
            nextFireTime = float.MaxValue;
        }
    }
    
}
