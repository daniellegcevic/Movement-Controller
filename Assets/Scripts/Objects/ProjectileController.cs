using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    #region Variables
    
        #region Singleton

            public static ProjectileController instance = null;

        #endregion
        
        #region Settings
        
            public Rigidbody projectile;
            public float speed;
            public float raycastRange;
        
        #endregion

        #region DEBUG

            [HideInInspector] public bool enableShooting = false;

        #endregion
    
        #region Components
        
            private Rigidbody rb;
            private Transform defaultTarget;
            private MovementController movementController;
            private Camera mainCamera;
            private RaycastHit hit;
        
        #endregion
    
    #endregion
    
    #region Built-in Methods
        
        private void Awake()
        {
            if(instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            movementController = MovementController.instance;
            defaultTarget = gameObject.transform.GetChild(0);
            mainCamera = Camera.main;
        }
        
        private void Update()
        {
            Physics.Raycast(mainCamera.transform.position, mainCamera.transform.TransformDirection(Vector3.forward), out hit, raycastRange);

            if(hit.transform)
            {
                Debug.DrawRay(mainCamera.transform.position, mainCamera.transform.TransformDirection(Vector3.forward) * hit.distance, Color.green);
            }
            else
            {
                Debug.DrawRay(mainCamera.transform.position, mainCamera.transform.TransformDirection(Vector3.forward) * raycastRange, Color.red);
            }

            if(enableShooting)
            {
                if(InputHandler.instance.leftMouseClicked)
                {
                    if(!rb) // Throw Projectile
                    {
                        rb = Instantiate(projectile, transform.position, transform.rotation);

                        if(hit.transform)
                        {
                            rb.velocity = Vector3.Normalize(hit.point - transform.position) * speed + movementController.finalMovementVector;
                        }
                        else
                        {
                            rb.velocity = Vector3.Normalize(defaultTarget.position - transform.position) * speed + movementController.finalMovementVector;
                        }
                    }
                }
                else if(InputHandler.instance.rightMouseClicked)
                {
                    if(rb) // Teleport
                    {
                        rb.GetComponent<Projectile>().Teleport();
                    }
                }
                else if(InputHandler.instance.destroyClicked)
                {
                    if(rb) // Destroy Projectile
                    {
                        rb.GetComponent<Projectile>().Disintegrate();
                    }
                }

                if(hit.transform)
                {
                    Debug.DrawRay(transform.position, hit.point - transform.position, Color.gray);
                }
                else
                {
                    Debug.DrawRay(transform.position, defaultTarget.position - transform.position, Color.gray);
                }
            }
        }

        private void OnDrawGizmos()
        {
            if(hit.transform)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(hit.point, 0.1f);
            }
        }
    
    #endregion
}