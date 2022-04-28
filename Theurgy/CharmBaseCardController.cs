using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Theurgy
{
	public abstract class CharmBaseCardController : TheurgyBaseCardController
	{
		// Play this card next to a hero character card.
		// [add some unique triggers]
		// That hero gains the following power: destroy this card
		// When this card is destroyed, [special effects]

		public CharmBaseCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			AddAsPowerContributor();
		}

		protected abstract TriggerType[] DestructionTriggers { get; }

		public override void AddTriggers()
		{
			base.AddTriggers();

			// when this card is destroyed...
			AddWhenDestroyedTrigger(CharmDestroyResponse, DestructionTriggers);
			// AddBeforeDestroyAction(CharmDestroyResponse);

			AddIfTheCardThatThisCardIsNextToLeavesPlayMoveItToYourHandTrigger();
		}

		public override IEnumerator DeterminePlayLocation(
			List<MoveCardDestination> storedResults,
			bool isPutIntoPlay,
			List<IDecision> decisionSources,
			Location overridePlayArea = null,
			LinqTurnTakerCriteria additionalTurnTakerCriteria = null
		)
		{
			//When this card enters play, put it next to a hero
			IEnumerator selectHeroCR = SelectCardThisCardWillMoveNextTo(
				new LinqCardCriteria(
					(Card c) => c.IsHeroCharacterCard && !c.IsIncapacitatedOrOutOfGame,
					"hero character"
				),
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

		protected Card CharmedHero()
		{
			// need to be prepared for both SW Sentinels AND Guise
			Card cardToCheck = GetCardThisCardIsNextTo();
			if (cardToCheck == null)
			{
				cardToCheck = base.Card.Location.OwnerTurnTaker.CharacterCard;
			}
			return cardToCheck;
		}

		public override IEnumerable<Power> AskIfContributesPowersToCardController(CardController cc)
		{
			// this defines what displays in a green box in the UI
			if (cc.Card == CharmedHero())
			{
				//If this card is next to a hero, they have this power
				return new Power[1]
				{
					new Power(
						cc.HeroTurnTakerController,
						cc,
						$"Destroy {base.Card.Title}.",
						GameController.DestroyCard(
							cc.HeroTurnTakerController,
							base.Card,
							cardSource: GetCardSource()
						),
						0,
						null,
						GetCardSource()
					)
				};
			}
			return null;
		}

		protected abstract IEnumerator CharmDestroyResponse(GameAction ga);

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