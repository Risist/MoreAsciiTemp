using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class CharacterUiJumpMarker : CharacterUiMarker
{
    public float minNormalDot;
    public float destinationOffset;

    public void InitJump(Rigidbody2D other)
    {
        other.simulated = false;
        other.velocity = Vector2.zero;
    }
    public void UpdateJump(Transform tr, Vector3 finalPosition, float movementSpeed)
    {
        tr.position = Vector3.Lerp(tr.position, finalPosition, movementSpeed);
    }
    public void OutJump(Rigidbody2D other)
    {
        other.simulated = true;
        //tr.position = finalPosition;
    }

    public Vector3 GetFinalPosition(Vector2 normal)
    {
        return transform.position + (Vector3)normal * destinationOffset;
    }

}

namespace Character
{
    public class CStateJumpOverWall : StateComponent
    {
        public CStateJumpOverWall(float maxAplicationPeriod = 0.8f, float movementSpeed = 0.05f, float rotationSpeed = 0.15f, float requiredRayDistance = 100.0f)
        {
            this.maxAplicationPeriod = maxAplicationPeriod;
            this.rotationSpeed = rotationSpeed;
            this.requiredRayDistance = requiredRayDistance;
            this.movementSpeed = movementSpeed;
        }
        public float maxAplicationPeriod;
        public float requiredRayDistance;

        public float rotationSpeed;
        public float movementSpeed;

        CharacterUiIndicator indicator;
        CharacterUiJumpMarker pretendingMarker;
        CharacterUiJumpMarker currentMarker;

        Vector2 hitNormal;

        Vector3 initPosition;
        Vector3 finalPosition;

        bool final;

        public override void Init()
        {
            indicator = stateController.GetComponentInChildren<CharacterUiIndicator>();
        }

        public override bool CanEnter()
        {
            if (indicator.environmentIndicators[0].use && indicator.environmentIndicators[0].hit.distance < requiredRayDistance)
            {
                pretendingMarker = indicator.environmentIndicators[0].hit.collider.GetComponent<CharacterUiJumpMarker>();
                hitNormal = indicator.environmentIndicators[0].hit.normal;
                return true;
            }
            return false;
        }

        public override void Enter()
        {
            currentMarker = pretendingMarker;
            currentMarker.InitJump(stateController.rigidbody);

            initPosition = stateController.transform.position;
            finalPosition = currentMarker.GetFinalPosition(hitNormal);

            Vector2 toFinal = -(initPosition - finalPosition).normalized;
            stateController.movement.ApplyExternalRotation(toFinal, rotationSpeed);
            final = false;
        }

        public override void Exit()
        {
            if (!final)
                currentMarker.OutJump(rigidbody);
        }

        public override void Update(float animationTime)
        {
            if (final)
                return;

            Vector2 toFinal = -(initPosition - finalPosition).normalized;
            movement.ApplyExternalRotation(toFinal, rotationSpeed);

            if (animationTime > maxAplicationPeriod)
            {
                final = true;
                currentMarker.OutJump(rigidbody);
            }else
            {
                currentMarker.UpdateJump(stateController.transform, finalPosition, movementSpeed );
            }
                
        }
    }
}