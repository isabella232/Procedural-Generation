using Assets.Gamelogic.Visualizers;
using Improbable.Math;
using Improbable.Player.Controls;
using Improbable.Player.Physical;
using Improbable.Unity.Common.Core.Math;
using Improbable.Unity.Visualizer;
using UnityEngine;

namespace Gamelogic.Visualizers
{
    public class PlayerMovementBehaviour : MonoBehaviour
    {
        [Require]
        protected PlayerControlsReader playerControlsReader;
        [Require]
        protected ForcesReader forcesReader;
        private Rigidbody cachedRigidbody;
        private Vector3 movementDirection;

        protected virtual void Awake()
        {
            cachedRigidbody = GetComponent<Rigidbody>();
        }

        protected virtual void OnEnable()
        {
            playerControlsReader.MovementDirectionUpdated += SetMovementDirection;
            // Generate tiles for initial position
            TileManager.GenerateAroundPosition(transform.position);
        }

        protected virtual void OnDisable()
        {
            playerControlsReader.MovementDirectionUpdated -= SetMovementDirection;
        }

        protected void SetMovementDirection(Vector3d direction)
        {
            movementDirection = direction.ToUnityVector();
        }

        protected virtual void FixedUpdate()
        {
            transform.Translate(movementDirection * forcesReader.ForceMagnitude * Time.deltaTime);
            if (!movementDirection.Equals(Vector3.zero))
            {
                TileManager.GenerateAroundPosition(transform.position);
            }
        }
    }
}
