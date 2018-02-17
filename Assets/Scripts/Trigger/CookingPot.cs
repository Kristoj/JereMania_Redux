using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CookingPot : NetworkBehaviour {

	public float cookTime = 20;
	public float fuelConsumption = .2f;
	private float fuel = 100;
	public int potatoCount = 0;
	public int mushroomCount = 0;
	public int carrotCount = 0;
	private bool isCooking = false;
	public Rigidbody itemToSpawn;

	void Update() {
		if (isServer) {
			if (potatoCount > 0 && carrotCount > 0 && mushroomCount > 0 && !isCooking) {
				StartCoroutine (MeltOre ());
			}
		}
	}

	void OnTriggerEnter (Collider c) {

		if (isServer) {
			Resource dynamicEntity = c.GetComponent<Resource> ();

			if (dynamicEntity != null && dynamicEntity.isAvailable) {
				if (dynamicEntity.entityName == "Potato" && potatoCount < 1) {
					potatoCount++;
					NetworkServer.Destroy (c.gameObject);
				}
					
				if (dynamicEntity.entityName == "Carrot" && carrotCount < 1) {
					carrotCount++;
					NetworkServer.Destroy (c.gameObject);
				}

				if (dynamicEntity.entityName == "Mushroom" && mushroomCount < 1) {
					mushroomCount++;
					NetworkServer.Destroy (c.gameObject);
				}

				if (dynamicEntity.entityName == "Wood") {
					fuel += 30;
					fuel = Mathf.Clamp (fuel, 0, 100);
					NetworkServer.Destroy (c.gameObject);
				}
			}
		}
	}

	IEnumerator MeltOre() {
		float t = 0;
		isCooking = true;
		while (t < cookTime) {
			if (fuel > 0) {
				t += Time.deltaTime;
				fuel -= Time.deltaTime * fuelConsumption;
			}
			yield return null;
		}
			
		Rigidbody clone = Instantiate (itemToSpawn, transform.position + transform.up, transform.rotation);
		NetworkServer.Spawn (clone.gameObject);
		RpcSpawnFood (clone.GetComponent<NetworkIdentity>().netId);

		potatoCount--;
		carrotCount--;
		mushroomCount--;
		isCooking = false;
	}

	[ClientRpc]
	void RpcSpawnFood(NetworkInstanceId cloneId) {
		GameObject cloneObject = ClientScene.FindLocalObject (cloneId);
		if (cloneObject != null) {
			Rigidbody rg = cloneObject.GetComponent<Rigidbody> ();
			rg.AddForce (Vector3.up * 5 * rg.mass, ForceMode.Impulse);
			rg.AddTorque (Random.insideUnitSphere * 5 * rg.mass, ForceMode.Impulse);
		}
	}
}
