using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.RedRifle
{
	public class HairTriggerCardController : RedRifleBaseCardController
	{
		/*
		 * Whenever a non-hero target enters play, {RedRifle} may deal that target 1 projectile damage.
		 * If that target takes no damage, add 1 token to your trueshot pool.
		 */

		public HairTriggerCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// Whenever a non-hero target enters play, {RedRifle} may deal that target 1 projectile damage.
			AddTargetEntersPlayTrigger(
				(Card c) => !c.IsHero,
				(Card c) => HairTriggerResponse(c),
				TriggerType.DealDamage,
				TriggerTiming.After
			);
			base.AddTriggers();
		}

		private IEnumerator HairTriggerResponse(Card target)
		{
			List<DealDamageAction> storedResults = new List<DealDamageAction>();

			// Whenever a non-hero target enters play, {RedRifle} may deal that target 1 projectile damage.
			IEnumerator dealDamageCR = DealDamage(
				this.CharacterCard,
				target,
				1,
				DamageType.Projectile,
				optional: true,
				storedResults: storedResults,
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

			// If that target takes no damage, add 1 token to your trueshot pool.
			if (!storedResults.Any() || !storedResults.FirstOrDefault().DidDealDamage)
			{
				IEnumerator addTokenCR = RedRifleTrueshotPoolUtility.AddTrueshotTokens(this, 1);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(addTokenCR);
				}
				else
				{
					GameController.ExhaustCoroutine(addTokenCR);
				}
			}

			yield break;
		}
	}
}