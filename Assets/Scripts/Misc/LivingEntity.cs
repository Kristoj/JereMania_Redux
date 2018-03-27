using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

[RequireComponent (typeof(NetworkIdentity))]
public class LivingEntity : Entity{

	[Header ("Health")]
	public int startingHealth = 100;
	[SyncVar]
	public int health;
	[Header("Entity Resistance")]
	public float slashResistance = 25;
	public float bluntResistance = 25;
	public float piercingResistance = 25;
	public SkillWeakness skillWeakness;
	public enum SkillWeakness {Woodcutting, Mining, Harvesting};
	public float weaknessAmount = 25;

	[Header("Experience")]
	public List<ExperienceDropTable> expDropTable = new List<ExperienceDropTable>();

	[Header ("FX")]
	protected bool dead;

	public override void Start() {
		health = startingHealth;
		rig = GetComponent<Rigidbody> ();
	}

	public override void OnEntityHit (string sourcePlayer, string sourceEquipmentName, float damage) {
		base.OnEntityHit (sourcePlayer, sourceEquipmentName, damage);
		TakeDamage (damage, sourcePlayer);
	}

	/// <summary>
	/// Apply damage to target living entity.
	/// </summary>
	/// <param name="damage">Damage amount.</param>
	/// <param name="masterId">Who caused the damage.</param>
	public virtual void TakeDamage (float damage, string sourcePlayer) {
		if (!dead) {
			health -= (int)damage;

			if (health <= 0 && !dead) {
				Die (sourcePlayer);
			}
		}
	}
		
	public virtual void Die(string sourcePlayer) {
		OnEntityDestroy (sourcePlayer);
		RpcDie ();
	}

	[ClientRpc]
	void RpcDie() {
		dead = true;
		OnClientDie();
	}

	public virtual void OnClientDie() {
		AchievementProgressTracker.instance.AddAchievementProgress (entityName);
	}

	/// <summary>
	/// Destroys the entity from all clients and unregisters it from the gamemanager.
	/// Must be called from the server!
	/// </summary>
	/// <param name="sourcePlayer">Player who called this function.</param>
	public override void DestroyEntity(string sourcePlayer) {
		Debug.Log ("LE");
		//base.DestroyEntity (sourcePlayer);
		GameManager.instance.RemoveLivingEntity (this, entityGroupIndex);
		NetworkServer.Destroy (this.gameObject);
	}
}