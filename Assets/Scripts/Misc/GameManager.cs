using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour {

	private static Dictionary <string, LivingEntity> players = new Dictionary<string, LivingEntity>();
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

	public static void RegisterPlayer(string netId, LivingEntity entity, string prefix) {
		if (!players.ContainsKey (prefix + netId)) {
			players.Add (prefix + netId, entity);
			entity.transform.name = prefix + netId;
		}
	}

	public static LivingEntity GetPlayerByName (string playerName) {
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

	public static LivingEntity GetCharacterByName (string characterName) {
		if (characters.ContainsKey (characterName)) {
			return characters [characterName];
		} else {
			return null;
		}
	}

	/**
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
		for (int i = 0; i < livingEntityGroups.Count; i++) {
			if (livingEntityGroups [i].GetLivingEntityGroupCount () < groupLength) {
				livingEntityGroups [i].AddLivingEntityToGroup (entity);
				entity.transform.name = prefix + netId;
				entity.SetGroupId (i);
				found = true;
				break;
			}
		}
		if (!found) {
			livingEntityGroups.Add (new LivingEntityGroup ());
			livingEntityGroups [livingEntityGroups.Count - 1].AddLivingEntityToGroup (entity);
			entity.SetGroupId (livingEntityGroups.Count - 1);
		}

		// Entity list
		if (!livingEntities.ContainsKey (prefix + netId)) {
			livingEntities.Add (prefix + netId, entity);
			entity.transform.name = prefix + netId;
		}
	}

	public void RemoveEntity(Entity entityToRemove, int entityGroupIndex) {
		entityGroups [entityGroupIndex].RemoveEntityFromGroup (entityToRemove, entityGroupIndex);
	}

	public void RemoveLivingEntity(LivingEntity livingEntityToRemove, int entityGroupIndex) {
		livingEntityGroups [entityGroupIndex].RemoveLivingEntityFromGroup (livingEntityToRemove, entityGroupIndex);
	}



	/**
	public Entity GetEntity (string entityName) {
		if (entities.ContainsKey (entityName)) {
			return entities [entityName];
		} else {
			return null;
		}
	}
	**/



	public Entity GetEntity(string entityName, int entityGroupIndex) {
		Entity e = null;
		if (entityGroupIndex <= entityGroups.Count - 1) {
			e = entityGroups [entityGroupIndex].GetEntityFromGroup (entityName);
		}
		if (e == null) {
			if (entities.ContainsKey (entityName)) {
				e = entities [entityName];
				if (e != null) {
					//Debug.LogWarning ("Used alternative fetch method on a entity");
				} else {
					Debug.LogError ("Could not find target entity");
				}
			}
		}
		return e;
	}

	public LivingEntity GetLivingEntity(string entityName, int entityGroupIndex) {
		LivingEntity l = livingEntityGroups[entityGroupIndex].GetLivingEntityFromGroup (entityName);
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

	public int GetPlayerCount() {
		return players.Count;
	}

	public static Player GetLocalPlayer() {
		return localPlayer;
	}

	public void SetLocalPlayer(Player p) {
		localPlayer = p;
	}

	[System.Serializable]
	public class EntityGroup {
		[SyncVar]
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
