using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MeleeWeapon : Weapon {

	public override void OnServerEntityHit(string victimName, int victimGroup, string sourcePlayer) {
		if (GameManager.instance.GetLivingEntity (victimName, victimGroup) != null) {
			// Calculate Damage
			LivingEntity victim = GameManager.instance.GetLivingEntity (victimName, victimGroup);
			float dmg = CalculateDamage(victim.slashResistance, victim.bluntResistance, victim.piercingResistance, victim.skillWeakness, victim.weaknessAmount);
			victim.TakeDamage (dmg, sourcePlayer);
		}
	}

	public float CalculateDamage(float slashResistance, float bluntResistance, float piercingResistance, LivingEntity.SkillWeakness skillWeakness, float weaknessAmount) {
		// Calculate Base damage
		float baseDmgSummary = 0;
		baseDmgSummary += slashDamage - (slashDamage * (slashResistance / 100));
		baseDmgSummary += bluntDamage - (bluntDamage * (bluntResistance / 100));
		baseDmgSummary += piercingDamage - (piercingDamage * (piercingResistance / 100));

		// Calculate profession damage bonus
		float skillBonus = 0;
		// Woodcutting
		if (skillWeakness == LivingEntity.SkillWeakness.Woodcutting) {
			skillBonus = (weaknessAmount / 100) * (woodcuttingBonus / 100);
		}
		// Mining
		if (skillWeakness == LivingEntity.SkillWeakness.Mining) {
			skillBonus = (weaknessAmount / 100) * (miningBonus / 100);
		}
		// Harvesting
		if (skillWeakness == LivingEntity.SkillWeakness.Harvesting) {
			skillBonus = (weaknessAmount / 100) * (harvestingBonus / 100);
		}

		// Calculate final damage
		float finalDamage = baseDmgSummary + (baseDmgSummary * skillBonus);
		return finalDamage;
	}
}
