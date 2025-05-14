using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // Player speed
    private Animator animator;   // Reference to Animator

    private void Start()
    {
        animator = GetComponent<Animator>(); // Get Animator from the player
    }

    private void Update()
    {
        // Get input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 movement = new Vector3(horizontal, 0f, vertical);

        // Check if player is pressing movement keys
        bool isMoving = movement.sqrMagnitude > 0f;

        // Apply movement
        if (isMoving)
        {
            movement = movement.normalized * moveSpeed * Time.deltaTime;
            transform.Translate(movement, Space.World);
        }

        // Update animation
        if (animator != null)
        {
            animator.SetBool("isMoving", isMoving);
        }
    }
}
