using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using System.Collections;
using Handelabra;

namespace Angille.TheUndersiders
{
	public class BitchCharacterCardController : TheUndersidersVillainCardController
	{
		public BitchCharacterCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
			SpecialStringMaker.ShowNumberOfCardsInPlay(
				new LinqCardCriteria((Card c) => c.DoKeywordsContain("dog") && c.IsVillain, "dog")
			);
			SpecialStringMaker.ShowNumberOfCardsAtLocation(
				this.Card.Owner.Trash,
				new LinqCardCriteria((Card c) => c.DoKeywordsContain("dog") && c.IsVillain, "dog")
			).Condition = () => !this.Card.IsFlipped;
		}

		public override IEnumerator Play()
		{
			// When this card enters play, reveal cards from the top of the villain deck until a dog card is revealed. Put it into play. Shuffle the other revealed cards back into the villain deck.
			IEnumerator getDogCR = RevealCards_MoveMatching_ReturnNonMatchingCards(
				this.TurnTakerController,
				this.TurnTaker.Deck,
				playMatchingCards: false,
				putMatchingCardsIntoPlay: true,
				moveMatchingCardsToHand: false,
				new LinqCardCriteria((Card c) => c.DoKeywordsContain("dog") && IsVillain(c), "dog"),
				1
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(getDogCR);
			}
			else
			{
				GameController.ExhaustCoroutine(getDogCR);
			}

			yield break;
		}

		public override void AddSideTriggers()
		{
			if (!this.Card.IsFlipped)
			{
				// At the start of the villain turn, if there are no villain dog cards in play...
				// ...move 1 dog card from the villain trash into play.
				AddSideTrigger(AddStartOfTurnTrigger(
					(TurnTaker tt) => tt == this.TurnTaker,
					RecoverDogResponse,
					TriggerType.MoveCard
				));

				// increase damage dealt by dog targets by 1.
				AddSideTrigger(AddIncreaseDamageTrigger(
					(DealDamageAction dd) => dd.DamageSource.IsCard && dd.DamageSource.Card.DoKeywordsContain("dog"),
					1
				));

				// Treat {Dog} effects as active. (this is done by the cards)
			}
			else
			{
				// When a dog target is destroyed by damage from a target, it first deals {H - 1} melee damage to that target.
				AddSideTrigger(AddTrigger<DealDamageAction>(
					dda => dda.Target.DoKeywordsContain("dog")
						&& dda.Target.HitPoints <= dda.Amount
						&& dda.DamageSource.IsTarget
						&& dda.Target != dda.DamageSource.Card,
					RetaliationResponse,
					new[] { TriggerType.DealDamage },
					TriggerTiming.Before
				));
			}

			base.AddSideTriggers();
		}

		private IEnumerator RecoverDogResponse(PhaseChangeAction p)
		{
			if (FindCardsWhere(
				(Card c) => c.DoKeywordsContain("dog") && IsVillain(c) && c.IsInPlayAndHasGameText
			).Count() == 0)
			{
				IEnumerator recoverDogCR = PlayCardsFromLocation(
					this.TurnTaker.Trash,
					new LinqCardCriteria((Card c) => c.DoKeywordsContain("dog"), "dog"),
					numberOfCards: 1
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(recoverDogCR);
				}
				else
				{
					GameController.ExhaustCoroutine(recoverDogCR);
				}
			}

			yield break;
		}

		private IEnumerator RetaliationResponse(DealDamageAction dda)
		{
			IEnumerator retaliateCR = DealDamage(
				dda.Target,
				dda.DamageSource.Card,
				H - 1,
				DamageType.Melee,
				isCounterDamage: true,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(retaliateCR);
			}
			else
			{
				GameController.ExhaustCoroutine(retaliateCR);
			}

			yield break;
		}
	}
}
