using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    #region Variables

        #region Settings
        
            public float radius;
        
        #endregion

        #region DEBUG
        
            [HideInInspector] public bool enableTeleportation = true;
        
        #endregion
        
        #region Components
        
            private MovementController movementController;
            private Rigidbody rb;
        
        #endregion
    
    #endregion
    
    #region Built-in Methods

        private void Start()
        {
            movementController = MovementController.instance;
            rb = GetComponent<Rigidbody>();
        }

        private void OnTriggerEnter(Collider collider)
        {
            if(collider.gameObject.tag == "No Teleportation")
            {
                enableTeleportation = false;
            }
        }

        private void OnTriggerExit(Collider collider)
        {
            if(collider.gameObject.tag == "No Teleportation")
            {
                enableTeleportation = true;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if(collision.gameObject.tag == "Bouncy")
            {
                rb.velocity *= 2f;
            }
            else if(collision.gameObject.tag == "Explosive")
            {
                Explode();
            }
        }

    #endregion

    #region Custom Methods
    
        public void Teleport()
        {
            if(enableTeleportation)
            {
                StartCoroutine(TeleportPlayer());
            }
        }
        
        public void Disintegrate()
        {
            Destroy(gameObject);
        }

        public void Explode()
        {
            Destroy(gameObject);
        }
    
    #endregion

    #region Coroutines
    
        private IEnumerator TeleportPlayer()
        {
            movementController.enablePlayerMovement = false;
            yield return new WaitForSeconds(0.05f);

            movementController.gameObject.transform.position = new Vector3(transform.position.x, transform.position.y - radius, transform.position.z);
            yield return new WaitForSeconds(0.05f);

            movementController.enablePlayerMovement = true;
            Destroy(gameObject);
        }
    
    #endregion
}