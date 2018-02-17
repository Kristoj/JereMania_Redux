using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MeleeWeapon : Weapon {

	public override void TakeDamage(string victimId, int targetGroup, string playerId) {
		CmdTakeDamage (victimId, targetGroup, playerId);
	}

	[Command]
	void CmdTakeDamage (string victimId, int targetGroup, string playerId) {
		if (GameManager.instance.GetLivingEntity (victimId, targetGroup) != null) {
			// Calculate Damage
			LivingEntity victim = GameManager.instance.GetLivingEntity (victimId, targetGroup);
			float dmg = CalculateDamage(victim.slashResistance, victim.bluntResistance, victim.piercingResistance, victim.professionWeakness, victim.weaknessAmount);
			victim.TakeDamage (dmg, playerId);
		}
	}

	public float CalculateDamage(float slashResistance, float bluntResistance, float piercingResistance, LivingEntity.ProfessionWeakness professionWeakness, float weaknessAmount) {
		// Calculate Base damage
		float baseDmgSummary = 0;
		baseDmgSummary += slashDamage - (slashDamage * (slashResistance / 100));
		baseDmgSummary += bluntDamage - (bluntDamage * (bluntResistance / 100));
		baseDmgSummary += piercingDamage - (piercingDamage * (piercingResistance / 100));

		// Calculate profession damage bonus
		float professionBonus = 0;
		// Woodcutting
		if (professionWeakness == LivingEntity.ProfessionWeakness.Woodcutting) {
			professionBonus = (weaknessAmount / 100) * (woodcuttingBonus / 100);
		}
		// Mining
		if (professionWeakness == LivingEntity.ProfessionWeakness.Mining) {
			professionBonus = (weaknessAmount / 100) * (miningBonus / 100);
		}
		// Harvesting
		if (professionWeakness == LivingEntity.ProfessionWeakness.Harvesting) {
			professionBonus = (weaknessAmount / 100) * (harvestingBonus / 100);
		}

		// Calculate final damage
		float finalDamage = baseDmgSummary + (baseDmgSummary * professionBonus);
		return finalDamage;
	}
}
