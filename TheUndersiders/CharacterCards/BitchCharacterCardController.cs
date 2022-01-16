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
			base.SpecialStringMaker.ShowNumberOfCardsInPlay(
				new LinqCardCriteria((Card c) => c.DoKeywordsContain("dog") && c.IsVillain,"dog")
			);
			base.SpecialStringMaker.ShowNumberOfCardsAtLocation(
				base.Card.Owner.Trash,
				new LinqCardCriteria((Card c) => c.DoKeywordsContain("dog") && c.IsVillain, "dog")
			).Condition = () => !base.Card.IsFlipped;
		}

		public override IEnumerator Play()
		{
			// When this card enters play, reveal cards from the top of the villain deck until a dog card is revealed. Put it into play. Shuffle the other revealed cards back into the villain deck.
			IEnumerator getDogCR = RevealCards_MoveMatching_ReturnNonMatchingCards(
				base.TurnTakerController,
				base.TurnTaker.Deck,
				playMatchingCards: false,
				putMatchingCardsIntoPlay: true,
				moveMatchingCardsToHand: false,
				new LinqCardCriteria((Card c) => c.DoKeywordsContain("dog") && c.IsVillain, "dog"),
				1
			);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(getDogCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(getDogCR);
			}

			yield break;
		}

		public override void AddSideTriggers()
		{
			if (!base.Card.IsFlipped)
			{
				// At the start of the villain turn, if there are no dog targets in play, move 1 dog card from the villain trash into play.
				AddSideTrigger(AddStartOfTurnTrigger(
					(TurnTaker tt) => tt == base.TurnTaker,
					RecoverDogResponse,
					TriggerType.MoveCard
				));

				// increase damage dealt by dog targets by 1.
				AddSideTrigger(AddIncreaseDamageTrigger(
					(DealDamageAction dd) => dd.DamageSource.Card.DoKeywordsContain("dog"),
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
			if (FindCardsWhere((Card c) => c.DoKeywordsContain("dog") && c.IsInPlayAndHasGameText).Count() == 0)
			{
				IEnumerator recoverDogCR = PlayCardsFromLocation(
					base.TurnTaker.Trash,
					new LinqCardCriteria((Card c) => c.DoKeywordsContain("dog"), "dog"),
					numberOfCards: 1
				);

				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(recoverDogCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(recoverDogCR);
				}
			}

			yield break;
		}

		private IEnumerator RetaliationResponse(DealDamageAction dda)
		{
			IEnumerator retaliateCR = DealDamage(
				dda.Target,
				dda.DamageSource.Card,
				base.Game.H - 1,
				DamageType.Melee,
				isCounterDamage: true,
				cardSource: GetCardSource()
			);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(retaliateCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(retaliateCR);
			}

			yield break;
		}
	}
}
