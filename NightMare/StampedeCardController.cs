using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.NightMare
{
	public class StampedeCardController : NightMareBaseCardController
	{
		/*
		 * {NightMare} deals each non-Hero Target 1 Melee Damage.
		 * {NightMare} deals each non-Hero Target with 1 HP 1 infernal damage.
		 * 
		 * DISCARD
		 * Destroy a target with 1 HP.
		 */

		public StampedeCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// {NightMare} deals each non-Hero Target 1 Melee Damage.
			IEnumerator damageCR = DealDamage(
				this.CharacterCard,
				(Card c) => !IsHeroTarget(c),
				1,
				DamageType.Melee
			);

			// {NightMare} deals each non-Hero Target with 1 HP 1 infernal damage.
			IEnumerator attritionCR = DealDamage(
				this.CharacterCard,
				(Card c) => !IsHeroTarget(c) && c.HitPoints == 1,
				1,
				DamageType.Infernal
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(damageCR);
				yield return GameController.StartCoroutine(attritionCR);
			}
			else
			{
				GameController.ExhaustCoroutine(damageCR);
				GameController.ExhaustCoroutine(attritionCR);
			}

			yield break;
		}

		protected override IEnumerator DiscardResponse(GameAction ga)
		{
			// Destroy a target with 1 HP.
			IEnumerator destroyWeakCR = GameController.SelectAndDestroyCard(
				DecisionMaker,
				new LinqCardCriteria(
					(Card c) => c.IsTarget && c.HitPoints.Value == 1,
					"targets with 1 HP",
					useCardsSuffix: false
				),
				optional: false,
				cardSource: GetCardSource()
			);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(destroyWeakCR);
			}
			else
			{
				GameController.ExhaustCoroutine(destroyWeakCR);
			}

			yield break;
		}
	}
}