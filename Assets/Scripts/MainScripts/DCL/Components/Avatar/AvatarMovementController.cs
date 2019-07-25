﻿using UnityEngine;

namespace DCL
{
    [RequireComponent(typeof(AvatarShape))]
    public class AvatarMovementController : MonoBehaviour
    {
        const float SPEED_SLOW = 2.0f;
        const float SPEED_FAST = 4.0f;
        const float SPEED_ULTRA_FAST = 8.0f;
        const float SPEED_GRAVITY = 8.0f;
        const float ROTATION_SPEED = 6.25f;
        const float SPEED_EPSILON = 0.0001f;

        Transform avatarTransform
        {
            get
            {
                if (avatarTransformValue == null)
                    avatarTransformValue = GetComponent<AvatarShape>().entity.gameObject.transform;

                return avatarTransformValue;
            }
        }

        Transform avatarTransformValue;

        bool isInitialPosition = true;
        Vector3 currentPosition
        {
            get
            {
                return currentWorldPosition;
            }

            set
            {
                currentWorldPosition = value;
                avatarTransform.position = DCLCharacterController.i.characterPosition.WorldToUnityPosition(currentWorldPosition);
            }
        }

        Vector3 currentWorldPosition = Vector3.zero;
        Quaternion currentRotation
        {
            get { return avatarTransform.rotation; }
            set { avatarTransform.rotation = value; }
        }

        Vector3 targetPosition;
        Quaternion targetRotation;

        float movementSpeed = SPEED_SLOW;

        void OnEnable()
        {
            if (DCLCharacterController.i)
                DCLCharacterController.i.characterPosition.OnPrecisionAdjust += OnPrecisionAdjust;
        }

        void OnDisable()
        {
            if (DCLCharacterController.i)
                DCLCharacterController.i.characterPosition.OnPrecisionAdjust -= OnPrecisionAdjust;
        }

        void OnPrecisionAdjust(DCLCharacterPosition position)
        {
            avatarTransform.position = position.WorldToUnityPosition(currentWorldPosition);
        }

        public void MoveTo(Vector3 position, Quaternion rotation)
        {
            // Edge case on first initialization
            if (isInitialPosition)
            {
                currentPosition = position;
                avatarTransform.rotation = rotation;
                isInitialPosition = false;
            }

            Vector3 flatEulerRotation = rotation.eulerAngles;
            flatEulerRotation.z = flatEulerRotation.x = 0;
            rotation = Quaternion.Euler(flatEulerRotation);

            targetPosition = position;
            targetRotation = rotation;

            float distance = Vector3.Distance(targetPosition, currentWorldPosition);

            //NOTE(Brian): More distance to goal = faster movement.
            if (distance >= 50)
            {
                this.movementSpeed = float.MaxValue;
            }
            else if (distance >= 3)
            {
                this.movementSpeed = Mathf.Lerp(SPEED_SLOW, SPEED_ULTRA_FAST, (distance - 3) / 10.0f);
            }
            else
            {
                this.movementSpeed = SPEED_SLOW;
            }
        }


        void UpdateLerp(float deltaTime)
        {
            if (Vector3.SqrMagnitude(currentPosition - targetPosition) < SPEED_EPSILON)
            {
                UpdateRotation(deltaTime, targetRotation);
                return;
            }

            //NOTE(Brian): When we update movement we don't update rotation, because the Avatar will face the movement forward vector.
            UpdateMovement(deltaTime);
        }

        private void UpdateRotation(float deltaTime, Quaternion targetRotation)
        {
            currentRotation = Quaternion.Slerp(currentRotation, targetRotation, ROTATION_SPEED * deltaTime);
        }

        private void UpdateMovement(float deltaTime)
        {
            Vector3 flattenedDiff = targetPosition - currentPosition;
            flattenedDiff.y = 0;

            //NOTE(Brian): Avoid Unity error when computing look rotation for 0 magnitude vectors.
            //             Note that this isn't the same as the previous distance check because this 
            //             is computed with a flattened vector.
            if (flattenedDiff != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(flattenedDiff, Vector3.up);
                UpdateRotation(deltaTime, lookRotation);
            }

            Vector3 direction = (targetPosition - currentPosition).normalized;
            Vector3 delta = direction * movementSpeed * deltaTime;

            //NOTE(Brian): We need a separate value for Y movement because the gravity has to be lerped faster.
            delta.y = direction.y * SPEED_GRAVITY * deltaTime;

            //NOTE(Brian): If we overshoot targetPosition we adjust the delta value accordingly.
            if (delta.sqrMagnitude > Vector3.SqrMagnitude(targetPosition - currentPosition))
            {
                delta = targetPosition - currentPosition;
            }

            currentPosition += delta;
        }

        void Update()
        {
            UpdateLerp(Time.deltaTime);
        }
    }
}
