using UnityEngine;

public class DebugMenu : MonoBehaviour
{
    #region Variables

        [Header("References")]
        [SerializeField] private GameObject debugMenu;
        [SerializeField] private GameObject projectileSpeed;
    
    #endregion
    
    #region Built-in Methods
    
        private void Start()
        {
            UpdateDebugMenu();
        }
    
        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.M))
            {
                debugMenu.SetActive(!debugMenu.activeInHierarchy);
            }
            else if(Input.GetKeyDown(KeyCode.Period))
            {
                ProjectileController.instance.speed += 1f;
                UpdateDebugMenu();
            }
            else if(Input.GetKeyDown(KeyCode.Comma))
            {
                ProjectileController.instance.speed -= 1f;
                UpdateDebugMenu();
            }
        }
    
    #endregion
    
    #region Custom Methods
    
        private void UpdateDebugMenu()
        {
            projectileSpeed.GetComponent<TMPro.TextMeshProUGUI>().text = "Projectile Speed: " + ProjectileController.instance.speed + " m/s";
        }
    
    #endregion
}