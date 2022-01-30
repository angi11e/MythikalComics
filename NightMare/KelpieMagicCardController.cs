using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.NightMare
{
	public class KelpieMagicCardController : NightMareBaseCardController
	{
		/*
		 * increase damage dealt by {NightMare} by 1.
		 * If {NightMare} would deal melee damage, change its type to infernal.
		 * 
		 * POWER
		 * Draw 1 card. Discard 1 card. Play 1 card.
		 * 
		 * DISCARD
		 * Play a card.
		 */

		public KelpieMagicCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// increase damage dealt by {NightMare} by 1.
			AddIncreaseDamageTrigger((DealDamageAction dda) => dda.DamageSource.IsSameCard(base.CharacterCard), 1);

			// If {NightMare} would deal melee damage, change its type to infernal.
			AddChangeDamageTypeTrigger(
				(DealDamageAction dda) =>
					dda.DamageSource.IsSameCard(base.CharacterCard)
					&& dda.DamageType == DamageType.Melee,
				DamageType.Infernal
			);

			base.AddTriggers();
		}

		public override IEnumerator UsePower(int index = 0)
		{
			// Draw 1 card. Discard 1 card. Play 1 card.
			int drawNumeral = GetPowerNumeral(0, 1);
			int discardNumeral = GetPowerNumeral(1, 1);
			int playNumeral = GetPowerNumeral(2, 1);

			IEnumerator drawCardsCR = DrawCards(DecisionMaker, drawNumeral);
			IEnumerator discardCR = SelectAndDiscardCards(DecisionMaker, discardNumeral);
			IEnumerator playCardsCR = SelectAndPlayCardsFromHand(DecisionMaker, playNumeral);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(drawCardsCR);
				yield return GameController.StartCoroutine(discardCR);
				yield return GameController.StartCoroutine(playCardsCR);
			}
			else
			{
				GameController.ExhaustCoroutine(drawCardsCR);
				GameController.ExhaustCoroutine(discardCR);
				GameController.ExhaustCoroutine(playCardsCR);
			}

			yield break;
		}

		protected override IEnumerator DiscardResponse(GameAction ga)
		{
			// Play a card.
			IEnumerator playCardsCR = SelectAndPlayCardsFromHand(DecisionMaker, 1);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(playCardsCR);
			}
			else
			{
				GameController.ExhaustCoroutine(playCardsCR);
			}

			yield break;
		}
	}
}