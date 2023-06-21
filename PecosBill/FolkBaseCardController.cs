using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.PecosBill
{
	public abstract class FolkBaseCardController : PecosBillBaseCardController
	{
		/*
		 * at the end of your turn, you may discard a card.
		 * If you do, <DiscardRewardResponse()>.
		 * 
		 * When this card would be destroyed,
		 * destroy all [u]hyperbole[/u] cards next to it instead and restore it to 5 HP.
		 * Otherwise, {PecosBill} deals himself 2 psychic damage, then destroy this card.
		 */

		private readonly LinqCardCriteria _hyperboleCriteria;

		protected FolkBaseCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
		{
			_hyperboleCriteria = new LinqCardCriteria(
				(Card c) => c.IsInPlay && IsHyperbole(c) && c.Location == this.Card.NextToLocation,
				"hyperbole"
			);

			SpecialStringMaker.ShowNumberOfCards(_hyperboleCriteria);
		}

		// public override bool CanBeDestroyed => false;

		public override void AddTriggers()
		{
			base.AddTriggers();

			// When this card would be destroyed...
			AddTrigger(
				(DestroyCardAction dca) => dca.CardToDestroy.Card == this.Card,
				DestroyAttemptResponse,
				TriggerType.CancelAction,
				TriggerTiming.Before,
				priority: TriggerPriority.High
			);

			// at the end of your turn...
			AddEndOfTurnTrigger(
				(TurnTaker tt) => tt == this.TurnTaker,
				EndTurnDiscardResponse,
				TriggerType.DiscardCard
			);
		}

		private IEnumerator DestroyAttemptResponse(DestroyCardAction dca)
		{
			// When this card would be destroyed...
			List<DestroyCardAction> storedResults = new List<DestroyCardAction>();
			IEnumerable<Card> hyperboles = GameController.FindCardsWhere(
				(Card c) => _hyperboleCriteria.Criteria(c) && !GameController.IsCardIndestructible(c),
				visibleToCard: GetCardSource()
			);

			if (hyperboles.Any())
			{
				// ...destroy all [u]hyperbole[/u] cards next to it instead...
				IEnumerator cancelCR = CancelAction(dca);
				IEnumerator destroyHyperboleCR = GameController.DestroyCards(
					DecisionMaker,
					_hyperboleCriteria,
					storedResults: storedResults,
					showOutput: true,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(cancelCR);
					yield return GameController.StartCoroutine(destroyHyperboleCR);
				}
				else
				{
					GameController.ExhaustCoroutine(cancelCR);
					GameController.ExhaustCoroutine(destroyHyperboleCR);
				}
			}

			if (DidDestroyCard(storedResults))
			{
				// ...and restore it to 5 HP.
				IEnumerator restoreCR = GameController.SetHP(
					this.Card,
					this.Card.MaximumHitPoints.Value,
					GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(restoreCR);
				}
				else
				{
					GameController.ExhaustCoroutine(restoreCR);
				}
			}
			else
			{
				// Otherwise, {PecosBill} deals himself 2 psychic damage...
				IEnumerator griefDamageCR = DealDamage(
					this.CharacterCard,
					this.CharacterCard,
					2,
					DamageType.Psychic,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(griefDamageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(griefDamageCR);
				}
			}
			yield break;
		}

		private IEnumerator EndTurnDiscardResponse(PhaseChangeAction pca)
		{
			// ...you may discard a card.
			List<DiscardCardAction> storedResults = new List<DiscardCardAction>();
			IEnumerator discardCR = GameController.SelectAndDiscardCard(
				DecisionMaker,
				true,
				storedResults: storedResults,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(discardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(discardCR);
			}

			if (DidDiscardCards(storedResults))
			{
				// ...if you do... (different for each folk)
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(DiscardRewardResponse());
				}
				else
				{
					GameController.ExhaustCoroutine(DiscardRewardResponse());
				}
			}

			yield break;
		}

		protected abstract IEnumerator DiscardRewardResponse();
	}
}