using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.RedRifle
{
	public class PoweredShotCardController : RedRifleBaseCardController
	{
		/*
		 * {RedRifle} deals 1 target 1 irreducible projectile damage.
		 * Remove 2 tokens from your trueshot pool.
		 *  If you do so, {RedRifle} deals that target 1 irreducible energy damage.
		 * Remove 2 tokens from your trueshot pool.
		 *  If you do so, {RedRifle} deals that target 1 irreducible sonic damage.
		 * Remove 2 tokens from your trueshot pool.
		 *  If you do so, {RedRifle} deals that target 1 irreducible radiant damage.
		 */

		public PoweredShotCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			base.SpecialStringMaker.ShowTokenPool(base.TrueshotPool);
		}

		public override IEnumerator Play()
		{
			List<DealDamageAction> storedResults = new List<DealDamageAction>();

			// {RedRifle} deals 1 target 1 irreducible projectile damage.
			IEnumerator dealDamageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, base.Card),
				1,
				DamageType.Projectile,
				1,
				false,
				1,
				true,
				storedResultsDamage: storedResults,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(dealDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(dealDamageCR);
			}

			Card target = storedResults.FirstOrDefault().Target;
			// If you do so, {RedRifle} deals that target 1 irreducible energy damage.
			IEnumerator bonusDamageEnergy = DealBonusDamage(target, DamageType.Energy);
			// If you do so, {RedRifle} deals that target 1 irreducible sonic damage.
			IEnumerator bonusDamageSonic = DealBonusDamage(target, DamageType.Sonic);
			// If you do so, {RedRifle} deals that target 1 irreducible radiant damage.
			IEnumerator bonusDamageRadiant = DealBonusDamage(target, DamageType.Radiant);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(bonusDamageEnergy);
				yield return GameController.StartCoroutine(bonusDamageSonic);
				yield return GameController.StartCoroutine(bonusDamageRadiant);
			}
			else
			{
				GameController.ExhaustCoroutine(bonusDamageEnergy);
				GameController.ExhaustCoroutine(bonusDamageSonic);
				GameController.ExhaustCoroutine(bonusDamageRadiant);
			}

			yield break;
		}

		private IEnumerator DealBonusDamage(Card target, DamageType damageType)
		{
			if (target.IsTarget && base.TrueshotPool.CurrentValue >= 2)
			{
				// Remove 2 tokens from your trueshot pool.
				IEnumerator removeTokens = RedRifleTrueshotPoolUtility.RemoveTrueshotTokens<GameAction>(this, 2);

				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(removeTokens);
				}
				else
				{
					base.GameController.ExhaustCoroutine(removeTokens);
				}

				// If you do so, {RedRifle} deals that target 1 irreducible ENERGYTYPE damage.
				IEnumerator dealDamageCR = DealDamage(
					base.Card,
					target,
					1,
					damageType,
					true,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(dealDamageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(dealDamageCR);
				}
			}

			yield break;
		}
	}
}