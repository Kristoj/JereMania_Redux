using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FarmPatch : LivingEntity {

	public Transform defaultPlantMesh;
	private Transform curPlantMesh;
	[HideInInspector]
	[SyncVar]
	public bool hasPlant = false;
	[SyncVar]
	[HideInInspector]
	public bool canHarvest = false;
	private float growTime;
	private int growStages;
	private string curPlantName;
	public int curGrowStage;
	private Resource harvestResource;
	private Vector3 originalSize;

	// Visual
	private MeshFilter meshFilter;
	private Collider myCollider;

	public override void Start() {
		base.Start ();
		meshFilter = GetComponent<MeshFilter> ();
		myCollider = GetComponent<Collider> ();
		originalSize = transform.localScale;
		transform.parent = WorldManager.instance.spawnedItemHolder;

		if (isServer) {
			//StartCoroutine(PlantSeed ("Potato"));
		}
	}

	public void PlantSeed(string plantName) {
		EquipmentLibrary.FarmPlant farmPlant = null;
		foreach (EquipmentLibrary.FarmPlant fp in EquipmentLibrary.instance.farmPlant) {
			if (fp.plantName == plantName) {
				farmPlant = fp;
			}
		}

		if (farmPlant == null) {
			return;
		}

		hasPlant = true;
		growStages = farmPlant.growStages;
		growTime = farmPlant.growTime;
		curPlantName = plantName;
		curGrowStage = 1;
		myCollider.enabled = false;
		health = startingHealth;
		dead = false;

		RpcNextStage (curGrowStage, plantName);
		StartCoroutine (GrowPlant (plantName));
	}

	IEnumerator GrowPlant(string plantName) {
		yield return new WaitForSeconds (growTime);
		curGrowStage++;
		// Update plant for clients
		RpcNextStage (curGrowStage, plantName);

		// If plant is has not grown yet move to next stage
		if (curGrowStage < growStages) {
			StartCoroutine (GrowPlant (plantName));
		} 

		// Plant is ready to harvest
		else {
			canHarvest = true;
			RpcPlantFinished ();
		}
	}

	[ClientRpc]
	void RpcNextStage(int stage, string plantName) {
		foreach (EquipmentLibrary.FarmPlant fp in EquipmentLibrary.instance.farmPlant) {
			if (fp.plantName == plantName) {
				// Update plant mesh for clients
				if (fp.growStageMeshes.Length > stage - 1) {
					meshFilter.mesh = fp.growStageMeshes[stage-1];
				}
				// Change size
				float f = fp.plantSizeMinMax.x + ((fp.plantSizeMinMax.y - fp.plantSizeMinMax.x) * ((float)stage / (float)fp.growStages));
				Vector3 newScale = new Vector3 (f, f, f);
				transform.localScale = newScale;

				if (curPlantMesh != null) {
					Destroy (curPlantMesh.gameObject);
				}
				if (defaultPlantMesh != null) {
					curPlantMesh = Instantiate (defaultPlantMesh.transform, transform.position, transform.rotation, transform) as Transform;
					curPlantMesh.transform.position += -transform.up * myCollider.transform.localScale.y / 2;
				}
			}
		}
	}

	[ClientRpc]
	void RpcPlantFinished() {
		myCollider.enabled = true;
		gameObject.layer = LayerMask.NameToLayer ("Default");
		transform.localScale = originalSize;
	}

	void DropResource(string plantName) {
		// Instantiate drop resource
		foreach (EquipmentLibrary.FarmPlant fp in EquipmentLibrary.instance.farmPlant) {
			if (fp.plantName == plantName) {
				if (fp.dropResource != null) {
					int dropCount = Random.Range ((int)fp.dropCountMinMax.x, (int)fp.dropCountMinMax.y);
					for (int i = 0; i < dropCount; i++) {
						Resource clone = Instantiate (fp.dropResource, transform.position, transform.rotation);
						Rigidbody cloneRig = clone.GetComponent<Rigidbody>();
						cloneRig.AddForce (Random.insideUnitSphere * 250 * cloneRig.mass);
						cloneRig.AddTorque (Random.insideUnitSphere * 250 * cloneRig.mass);
						NetworkServer.Spawn (clone.gameObject);
					}
				}
			}
		}
	}

	public override void Die() {
		base.Die ();
		DropResource (curPlantName);
		hasPlant = false;
	}

	public override void OnClientDie () { 
		base.OnClientDie ();
		gameObject.layer = LayerMask.NameToLayer ("MeleeInteraction");
		meshFilter.mesh = null;

		if (curPlantMesh != null) {
			Destroy (curPlantMesh.gameObject);
		}
	}
}
