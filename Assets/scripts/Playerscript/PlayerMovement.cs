using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	public Animator playerAnim;
	public Rigidbody playerRigid;
	public float w_speed, wb_speed, olw_speed, rn_speed, ro_speed;
	public bool walking;
	public Transform playerTrans;
	private bool isMovingBackward;


	public bool isSlashing = false;
	public float slashCooldown = 1f;
	private float slashTimer = 0f;



	public GameObject weaponHitboxObj; // Assign in Inspector
	private WeaponHitbox weaponHitbox;

	void Awake()
	{
		weaponHitbox = weaponHitboxObj.GetComponent<WeaponHitbox>();
	}


	void FixedUpdate()
	{
		if (Input.GetKey(KeyCode.W))
		{
			playerRigid.velocity = transform.forward * w_speed * Time.deltaTime;
		}
		else if (Input.GetKey(KeyCode.S))
		{
			playerRigid.velocity = -transform.forward * wb_speed * Time.deltaTime;
		}
		else
		{
			playerRigid.velocity = Vector3.zero;
		}
	}

	void Update()
	{
		// Handle slash cooldown timer
		if (isSlashing)
		{
			slashTimer -= Time.deltaTime;
			if (slashTimer <= 0f)
			{
				ResetSlash();
			}
		}

		// Slash input (Left Mouse Button)
		if (Input.GetMouseButtonDown(0) && !isSlashing)
		{
			Slash();
		}


		if (Input.GetKeyDown(KeyCode.W))
		{
			playerAnim.SetTrigger("walk");
			playerAnim.ResetTrigger("idle");
			walking = true;
			isMovingBackward = false;
		}
		if (Input.GetKeyUp(KeyCode.W))
		{
			playerAnim.ResetTrigger("walk");
			playerAnim.SetTrigger("idle");
			walking = false;
		}

		if (Input.GetKeyDown(KeyCode.S))
		{
			playerAnim.SetTrigger("walkback");
			playerAnim.ResetTrigger("idle");
			isMovingBackward = true;
			walking = false;
		}
		if (Input.GetKeyUp(KeyCode.S))
		{
			playerAnim.ResetTrigger("walkback");
			playerAnim.SetTrigger("idle");
			isMovingBackward = false;
		}

		if (Input.GetKey(KeyCode.A))
		{
			playerTrans.Rotate(0, -ro_speed * Time.deltaTime, 0);
		}
		if (Input.GetKey(KeyCode.D))
		{
			playerTrans.Rotate(0, ro_speed * Time.deltaTime, 0);
		}
		if (walking == true)
		{
			if (Input.GetKeyDown(KeyCode.LeftShift))
			{
				//steps1.SetActive(false);
				//steps2.SetActive(true);
				w_speed = w_speed + rn_speed;
				playerAnim.SetTrigger("run");
				playerAnim.ResetTrigger("walk");
			}
			if (Input.GetKeyUp(KeyCode.LeftShift))
			{
				//steps1.SetActive(true);
				//steps2.SetActive(false);
				w_speed = olw_speed;
				playerAnim.ResetTrigger("run");
				playerAnim.SetTrigger("walk");
			}
		}
	}

	void Slash()
	{
		isSlashing = true;
		slashTimer = slashCooldown;

		playerAnim.SetTrigger("slash");

		// Activate hitbox for a short window
		weaponHitbox.EnableDamage();
		Invoke(nameof(DisableHitbox), 0.3f); // adjust timing based on animation

		Invoke(nameof(ResetSlash), slashCooldown);
	}

	void DisableHitbox()
	{
		weaponHitbox.DisableDamage();
	}


	void ResetSlash()
	{
		isSlashing = false;
	}

}