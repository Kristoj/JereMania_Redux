﻿using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour {

	private static Dictionary <string, Player> players = new Dictionary<string, Player>();
	private static Dictionary <string, LivingEntity> characters = new Dictionary<string, LivingEntity>();
	private static Dictionary <string, Entity> entities = new Dictionary<string, Entity>();
	private static Dictionary <string, LivingEntity> livingEntities = new Dictionary<string, LivingEntity>();
	public List<EntityGroup> entityGroups = new List<EntityGroup> ();
	public List<LivingEntityGroup> livingEntityGroups = new List<LivingEntityGroup> ();
	private int groupLength = 30;
	[HideInInspector]
	private static Player localPlayer;
	public static GameManager instance;
	[SyncVar]
	public float money;

	void Awake() {
		instance = this;
		entityGroups.Add (new EntityGroup ());
		livingEntityGroups.Add (new LivingEntityGroup ());
	}

	public static void RegisterPlayer(string netId, Player entity, string prefix) {
		if (!players.ContainsKey (prefix + netId)) {
			players.Add (prefix + netId, entity);
			entity.transform.name = prefix + netId;
		}
	}

	/// <summary>
	/// Returns a player by its name.
	/// </summary>
	/// <param name="playerName">Target players name.</param>
	public static Player GetPlayerByName (string playerName) {
		if (players.ContainsKey (playerName)) {
			return players [playerName];
		} else {
			return null;
		}
	}

	public static void RegisterCharacter(string netId, LivingEntity entity, string prefix) {
		if (!characters.ContainsKey (prefix + netId)) {
			characters.Add (prefix + netId, entity);
			entity.transform.name = prefix + netId;
		}
	}

	/**
	public static LivingEntity GetCharacterByName (string characterName) {
		if (characters.ContainsKey (characterName)) {
			return characters [characterName];
		} else {
			return null;
		}
	}


	public static void RegisterEntity(string netId, Entity entity, string prefix) {
		if (!entities.ContainsKey (prefix + netId)) {
			entities.Add (prefix + netId, entity);
			entity.transform.name = prefix + netId;
		}
	}
	**/

	public void RegisterEntity(string netId, Entity entity, string prefix) {
		bool found = false;
		for (int i = 0; i < entityGroups.Count; i++) {
			if (entityGroups [i].GetEntityGroupCount () < groupLength) {
				entityGroups [i].AddEntityToGroup (entity);
				entity.transform.name = prefix + netId;
				entity.SetGroupId (i);
				found = true;
				break;
			}
		}
		if (!found) {
			entityGroups.Add (new EntityGroup ());
			entityGroups [entityGroups.Count - 1].AddEntityToGroup (entity);
			entity.SetGroupId (entityGroups.Count - 1);
		}

		// Entity list
		if (!entities.ContainsKey (prefix + netId)) {
			entities.Add (prefix + netId, entity);
			entity.transform.name = prefix + netId;
		}
	}

	public void RegisterLivingEntity(string netId, LivingEntity entity, string prefix) {
		bool found = false;
		// Check some group has space for new living entity
		for (int i = 0; i < livingEntityGroups.Count; i++) {
			if (livingEntityGroups [i].GetLivingEntityGroupCount () < groupLength) {
				livingEntityGroups [i].AddLivingEntityToGroup (entity);
				entity.transform.name = prefix + netId;
				entity.SetGroupId (i);
				found = true;
				break;
			}
		}
		// If we didn't find any empty spaces, make a new group and store target living entity there
		if (!found) {
			livingEntityGroups.Add (new LivingEntityGroup ());
			livingEntityGroups [livingEntityGroups.Count - 1].AddLivingEntityToGroup (entity);
			entity.SetGroupId (livingEntityGroups.Count - 1);
		}

		// Also add the target living entity to our living entities list
		if (!livingEntities.ContainsKey (prefix + netId)) {
			livingEntities.Add (prefix + netId, entity);
			entity.transform.name = prefix + netId;
		}
	}

	/// <summary>
	/// Removes the target entity from a target group.
	/// </summary>
	/// <param name="entityToRemove">Entity to remove.</param>
	/// <param name="entityGroupIndex">Entitys group index that we are trying to remove.</param>
	public void RemoveEntity(Entity entityToRemove, int entityGroupIndex) {
		entityGroups [entityGroupIndex].RemoveEntityFromGroup (entityToRemove, entityGroupIndex);
	}

	/// <summary>
	/// Removes the target livingentity from a target group.
	/// </summary>
	/// <param name="livingEntityToRemove">Living entity to remove.</param>
	/// <param name="entityGroupIndex">LivingEntitys group index that we are trying to remove.</param>
	public void RemoveLivingEntity(LivingEntity livingEntityToRemove, int entityGroupIndex) {
		livingEntityGroups [entityGroupIndex].RemoveLivingEntityFromGroup (livingEntityToRemove, entityGroupIndex);
	}
		
	/// <summary>
	/// Gets a entity by name.
	/// </summary>
	/// <returns>The entity.</returns>
	/// <param name="entityName">Entity gameObject name we're looking for.</param>
	/// <param name="entityGroupIndex">Entitys group index were looking for. This reference can be fetched from entity class</param>
	public Entity GetEntity(string entityName, int entityGroupIndex) {
		Entity e = null;
		// Try to get a entity
		if (entityGroupIndex <= entityGroups.Count-1) {
			e = entityGroups [entityGroupIndex].GetEntityFromGroup (entityName);
		} 
		// Try to get a living entity
		if (e == null) {
			e = livingEntityGroups [entityGroupIndex].GetLivingEntityFromGroup (entityName) as Entity;
		}
		// Try to get a entity using alternate method
		if (e == null) {
			if (entities.ContainsKey (entityName)) {
				e = entities [entityName] as Entity;
				if (e != null) {
					//Debug.LogWarning ("Used alternative fetch method on a entity");
				} else {
					Debug.LogError ("Vili pls...Could not find target entity");
				}
			}

		}
		// Try to get a living entity using alternate method
		if (e == null) {
			if (livingEntities.ContainsKey (entityName)) {
				e = livingEntities [entityName] as Entity;
				if (e != null) {
					//Debug.LogWarning ("Used alternative fetch method on a entity");
				} else {
					Debug.LogError ("Vili pls...Could not find target entity");
				}
			}
		}
		return e;
	}

	/// <summary>
	/// Gets a equipment by name.
	/// </summary>
	/// <param name="entityName">Target equipments gameobject name.</param>
	/// <param name="entityGroupIndex">Entity group index. You can get this from the entity class.</param>
	public Equipment GetEquipment(string entityName, int entityGroupIndex) {
		Equipment e = null;
		Entity reference = null;
		// Try to get a entity
		if (entityGroupIndex <= entityGroups.Count-1) {
			e = entityGroups [entityGroupIndex].GetEntityFromGroup (entityName) as Equipment;
		} 
		// Try to get a living entity
		if (e == null) {
			reference = livingEntityGroups [entityGroupIndex].GetLivingEntityFromGroup (entityName);
			if (reference != null) {
				e = reference.GetComponent<Equipment> ();
			}
		}
		// Try to get a entity using alternate method
		if (e == null) {
			if (entities.ContainsKey (entityName)) {
				e = entities [entityName] as Equipment;
				if (e != null) {
					//Debug.LogWarning ("Used alternative fetch method on a entity");
				} else {
					Debug.LogError ("Vili pls...Could not find target entity");
				}
			}
		}
		// Try to get a living entity using alternate method
		if (e == null) {
			if (livingEntities.ContainsKey (entityName)) {
				reference = livingEntities [entityName];
				if (e != null) {
					e = reference.GetComponent<Equipment> ();
					//Debug.LogWarning ("Used alternative fetch method on a entity");
				} else {
					Debug.LogError ("Vili pls...Could not find target entity");
				}
			}
		}
		return e;
	}

	/// <summary>
	/// Gets a living entity by name.
	/// </summary>
	/// <param name="entityName">Living Entitys gameobject name.</param>
	/// <param name="entityGroupIndex">Entity group index. You can get this from the entity class.</param>
	public LivingEntity GetLivingEntity(string entityName, int entityGroupIndex) {
		LivingEntity l = null;
		if (livingEntityGroups.Count - 1 >= entityGroupIndex) {
			l = livingEntityGroups [entityGroupIndex].GetLivingEntityFromGroup (entityName);
		}
		if (l == null) {
			if (livingEntities.ContainsKey (entityName)) {
				l = livingEntities [entityName];
				if (l != null) {
					//Debug.LogWarning ("Used alternative fetch method on a living entity");
				} else {
					Debug.LogError ("Could not find target living entity");
				}

			}
		}
		return l;
	}


	public void TakeMoney(float f) {
		money -= f;
		money = Mathf.Clamp (money, 0, 999999);
	}

	public void GiveMoney(float f) {
		money += f;
		money = Mathf.Clamp (money, 0, 999999);
	}

	// Player related functions
	#region Player related functions
	public int GetPlayerCount() {
		return players.Count;
	}

	/// <summary>
	/// Gets the local player.
	/// </summary>
	public static Player GetLocalPlayer() {
		return localPlayer;
	}

	/// <summary>
	/// Sets the local player.
	/// </summary>
	/// <param name="p">New local player.</param>
	public void SetLocalPlayer(Player p) {
		localPlayer = p;
	}

	/// <summary>
	/// Gets the local player average ping.
	/// </summary>
	/// <returns>The local player average ping.</returns>
	public static int GetLocalPlayerAveragePing() {
		return Network.GetAveragePing (Network.player);
	}
	#endregion


	[System.Serializable]
	public class EntityGroup {
		public List<Entity> groupEntities = new List<Entity>();

		public Entity GetEntityFromGroup(string entityName ) {
			foreach (Entity e in groupEntities) {
				if (e != null) {
					if (e.name == entityName) {
						return e;
					}
				}
			}
			return null;
		}

		public void AddEntityToGroup(Entity entityToAdd) {
			if (!groupEntities.Contains (entityToAdd)) {
				groupEntities.Add (entityToAdd);
			}
		}

		public void RemoveEntityFromGroup(Entity e, int index) {
			if (groupEntities.Contains (e)) {
				groupEntities.Remove (e);
			}
			// groupEntities.Sort ();
		}
			

		public int GetEntityGroupCount() {
			return groupEntities.Count;
		}
	}

	[System.Serializable]
	public class LivingEntityGroup {
		[SyncVar]
		public List<LivingEntity> groupLivingEntities = new List<LivingEntity>();

		public LivingEntity GetLivingEntityFromGroup(string entityName ) {
			foreach (LivingEntity e in groupLivingEntities) {
				if (e != null) {
					if (e.name == entityName) {
						return e;
					}
				}
			}
			return null;
		}

		public void AddLivingEntityToGroup(LivingEntity entityToAdd) {
			if (!groupLivingEntities.Contains (entityToAdd)) {
				groupLivingEntities.Add (entityToAdd);
			}
		}

		public void RemoveLivingEntityFromGroup(LivingEntity e, int index) {
			if (groupLivingEntities.Contains (e)) {
				groupLivingEntities.Remove (e);
			}
			// groupEntities.Sort ();
		}

		public int GetLivingEntityGroupCount() {
			return groupLivingEntities.Count;
		}
	}
}
