using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.RedRifle
{
	public class OverclockingCardController : RedRifleBaseCardController
	{
		/*
		 * Increase damage dealt by {RedRifle} by 1.
		 * When this card is destroyed, {RedRifle} deals himself 2 energy damage. Add 3 tokens to your trueshot pool.
		 * POWER
		 * Play 2 one-shot cards now. Destroy this card.
		 */

		public OverclockingCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			base.SpecialStringMaker.ShowNumberOfCardsAtLocation(
				HeroTurnTaker.Hand,
				new LinqCardCriteria(c => c.IsOneShot, "one-shot", true)
			);
		}

		public override void AddTriggers()
		{
			// Increase damage dealt by {RedRifle} by 1.
			AddIncreaseDamageTrigger((DealDamageAction dd) => dd.DamageSource.IsSameCard(base.CharacterCard), 1);

			// When this card is destroyed, {RedRifle} deals himself 2 energy damage. Add 3 tokens to your trueshot pool.
			AddWhenDestroyedTrigger(
				DestructionResponse,
				new TriggerType[2] {
					TriggerType.DealDamage,
					TriggerType.AddTokensToPool
				}
			);

			base.AddTriggers();
		}

		private IEnumerator DestructionResponse(DestroyCardAction d)
		{
			// {RedRifle} deals himself 2 energy damage.
			IEnumerator selfDamageCR = DealDamage(
				base.CharacterCard,
				base.CharacterCard,
				2,
				DamageType.Energy
			);

			// Add 3 tokens to your trueshot pool.
			IEnumerator addTokensCR = RedRifleTrueshotPoolUtility.AddTrueshotTokens(this, 3);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selfDamageCR);
				yield return GameController.StartCoroutine(addTokensCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selfDamageCR);
				GameController.ExhaustCoroutine(addTokensCR);
			}

			yield break;
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int cardCount = GetPowerNumeral(0, 2);

			// Play 2 one-shot cards now.
			IEnumerator playCardsCR = SelectAndPlayCardsFromHand(
				DecisionMaker,
				cardCount,
				cardCriteria: new LinqCardCriteria((Card c) => c.IsOneShot, "one-shot")
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(playCardsCR);
			}
			else
			{
				GameController.ExhaustCoroutine(playCardsCR);
			}

			// Destroy this card.
			IEnumerator destructionCR = GameController.DestroyCard(
				this.DecisionMaker,
				base.Card,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(destructionCR);
			}
			else
			{
				GameController.ExhaustCoroutine(destructionCR);
			}

			yield break;
		}
	}
}