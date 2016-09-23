using Improbable.Player;
using Improbable.Player.Controls;
using Improbable.Unity.Visualizer;
using UnityEngine;
using Vector3 = Improbable.Math.Vector3d;

namespace Assets.Gamelogic.Visualizers
{
    public class PlayerControlsVisualizer : MonoBehaviour
    {

        [Require]
        protected LocalPlayerCheckWriter LocalPlayerCheck;
        [Require]
        protected PlayerControlsWriter PlayerControls;

        public void Update()
        {
            PlayerControls.Update.MovementDirection(GetMovementDirection()).FinishAndSend();
        }

        private Vector3 GetMovementDirection()
        {
            return new Vector3(UnityEngine.Input.GetAxis("Horizontal"), 0, UnityEngine.Input.GetAxis("Vertical"));
        }
    }
}