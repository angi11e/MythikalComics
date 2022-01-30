using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.WhatsHerFace
{
	public abstract class RecallBaseCardController : WhatsHerFaceBaseCardController
	{
		// Play this card next to [custom linq criteria].

		public RecallBaseCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		protected abstract LinqCardCriteria CustomCriteria { get; }

		public override IEnumerator DeterminePlayLocation(
			List<MoveCardDestination> storedResults,
			bool isPutIntoPlay,
			List<IDecision> decisionSources,
			Location overridePlayArea = null,
			LinqTurnTakerCriteria additionalTurnTakerCriteria = null
		)
		{
			//When this card enters play, put it next to [insert custom criteria]
			IEnumerator selectHeroCR = SelectCardThisCardWillMoveNextTo(
				CustomCriteria,
				storedResults,
				isPutIntoPlay,
				decisionSources
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selectHeroCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selectHeroCR);
			}
			yield break;
		}

		protected void AddIfTheCardThatThisCardIsNextToLeavesPlayMoveItToYourHandTrigger(IEnumerator doThisFirst = null)
		{
			if (Card.Location.OwnerCard == null)
			{
				return;
			}

			AddTrigger(
				(MoveCardAction moveCard) =>
					IsThisCardNextToCard(moveCard.CardToMove)
					&& !moveCard.Destination.IsInPlayAndNotUnderCard,
				(MoveCardAction d) => MoveToHandAfterNextToCardLeavesPlay(doThisFirst),
				TriggerType.MoveCard,
				TriggerTiming.After
			);

			AddTrigger(
				(BulkMoveCardsAction bulkMove) =>
					bulkMove.CardsToMove.Where((Card c) => IsThisCardNextToCard(c)).Count() > 0,
				(BulkMoveCardsAction d) => MoveToHandAfterNextToCardLeavesPlay(doThisFirst),
				TriggerType.MoveCard,
				TriggerTiming.After
			);

			AddTrigger(
				(FlipCardAction flip) =>
					IsThisCardNextToCard(flip.CardToFlip.Card)
					&& flip.CardToFlip.Card.IsFaceDownNonCharacter,
				(FlipCardAction d) => MoveToHandAfterNextToCardLeavesPlay(doThisFirst),
				TriggerType.MoveCard,
				TriggerTiming.After
			);

			AddTrigger(
				(RemoveTargetAction remove) => IsThisCardNextToCard(remove.CardToRemoveTarget),
				(RemoveTargetAction d) => MoveToHandAfterNextToCardLeavesPlay(doThisFirst),
				TriggerType.MoveCard,
				TriggerTiming.After
			);

			AddTrigger(
				(BulkRemoveTargetsAction remove) =>
					GetCardThisCardIsNextTo() != null
					&& remove.CardsToRemoveTargets.Contains(GetCardThisCardIsNextTo()),
				(BulkRemoveTargetsAction d) => MoveToHandAfterNextToCardLeavesPlay(doThisFirst),
				TriggerType.MoveCard,
				TriggerTiming.After
			);

			AddTrigger(
				(TargetLeavesPlayAction a) => IsThisCardNextToCard(a.TargetLeavingPlay),
				(TargetLeavesPlayAction d) => MoveToHandAfterNextToCardLeavesPlay(doThisFirst),
				TriggerType.MoveCard,
				TriggerTiming.After
			);

			if (Card.Location.OwnerCard.IsHero)
			{
				AddTrigger(
					(MoveCardAction move) =>
						GetCardThisCardIsNextTo() != null
						&& move.CardToMove.Identifier == "IsolatedHero"
						&& !GameController.IsCardVisibleToCardSource(
							GetCardThisCardIsNextTo(),
							GetCardSource()
						),
					(MoveCardAction d) => MoveToHandAfterNextToCardLeavesPlay(doThisFirst),
					TriggerType.MoveCard,
					TriggerTiming.After
				);
			}
		}

		private IEnumerator MoveToHandAfterNextToCardLeavesPlay(IEnumerator doThisFirst)
		{
			IEnumerator doFirstCR = doThisFirst;
			if (doFirstCR != null)
			{
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(doFirstCR);
				}
				else
				{
					GameController.ExhaustCoroutine(doFirstCR);
				}
			}

			if (Card.Owner.IsHero)
			{
				IEnumerator moveCardCR = GameController.MoveCard(
					DecisionMaker,
					Card,
					Card.Owner.ToHero().Hand,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(moveCardCR);
				}
				else
				{
					GameController.ExhaustCoroutine(moveCardCR);
				}
			}

			yield break;
		}
	}
}