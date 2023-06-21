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
		 * Remove up to 8 tokens from your trueshot pool and choose a target.
		 * For every 2 tokens removed, {RedRifle} deals that target 1 irreducible damage.
		 * That damage is projectile, energy, sonic, and radiant, in that order.
		 */

		public PoweredShotCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			SpecialStringMaker.ShowTokenPool(TrueshotPool);
		}

		public override IEnumerator Play()
		{
			int tokensRemoved = 0;
			string message = null;
			TokenPool trueshotPool = RedRifleTrueshotPoolUtility.GetTrueshotPool(this);

			// Remove up to 8 tokens from your trueshot pool and choose a target.
			if (trueshotPool == null)
			{
				message = "There is no trueshot pool to remove tokens from.";
			}
			else if (trueshotPool.CurrentValue < 2)
			{
				message = $"There are not enough tokens in {trueshotPool.Name} to remove.";
			}
			else
			{
				message = "Don't forget - odd numbers are illogical.";
			}
			if (message != null)
			{
				IEnumerator noTokensCR = GameController.SendMessageAction(
					message,
					Priority.Medium,
					GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(noTokensCR);
				}
				else
				{
					GameController.ExhaustCoroutine(noTokensCR);
				}
			}
			
			if (trueshotPool.CurrentValue >= 2)
			{
				List<SelectNumberDecision> tokensToRemove = new List<SelectNumberDecision>();
				IEnumerator howManyCR = GameController.SelectNumber(
					DecisionMaker,
					SelectionType.RemoveTokens,
					0,
					trueshotPool.CurrentValue > 8 ? 8 : trueshotPool.CurrentValue,
					storedResults: tokensToRemove,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(howManyCR);
				}
				else
				{
					GameController.ExhaustCoroutine(howManyCR);
				}

				tokensRemoved = tokensToRemove.FirstOrDefault()?.SelectedNumber ?? 0;
			}

			// I still can't figure out why, but SelectTargetAndDealMultipleInstancesOfDamage
			// didn't seem to be able to carry the isIrreducible tags?

			Card target = null;
			List<DealDamageAction> storedResults = new List<DealDamageAction>();
			IEnumerator energyCR = DoNothing();
			IEnumerator sonicCR = DoNothing();
			IEnumerator radiantCR = DoNothing();

			// For every 2 tokens removed, {RedRifle} deals that target 1 irreducible damage.
			// That damage is projectile, energy, sonic, and radiant, in that order.
			if (tokensRemoved >= 2)
			{
				IEnumerator removeTokensCR = RemoveTrueshotTokens<GameAction>(tokensRemoved);

				IEnumerator projectileCR = GameController.SelectTargetsAndDealDamage(
					DecisionMaker,
					new DamageSource(GameController, this.CharacterCard),
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
					yield return GameController.StartCoroutine(removeTokensCR);
					yield return GameController.StartCoroutine(projectileCR);
				}
				else
				{
					GameController.ExhaustCoroutine(removeTokensCR);
					GameController.ExhaustCoroutine(projectileCR);
				}

				target = storedResults.FirstOrDefault().Target;
			}
			if (tokensRemoved >= 4)
			{
				energyCR = DealDamage(
					this.CharacterCard,
					target,
					1,
					DamageType.Energy,
					true,
					cardSource: GetCardSource()
				);
			}
			if (tokensRemoved >= 6)
			{
				sonicCR = DealDamage(
					this.CharacterCard,
					target,
					1,
					DamageType.Sonic,
					true,
					cardSource: GetCardSource()
				);
			}
			if (tokensRemoved >= 8)
			{
				radiantCR = DealDamage(
					this.CharacterCard,
					target,
					1,
					DamageType.Radiant,
					true,
					cardSource: GetCardSource()
				);
			}

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(energyCR);
				yield return GameController.StartCoroutine(sonicCR);
				yield return GameController.StartCoroutine(radiantCR);
			}
			else
			{
				GameController.ExhaustCoroutine(energyCR);
				GameController.ExhaustCoroutine(sonicCR);
				GameController.ExhaustCoroutine(radiantCR);
			}

			yield break;
		}
	}
}