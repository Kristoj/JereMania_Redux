using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour {

	private static Dictionary <string, LivingEntity> players = new Dictionary<string, LivingEntity>();
	private static Dictionary <string, LivingEntity> characters = new Dictionary<string, LivingEntity>();
	private static Dictionary <string, Entity> entities = new Dictionary<string, Entity>();
	[HideInInspector]
	public Transform localPlayer;
	public static GameManager instance;
	[SyncVar]
	public float money;

	void Awake() {
		instance = this;
	}

	public static void RegisterPlayer(string netId, LivingEntity entity, string prefix) {
		if (!players.ContainsKey (prefix + netId)) {
			players.Add (prefix + netId, entity);
			entity.transform.name = prefix + netId;
		}
	}

	public static LivingEntity GetPlayer (string id) {
		if (players.ContainsKey (id)) {
			return players [id];
		} else {
			return null;
		}
	}

	public static LivingEntity[] GetAllPlayers () {
		LivingEntity[] playerList = new LivingEntity[players.Count-1];


		for (int i = 0; i < players.Count; i++) {
			players.Values.CopyTo (playerList, i);
		}
		return playerList;
	}
		
	public static void RegisterCharacter(string netId, LivingEntity entity, string prefix) {
		if (!characters.ContainsKey (prefix + netId)) {
			characters.Add (prefix + netId, entity);
			entity.transform.name = prefix + netId;
		}
	}

	public static LivingEntity GetCharacter (string id) {
		if (characters.ContainsKey (id)) {
			return characters [id];
		} else {
			return null;
		}
	}

	public static void RegisterEntity(string netId, Entity intera, string prefix) {
		if (!entities.ContainsKey (prefix + netId)) {
			entities.Add (prefix + netId, intera);
			intera.transform.name = prefix + netId;
		}
	}

	public static Entity GetEntity (string id) {
		if (entities.ContainsKey (id)) {
			return entities [id];
		} else {
			return null;
		}
	}

	public void TakeMoney(float f) {
		money -= f;
		money = Mathf.Clamp (money, 0, 999999);
	}

	public void GiveMoney(float f) {
		money += f;
		money = Mathf.Clamp (money, 0, 999999);
	}

	public int GetPlayerCount() {
		return players.Count;
	}
}
