using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player : LivingEntity {
	[Header ("Main Properties")]
	public Camera cam;
	[HideInInspector]
	public short myMsgId = 1000;

	// Reference classes
	[HideInInspector]
	public PlayerController playerController;
	[HideInInspector]
	public PlayerWeaponController weaponController;
	[HideInInspector]
	public PlayerSkills playerSkills;
	[HideInInspector]
	public PlayerStats playerStats;
	[HideInInspector]
	public PlayerAnimationController animationController;

	public override void OnStartClient() {
		string myID = GetComponent<NetworkIdentity> ().netId.ToString ();
		Player entity = GetComponent<Player> ();
		GameManager.RegisterPlayer (myID, entity, entityName);
	}

	void Awake() {
		// References
		playerController = GetComponent<PlayerController>();
		weaponController = GetComponent<PlayerWeaponController>();
		playerSkills = GetComponent<PlayerSkills>();
		playerStats = GetComponent<PlayerStats>();
		animationController = GetComponent<PlayerAnimationController>();
	}

	public override void Start() {
		base.Start ();
		if (gameObject.activeSelf) {
			if (isLocalPlayer) {
				NetworkManager.singleton.client.RegisterHandler (myMsgId, OnMessageReceive);
				GameManager.instance.SetLocalPlayer(this);
				SetAuthority (GameManager.instance.netId, GetComponent<NetworkIdentity> ());
			}
		}
	}

	/// <summary>
	/// Is called when local client receives a message from the server.
	/// </summary>
	/// <param name="netMsg">Message that is send to the client.</param>
	public void OnMessageReceive(NetworkMessage netMsg) {
		
	}

	public void KillPlayer() {
		// Penalties
		GameManager.instance.TakeMoney (75);
		PlayerInventory playerInventory = GetComponent<PlayerInventory> ();
		playerInventory.FlushInventory ();

		// Reset player stats
		PlayerStats ps = GetComponent<PlayerStats> ();
		ps.FatiqueAdd (100);
		ps.StaminaAdd (100);
		ps.HungerAdd (100);

		// Reset player stats
		CmdKillPlayer ();
	}

	[Command]
	void CmdKillPlayer() {
		RpcKillPlayer ();
	}

	[ClientRpc]
	void RpcKillPlayer() {
		transform.position = new Vector3 (0, .6f, 0);
	}

	#region AUTHORITY
	public void SetAuthority(NetworkInstanceId objectId, NetworkIdentity targetPlayer) {
		if (isServer) {
			AcceptAuthority (objectId, targetPlayer);
		} else {
			CmdSetAuthority (objectId, targetPlayer);
		}
	}

	[Command]
	void CmdSetAuthority(NetworkInstanceId objectId, NetworkIdentity targetPlayer) {
		AcceptAuthority (objectId, targetPlayer);
	}

	void AcceptAuthority(NetworkInstanceId objectId, NetworkIdentity targetPlayer) {
		GameObject targetObject = NetworkServer.FindLocalObject (objectId);
		if (targetObject != null) {
			NetworkIdentity targetIdentity = targetObject.GetComponent<NetworkIdentity> ();
			var currentOwner = targetIdentity.clientAuthorityOwner;

			if (currentOwner == targetPlayer.connectionToClient) {
				return;
			} else {
				if (currentOwner != null) {
					targetIdentity.RemoveClientAuthority (currentOwner);
				}
				targetIdentity.AssignClientAuthority (targetPlayer.connectionToClient);
			}
		}
	}
	#endregion

	#region Get Player Info
	public void SetCameraOffset (float f) {
		playerController.curViewOffset = f;
	}

	public void TeleportPlayer(Vector3 pos) {
		transform.position = pos;
	}

	public void SetCameraRotationY (float yRot) {
		playerController.camRotY = yRot;
	}

	public void AddCameraRotationX (float xRot) {
		playerController.camRotX += xRot;
	}

	public bool canJump {
		get {
			return playerController.canJump;
		}
		set {
			playerController.canJump = value;
		}
	}

	// Player static
	/// <summary>
	/// State that determines that should player be static. (Physics or player movement doesn't affect him). He only moves with his parent.
	/// </summary>
	/// <value><c>true</c> if the player is static; otherwise, <c>false</c>.</value>
	public bool isStatic {
		get {
			return playerController.isStatic;
		}
		set {
			playerController.isStatic = value;
		}
	}

	// Player active
	/// <summary>
	/// State that determines that does the player have control of himself.
	/// </summary>
	/// <value><c>true</c> if the player has control of himself; otherwise, <c>false</c>.</value>
	public bool isActive {
		get {
			return playerController.isActive;
		}

		set {
			playerController.isActive = value;
		}
	}

	#endregion
}
