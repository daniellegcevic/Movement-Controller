using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    #region Variables

        #region Singleton

            public static CameraController instance = null;

        #endregion

        #region Settings

            [Header("Player Settings")]
            public bool breathing = true;
            public bool zoom = true;

            [Header("Camera Settings")]
            public Vector2 sensitivity;
            public Vector2 smoothness;
            public Vector2 lookAngle;

            [Header("Camera Zoom")]
            [Range(20f, 60f)] public float zoomFOV;
            public AnimationCurve zoomCurve;
            public float zoomTransitionDuration;

            [Header("Camera Breathing")]
            public float noiseFrequency;
            public float noiseAmplitude;

        #endregion

        #region DEBUG

            [HideInInspector] public float cameraYaw;
            [HideInInspector] public float cameraPitch;
            [HideInInspector] public float smoothCameraYaw;
            [HideInInspector] public float smoothCameraPitch;

            private float normalFOV;

            private Vector3 positionOffset;
            private Vector3 rotationOffset;
            private Vector3 finalRotation;
            private Vector3 finalPosition;
            private Vector3 noiseOffset;
            private Vector3 noise;

            [HideInInspector] public bool enableCameraMovement = false;
            [HideInInspector] public bool enableCameraBreathing = false;

        #endregion

        #region Components

            [HideInInspector] public Transform cameraHolderTransform;
            private Camera mainCamera;

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
            cameraHolderTransform = transform.GetChild(0).transform;
            mainCamera = Camera.main;

            normalFOV = mainCamera.fieldOfView;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            PerlinNoiseController();
        }

        private void LateUpdate()
        {
            if(enableCameraMovement)
            {
                CameraRotation();
                ZoomCheck();
            }

            if(enableCameraBreathing)
            {
                CameraBreathing();
            }
        }

    #endregion

    #region Custom Methods

        private void CameraRotation()
        {
            cameraYaw += InputHandler.instance.mouseInputVector.x * sensitivity.x * Time.deltaTime;
            cameraPitch -= InputHandler.instance.mouseInputVector.y * sensitivity.y * Time.deltaTime;

            cameraPitch = Mathf.Clamp(cameraPitch, lookAngle.x, lookAngle.y);

            smoothCameraYaw = Mathf.Lerp(smoothCameraYaw, cameraYaw, smoothness.x * Time.deltaTime);
            smoothCameraPitch = Mathf.Lerp(smoothCameraPitch, cameraPitch, smoothness.y * Time.deltaTime);

            transform.eulerAngles = new Vector3(0f, smoothCameraYaw, 0f);
            cameraHolderTransform.localEulerAngles = new Vector3(smoothCameraPitch, 0f, 0f);
        }

        private void ZoomCheck()
        {
            if(zoom)
            {
                if(InputHandler.instance.zoomClicked || InputHandler.instance.zoomReleased)
                {
                    if(ZoomFOV() != null)
                    {
                        StopCoroutine(ZoomFOV());
                    }

                    StartCoroutine(ZoomFOV());
                } 
            } 
        }

        private void CameraBreathing()
        {
            if(breathing)
            {
                UpdateNoise();

                positionOffset = Vector3.zero;
                rotationOffset = Vector3.zero;

                rotationOffset.x += noise.x;
                finalRotation.x = rotationOffset.x;

                rotationOffset.y += noise.y;
                finalRotation.y = rotationOffset.y;

                finalRotation.z = transform.localEulerAngles.z;

                mainCamera.transform.localEulerAngles = finalRotation;
            }
        }

        private void PerlinNoiseController()
        {
            float maximum = 32f;

            noiseOffset.x = Random.Range(0f, maximum);
            noiseOffset.y = Random.Range(0f, maximum);
            noiseOffset.z = Random.Range(0f, maximum);
        }

        private void UpdateNoise()
        {
            float frequencyOffset = Time.deltaTime * noiseFrequency;

            noiseOffset.x += frequencyOffset;
            noiseOffset.y += frequencyOffset;
            noiseOffset.z += frequencyOffset;

            noise.x = Mathf.PerlinNoise(noiseOffset.x, 0f);
            noise.y = Mathf.PerlinNoise(noiseOffset.x, 1f);
            noise.z = Mathf.PerlinNoise(noiseOffset.x, 2f);

            noise -= Vector3.one * 0.5f;
            noise *= noiseAmplitude;
        }

    #endregion

    #region Coroutines

        private IEnumerator ZoomFOV()
        {
            float zoomTransitionPercentage = 0f;
            float smoothZoomTransitionPercentage = 0f;

            float zoomTransitionSpeed = 1f / zoomTransitionDuration;

            float currentFOV = mainCamera.fieldOfView;
            float targetFOV;

            if(InputHandler.instance.isZoomedIn)
            {
                targetFOV = normalFOV;
            }
            else
            {
                targetFOV = zoomFOV;
            }

            InputHandler.instance.isZoomedIn = !InputHandler.instance.isZoomedIn;

            while(zoomTransitionPercentage < 1f)
            {
                zoomTransitionPercentage += Time.deltaTime * zoomTransitionSpeed;
                smoothZoomTransitionPercentage = zoomCurve.Evaluate(zoomTransitionPercentage);
                mainCamera.fieldOfView = Mathf.Lerp(currentFOV, targetFOV, smoothZoomTransitionPercentage);
                yield return null;
            }
        }

    #endregion
}