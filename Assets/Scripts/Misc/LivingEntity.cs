using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

[RequireComponent (typeof(NetworkIdentity))]
public class LivingEntity : Entity{

	[Header ("Health")]
	public int startingHealth = 100;
	public float slashResistance = 25;
	public float bluntResistance = 25;
	public float piercingResistance = 25;

	public ProfessionWeakness professionWeakness;
	public enum ProfessionWeakness {Woodcutting, Mining, Harvesting};
	public float weaknessAmount = 25;

	[Header ("FX")]
	public bool destroyOnDeath = false;
	[SyncVar]
	protected int health;
	protected bool dead;

	public override void Start() {
		health = startingHealth;
		rig = GetComponent<Rigidbody> ();
	}

	public virtual void TakeDamage (float damage, string masterId) {
		health -= (int)damage;

		// Sound
		if (hurtSound != null) {
			AudioManager.instance.CmdPlaySound (hurtSound.name, transform.position, "", 1);
		}
		if (health <= 0 && !dead) {
			Die ();
		}
	}

	public void InstaKill() {
		health -= health;
		Die ();
	}

	[ClientRpc]
	public virtual void RpcDie() {

	}
		
	public virtual void Die() {
		dead = true;
		RpcDie ();
		OnEntityDestroy ();
		if (destroyOnDeath) {
			Destroy (gameObject);
		}
	}

	public override void DestroyEntity() {
		GameManager.instance.RemoveLivingEntity (this, entityGroupIndex);
		NetworkServer.Destroy (this.gameObject);
	}
}