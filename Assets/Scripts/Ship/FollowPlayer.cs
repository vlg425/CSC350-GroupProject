    using UnityEngine;

    public class CameraFollowPlayer : MonoBehaviour
    {
        public Transform player; // Assign your player's Transform in the Inspector
        public Vector3 offset; // Adjust the camera's offset from the player

        void LateUpdate()
        {
            // Update the camera's position to follow the player with the defined offset
            transform.position = player.position + offset;
        }
    }