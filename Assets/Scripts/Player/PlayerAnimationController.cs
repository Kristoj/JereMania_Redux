﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerAnimationController : NetworkBehaviour {

	// Components
	private Animator animator;
	private NetworkAnimator networkAnimator;
	private Animator viewAnimator;
	private CharacterController controller;
	private PlayerController playerController;

	public Transform viewModel;
	[HideInInspector]
	public bool viewModelEnabled = true;

	// id
	[HideInInspector]
	public int lastAttackId = 1;
	private int thisAttackId = 0;
	private int primaryActionId = 0;
	private int secondaryActionId = 0;
	private float vmMoveSpeed = 0;

	// Use this for initialization
	void Start () {
		animator = GetComponent<Animator> ();
		controller = GetComponent<CharacterController>();
		playerController = GetComponent<PlayerController>();
		networkAnimator = GetComponent<NetworkAnimator> ();

		viewModel = transform.Find ("Main Camera").transform.Find ("Arm Holder").transform.Find ("View_Model").transform;

		if (networkAnimator != null) {
			for (int i = 0; i < animator.parameterCount; i++) {
				networkAnimator.SetParameterAutoSend (i, true);
			}
		}

		if (viewModel != null) {
			viewAnimator = viewModel.GetComponent<Animator> ();
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (viewModelEnabled) {
			MoveAnimation ();
		}
	}

	void MoveAnimation() {
		float percentage = controller.velocity.magnitude / playerController.moveSpeed;
		animator.SetFloat ("moveSpeed", percentage);


		if (playerController.IsGrounded() || playerController.IsHardGrounded ()) {
			vmMoveSpeed = Mathf.Lerp (vmMoveSpeed, (new Vector2 (controller.velocity.x, controller.velocity.z).magnitude) / playerController.runSpeed, .2f);
		} else {
			vmMoveSpeed = Mathf.Lerp (vmMoveSpeed, 0, .25f);
		}
		viewAnimator.SetFloat ("holdId", vmMoveSpeed);
	}

	public void Attack() {
		if (viewModel != null && viewModelEnabled) {
			if (lastAttackId == 0) {
				thisAttackId = 1;
			} else {
				thisAttackId = 0;
			}
			lastAttackId = thisAttackId;

			viewAnimator.SetFloat ("attackId", (float)lastAttackId);
			viewAnimator.SetTrigger ("Attack");
		}
	}

	public void MeleeImpact() {
		if (viewModelEnabled && viewModel != null) {
			viewAnimator.SetTrigger ("AttackImpact");
		}
	}

	public void ActionStart(int buttonId) {
		viewAnimator.ResetTrigger ("ActionEnd");
		viewAnimator.ResetTrigger ("ActionEvent");

		int thisActionId = 0;
		if (buttonId == 0) {
			thisActionId = primaryActionId;
		} else {
			thisActionId = secondaryActionId;
		}
		viewAnimator.SetFloat ("actionId", (float)thisActionId);
		viewAnimator.SetTrigger ("ActionStart");
	}

	public void ActionEnd() {
		viewAnimator.SetTrigger ("ActionEnd");
	}

	public void ActionEvent() {
		viewAnimator.SetTrigger ("ActionEvent");
	}

	public void ChangeWeapon() {
		if (viewModelEnabled && viewModel != null) {
			viewAnimator.SetTrigger ("ChangeWeapon");
		}
	}

	public void PickupItem(int buttonId) {
		if (viewModelEnabled && viewModel != null) {
			viewAnimator.SetFloat ("pickupId", buttonId);
			viewAnimator.SetTrigger ("PickupItem");
		}
	}

	public void SetGunAnimationIds (int[] ids) {
		if (viewModel != null) {
			viewAnimator.SetFloat ("holdId", ids[0]);
			primaryActionId = ids[1];
			secondaryActionId = ids[2];
		}
	}

	public void EnableViewModel(bool state) {
		if (state) {
			viewModel.gameObject.SetActive (true);
		} else {
			viewModel.gameObject.SetActive (false);
		}
	}

	public void SetSitState (bool ab) {
		animator.SetBool ("isSitting", ab);
	}
}
