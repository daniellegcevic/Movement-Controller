using System.Collections;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    #region Variables

        #region Singleton

            public static MovementController instance = null;

        #endregion

        #region Settings

            [Header("Player Settings")]
            public bool run = false;
            public bool crouch = false;
            public bool jump = false;
            public bool gravity = true;
            public bool noclip = false;

            [Header("Movement Settings")]
            public float crouchSpeed = 1f;
            public float walkSpeed = 3f;
            public float runSpeed = 6f;
            public float jumpSpeed = 6f;
            [Range(0f, 1f)] public float backwardsSpeedPercent = 0.5f;
            [Range(0f, 1f)] public float sideSpeedPercent = 0.7f;

            [Header("Run Settings")]
            public AnimationCurve runTransitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            [Range(-1f, 1f)] public float runAbilityThreshold = 0.7f;

            [Header("Crouch Settings")]
            public AnimationCurve crouchTransitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            [Range(0.2f, 0.9f)] public float crouchPercent = 0.6f;
            public float crouchTransitionDuration = 0.5f;

            [Header("Jump Settings")]
            public int numberOfJumps = 1;
            [Range(0.05f, 0.5f)] public float lowImpactForce = 0.1f;
            [Range(0.2f, 0.9f)] public float highImpactForce = 0.6f;
            public float highImpactTime = 0.5f;
            public float landDuration = 1f;
            public AnimationCurve landTransitionCurve = AnimationCurve.EaseInOut(0f,0f,1f,1f);

            [Header("Gravity Settings")]
            public LayerMask groundLayers;
            [Range(0f, 1f)] public float groundRayLength = 0.1f;
            [Range(0.01f, 1f)] public float groundSphereRadius = 0.2f;                              
            public float groundedGravityForce = 1f;
            public float gravityMultiplier = 2.5f;

            [Header("Wall Check Settings")]
            public LayerMask obstacleLayers;
            [Range(0f, 1f)] public float obstacleRayLength = 0.4f;
            [Range(0.01f, 1f)] public float obstacleSphereRadius = 0.2f;

            [Header("Smooth Movement Settings")]               
            [Range(1f, 100f)] public float rotationSpeed = 10f;
            [Range(1f, 100f)] public float movementInputSpeed = 10f;
            [Range(1f, 100f)] public float velocitySpeed = 3f;
            [Range(1f, 100f)] public float directionSpeed = 10f;
            [Range(1f, 100f)] public float headBobSpeed = 5f;

            [Header("Head Bob Settings")]
            public AnimationCurve xCurve;
            public AnimationCurve yCurve;
            public float xAmplitude;
            public float yAmplitude;
            public float xFrequency;
            public float yFrequency;
            public float runAmplitudeMultiplier;
            public float runFrequencyMultiplier;
            public float crouchAmplitudeMultiplier;
            public float crouchFrequencyMultiplier;

        #endregion

        #region DEBUG

            private Vector2 normalizedMovementInputVector;
            [HideInInspector] public Vector2 smoothMovementInputVector;

            private Vector3 movementDirection;
            [HideInInspector] public Vector3 smoothMovementDirection;
            [HideInInspector] public Vector3 finalMovementVector;

            private float movementSpeed;
            [HideInInspector] public float smoothMovementSpeed;
            private float finalSmoothMovementSpeed;
            private float speedDifference;

            private float finalRayLength;
            private bool isHittingWall;
            private bool currentlyOnGround;
            private bool previouslyOnGround;

            private float playerHeight;
            private float crouchingHeight;
            private Vector3 playerCenter;
            private Vector3 crouchingCenter;

            private float cameraHeight;
            private float crouchingCameraHeight;
            private float heightDifference;
            private bool crouchAnimation;
            private bool runAnimation;

            private float backwardsMovementFrequencyMultiplier;
            private float sideMovementFrequencyMultiplier;

            private float xHeadBobFrequency;
            private float yHeadBobFrequency;

            private bool headBobHasBeenReset;
            private Vector3 finalHeadBobOffset;
            private float currentHeadBobHeight = 0f;

            private float inAirTimer = 0f;
            private int jumpCounter = 0;

            [HideInInspector] public bool enablePlayerMovement = false;

        #endregion

        #region Components

            private CharacterController characterController;
            private CameraController cameraController;

            private RaycastHit groundHit;
            private RaycastHit wallHit;
            private RaycastHit ceilingHit;

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
            InputHandler.instance.enableRunning = run;
            InputHandler.instance.enableCrouching = crouch;
            InputHandler.instance.enableJumping = jump;

            NoclipCheck();

            characterController = GetComponent<CharacterController>();
            cameraController = CameraController.instance;

            characterController.center = new Vector3(0f, characterController.height / 2f + characterController.skinWidth, 0f);

            playerCenter = characterController.center;
            playerHeight = characterController.height;

            crouchingHeight = playerHeight * crouchPercent;
            crouchingCenter = (crouchingHeight / 2f + characterController.skinWidth) * Vector3.up;

            heightDifference = playerHeight - crouchingHeight;

            cameraHeight = cameraController.transform.localPosition.y;
            crouchingCameraHeight = cameraHeight - heightDifference;

            finalRayLength = groundRayLength + characterController.center.y;

            currentlyOnGround = true;
            previouslyOnGround = true;

            currentHeadBobHeight = cameraHeight;

            speedDifference = runSpeed - walkSpeed;

            HeadBobSetup();
        }

        private void Update()
        {
            if(enablePlayerMovement)
            {
                if(characterController && cameraController)
                {
                    InputHandler.instance.MovementInputCheck();

                    /*/ 01 /*/ RotateToCamera();
                    /*/ 02 /*/ GroundCheck();
                    /*/ 03 /*/ WallCheck();

                    /*/ 04 /*/ SmoothMovementInput();
                    /*/ 05 /*/ MovementDirection();
                    /*/ 06 /*/ MovementSpeed();

                    /*/ 07 /*/ SmoothMovementDirection();
                    /*/ 08 /*/ SmoothMovementSpeed();
                    /*/ 09 /*/ CalculateMovement();

                    /*/ 10 /*/ CrouchCheck();
                    /*/ 11 /*/ HeadBobCheck();
                    /*/ 12 /*/ LandCheck();

                    /*/ 13 /*/ Gravity();
                    /*/ 14 /*/ JumpCheck();
                    /*/ 15 /*/ MovePlayer();

                    previouslyOnGround = currentlyOnGround;
                } 
            }
        }

    #endregion

    #region Custom Methods

        private void NoclipCheck()
        {
            if(noclip)
            {
                Physics.IgnoreLayerCollision(0, 2, true);
            }
        }

        private void RotateToCamera() /*/ 01 /*/
        {
            Quaternion currentPlayerRotation = transform.rotation;
            Quaternion newPlayerRotation = cameraController.transform.rotation;

            transform.rotation = Quaternion.Slerp(currentPlayerRotation, newPlayerRotation, Time.deltaTime * rotationSpeed);
        }

        private void GroundCheck() /*/ 02 /*/
        {
            Vector3 sphereOrigin = transform.position + characterController.center;
            currentlyOnGround = Physics.SphereCast(sphereOrigin, groundSphereRadius, Vector3.down, out groundHit, finalRayLength, groundLayers);
            Debug.DrawRay(sphereOrigin, Vector3.down * (finalRayLength), Color.green);
        }

        private void WallCheck() /*/ 03 /*/
        {
            Vector3 lowerSphereOrigin = new Vector3(0, 0.5f, 0) + transform.position;
            Vector3 middleSphereOrigin = transform.position + characterController.center;
            Vector3 upperSphereOrigin = new Vector3(0, 1.6f, 0) + transform.position;

            bool lowerWallHit = false;
            bool middleWallHit = false;
            bool upperWallHit = false;

            if(InputHandler.instance.hasMovementInput && movementDirection.sqrMagnitude > 0)
            {
                lowerWallHit = Physics.SphereCast(lowerSphereOrigin, obstacleSphereRadius, movementDirection, out wallHit, obstacleRayLength, obstacleLayers);
                middleWallHit = Physics.SphereCast(middleSphereOrigin, obstacleSphereRadius, movementDirection, out wallHit, obstacleRayLength, obstacleLayers);
                
                if(!InputHandler.instance.isCrouching)
                {
                    upperWallHit = Physics.SphereCast(upperSphereOrigin, obstacleSphereRadius, movementDirection, out wallHit, obstacleRayLength, obstacleLayers);
                }
            }

            Debug.DrawRay(lowerSphereOrigin, movementDirection * obstacleRayLength, Color.green);
            Debug.DrawRay(middleSphereOrigin, movementDirection * obstacleRayLength, Color.green);
            
            if(!InputHandler.instance.isCrouching)
            {
                Debug.DrawRay(upperSphereOrigin, movementDirection * obstacleRayLength, Color.green);
            }

            if(middleWallHit || lowerWallHit || upperWallHit)
            {
                isHittingWall = true;
            }
            else
            {
                isHittingWall = false;
            }
        }

        private void SmoothMovementInput() /*/ 04 /*/
        {
            normalizedMovementInputVector = InputHandler.instance.movementInputVector.normalized;
            smoothMovementInputVector = Vector2.Lerp(smoothMovementInputVector, normalizedMovementInputVector, Time.deltaTime * movementInputSpeed);
        }

        private void MovementDirection() /*/ 05 /*/
        {
            Vector3 verticalDirection = transform.forward * smoothMovementInputVector.y;
            Vector3 horizontalDirection = transform.right * smoothMovementInputVector.x;

            Vector3 desiredDirection = verticalDirection + horizontalDirection;
            Vector3 flattenDirection = FlattenVectorOnSlopes(desiredDirection);

            movementDirection = flattenDirection;
        }

        private Vector3 FlattenVectorOnSlopes(Vector3 desiredDirection)
        {
            if(currentlyOnGround)
            {
                desiredDirection = Vector3.ProjectOnPlane(desiredDirection, groundHit.normal);
            }

            return desiredDirection;
        }

        private void MovementSpeed() /*/ 06 /*/
        {
            if(InputHandler.instance.isRunning && PlayerCanRun())
            {
                movementSpeed = runSpeed;
            }
            else
            {
                movementSpeed = walkSpeed;
            }

            if(InputHandler.instance.isCrouching)
            {
                movementSpeed = crouchSpeed;
            }

            if(!InputHandler.instance.hasMovementInput)
            {
                movementSpeed = 0f;
            }

            if(InputHandler.instance.movementInputVector.y == -1)
            {
                movementSpeed = movementSpeed * backwardsSpeedPercent;
            }

            if(InputHandler.instance.movementInputVector.x != 0 && InputHandler.instance.movementInputVector.y ==  0)
            {
                movementSpeed = movementSpeed * sideSpeedPercent;
            }
        }

        private bool PlayerCanRun()
        {
            Vector3 normalizedDirection = Vector3.zero;

            if(smoothMovementDirection != Vector3.zero)
            {
                normalizedDirection = smoothMovementDirection.normalized;
            }

            float dotProduct = Vector3.Dot(transform.forward, normalizedDirection);

            if(dotProduct >= runAbilityThreshold && !InputHandler.instance.isCrouching)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void SmoothMovementDirection() /*/ 07 /*/
        {
            smoothMovementDirection = Vector3.Lerp(smoothMovementDirection, movementDirection, Time.deltaTime * directionSpeed);
            Debug.DrawRay(transform.position, smoothMovementDirection, Color.gray);
        }

        private void SmoothMovementSpeed() /*/ 08 /*/
        {
            smoothMovementSpeed = Mathf.Lerp(smoothMovementSpeed, movementSpeed, Time.deltaTime * velocitySpeed);

            if(InputHandler.instance.isRunning && PlayerCanRun())
            {
                float transitionPercentage = Mathf.InverseLerp(walkSpeed, runSpeed, smoothMovementSpeed);
                finalSmoothMovementSpeed = runTransitionCurve.Evaluate(transitionPercentage) * speedDifference + walkSpeed;
            }
            else
            {
                finalSmoothMovementSpeed = smoothMovementSpeed;
            }
        }

        private void CalculateMovement() /*/ 09 /*/
        {
            float smoothMovementInputVectorMagnitude = 1f;

            Vector3 finalVector = smoothMovementDirection * finalSmoothMovementSpeed * smoothMovementInputVectorMagnitude;

            finalMovementVector.x = finalVector.x;
            finalMovementVector.z = finalVector.z;

            if(gravity)
            {
                if(characterController.isGrounded)
                {
                    finalMovementVector.y += finalVector.y;
                }  
            }
        }

        private void CrouchCheck() /*/ 10 /*/
        {
            if(InputHandler.instance.crouchClicked && currentlyOnGround)
            {
                if(InputHandler.instance.isCrouching)
                {
                    if(CeilingCheck())
                    {
                        return;
                    }
                }

                if(Crouch() != null)
                {
                    StopCoroutine(Crouch());
                }

                StartCoroutine(Crouch());
            }
        }

        private bool CeilingCheck()
        {
            Vector3 origin = transform.position;
            float ceilingCheckRadius = characterController.radius;

            bool isUnderCeiling = Physics.SphereCast(origin, ceilingCheckRadius, Vector3.up, out ceilingHit, playerHeight);

            return isUnderCeiling;
        }

        private void HeadBobCheck() /*/ 11 /*/
        {
            if(InputHandler.instance.hasMovementInput && currentlyOnGround && !isHittingWall)
            {
                if(!crouchAnimation)
                {
                    HeadBob();
                }
            }
            else
            {
                if(!headBobHasBeenReset)
                {
                    ResetHeadBob();
                }

                if(!crouchAnimation)
                {
                    cameraController.transform.localPosition = Vector3.Lerp(cameraController.transform.localPosition, new Vector3(0f, currentHeadBobHeight, 0f), Time.deltaTime * headBobSpeed);
                }
            }
        }

        public void HeadBobSetup()
        {
            backwardsMovementFrequencyMultiplier = backwardsSpeedPercent;
            sideMovementFrequencyMultiplier = sideSpeedPercent;

            xHeadBobFrequency = 0f;
            yHeadBobFrequency = 0f;

            headBobHasBeenReset = false;
            finalHeadBobOffset = Vector3.zero;
        }

        public void HeadBob()
        {
            headBobHasBeenReset = false;

            float amplitudeMultiplier;
            float frequencyMultiplier;
            float additionalMultiplier;

            if(InputHandler.instance.isRunning && PlayerCanRun())
            {
                amplitudeMultiplier = runAmplitudeMultiplier;
            }
            else
            {
                amplitudeMultiplier = 1f;
            }

            if(InputHandler.instance.isCrouching)
            {
                amplitudeMultiplier = crouchAmplitudeMultiplier;
            }

            if(InputHandler.instance.isRunning && PlayerCanRun())
            {
                frequencyMultiplier = runFrequencyMultiplier;
            }
            else
            {
                frequencyMultiplier = 1f;
            }

            if(InputHandler.instance.isCrouching)
            {
                frequencyMultiplier = crouchFrequencyMultiplier;
            }

            if(InputHandler.instance.movementInputVector.y == -1)
            {
                additionalMultiplier = backwardsMovementFrequencyMultiplier;
            }
            else
            {
                additionalMultiplier = 1f;
            }

            if(InputHandler.instance.movementInputVector.x != 0 && InputHandler.instance.movementInputVector.y == 0)
            {
                additionalMultiplier = sideMovementFrequencyMultiplier;
            }

            xHeadBobFrequency += Time.deltaTime * xFrequency * frequencyMultiplier;
            yHeadBobFrequency += Time.deltaTime * yFrequency * frequencyMultiplier;

            float xValue;
            float yValue;

            xValue = xCurve.Evaluate(xHeadBobFrequency);
            yValue = yCurve.Evaluate(yHeadBobFrequency);

            finalHeadBobOffset.x = xValue * xAmplitude * amplitudeMultiplier * additionalMultiplier;
            finalHeadBobOffset.y = yValue * yAmplitude * amplitudeMultiplier * additionalMultiplier;

            cameraController.transform.localPosition = Vector3.Lerp(cameraController.transform.localPosition, (Vector3.up * currentHeadBobHeight) + finalHeadBobOffset, Time.deltaTime * headBobSpeed);
        }

        public void ResetHeadBob()
        {
            xHeadBobFrequency = 0f;
            yHeadBobFrequency = 0f;

            finalHeadBobOffset = Vector3.zero;

            headBobHasBeenReset = true;
        }

        private void LandCheck() /*/ 12 /*/
        {
            if(!previouslyOnGround && currentlyOnGround)
            {
                if(Land() != null)
                {
                    StopCoroutine(Land());
                }

                StartCoroutine(Land());
            }
        }

        private void JumpCheck() /*/ 13 /*/
        {
            if(jump)
            {
                if(numberOfJumps == 1)
                {
                    if(characterController.isGrounded)
                    {
                        if(InputHandler.instance.jumpClicked && !InputHandler.instance.isCrouching)
                        {
                            InputHandler.instance.enableJumping = false;
                            InputHandler.instance.enableCrouching = false;

                            finalMovementVector.y = jumpSpeed;
                        }
                    }
                }
                else if(numberOfJumps > jumpCounter)
                {
                    if(InputHandler.instance.jumpClicked && !InputHandler.instance.isCrouching)
                    {
                        InputHandler.instance.enableCrouching = false;

                        finalMovementVector.y = jumpSpeed;

                        jumpCounter++;
                    }
                }
            }
        }

        private void Gravity() /*/ 14 /*/
        {
            if(gravity)
            {
                if(characterController.isGrounded)
                {
                    inAirTimer = 0f;
                    finalMovementVector.y = -groundedGravityForce;
                }
                else
                {
                    inAirTimer += Time.deltaTime;
                    finalMovementVector += Physics.gravity * gravityMultiplier * Time.deltaTime;
                }
            }
        }

        private void MovePlayer() /*/ 15 /*/
        {
            characterController.Move(finalMovementVector * Time.deltaTime);
        }

    #endregion

    #region Coroutines

        private IEnumerator Crouch()
        {
            InputHandler.instance.enableJumping = false;
            InputHandler.instance.enableCrouching = false;

            crouchAnimation = true;

            float crouchTransitionPercentage = 0f;
            float smoothCrouchTransitionPercentage = 0f;
            float crouchSpeed = 1f / crouchTransitionDuration;

            float currentHeight = characterController.height;
            Vector3 currentCenter = characterController.center;

            float desiredHeight;
            Vector3 desiredCenter;
            float desiredCameraHeight;

            Vector3 cameraControllerPosition = cameraController.transform.localPosition;
            float currentCameraHeight = cameraControllerPosition.y;

            InputHandler.instance.isCrouching = !InputHandler.instance.isCrouching;

            if(InputHandler.instance.isCrouching)
            {
                desiredHeight = crouchingHeight;
                desiredCenter = crouchingCenter;
                desiredCameraHeight = crouchingCameraHeight;

                currentHeadBobHeight = crouchingCameraHeight;
            }
            else
            {
                desiredHeight = playerHeight;
                desiredCenter = playerCenter;
                desiredCameraHeight = cameraHeight;

                currentHeadBobHeight = cameraHeight;
            }

            while(crouchTransitionPercentage < 1f)
            {
                crouchTransitionPercentage += Time.deltaTime * crouchSpeed;
                smoothCrouchTransitionPercentage = crouchTransitionCurve.Evaluate(crouchTransitionPercentage);

                characterController.height = Mathf.Lerp(currentHeight, desiredHeight, smoothCrouchTransitionPercentage);
                characterController.center = Vector3.Lerp(currentCenter, desiredCenter, smoothCrouchTransitionPercentage);

                cameraControllerPosition.y = Mathf.Lerp(currentCameraHeight, desiredCameraHeight, smoothCrouchTransitionPercentage);
                cameraController.transform.localPosition = cameraControllerPosition;

                yield return null;
            }

            if(!InputHandler.instance.isCrouching)
            {
                InputHandler.instance.enableJumping = true;
            }

            InputHandler.instance.enableCrouching = true;

            crouchAnimation = false;
        }

        private IEnumerator Land()
        {
            float landTransitionPercentage = 0f;
            float impactForce = 0f;

            float landSpeed = 1f / landDuration;

            Vector3 localPosition = cameraController.transform.localPosition;
            float initialLandHeight = localPosition.y;

            impactForce = inAirTimer >= highImpactTime ? highImpactForce : lowImpactForce;

            while(landTransitionPercentage < 1f)
            {
                landTransitionPercentage += Time.deltaTime * landSpeed;
                float desiredHeight = landTransitionCurve.Evaluate(landTransitionPercentage) * impactForce;

                localPosition.y = initialLandHeight + desiredHeight;
                cameraController.transform.localPosition = localPosition;

                yield return null;
            }

            jumpCounter = 0;

            InputHandler.instance.enableJumping = true;
            InputHandler.instance.enableCrouching = true;
        }

    #endregion
}