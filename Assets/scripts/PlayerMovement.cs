using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;      // Player speed
    private Animator animator;        // Reference to Animator

    private void Start()
    {
        animator = GetComponent<Animator>(); // Get Animator from the player
    }

    private void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontal, 0f, vertical);

        // Move the player
        movement = movement.normalized * moveSpeed * Time.deltaTime;
        transform.Translate(movement, Space.World);

        // Play animation
        if (animator != null)
        {
            bool isMoving = movement.magnitude > 0;
            animator.SetBool("isMoving", isMoving);
        }
    }
}
