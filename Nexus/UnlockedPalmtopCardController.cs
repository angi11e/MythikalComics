using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Nexus
{
	public class UnlockedPalmtopCardController : CardController
	{
		/*
		 * whenever one of your non-limited cards is destroyed, you may discard a card.
		 * if you do, move the destroyed card into your hand.
		 * 
		 * POWER
		 * reveal the top card of each deck, then replace it.
		 * select a deck and either discard or put into play its top card.
		 * if a card was put into play this way, {Nexus} deals herself 2 irreducible psychic damage.
		 */

		private bool _playedCard = false;

		public UnlockedPalmtopCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// whenever one of your non-limited cards is destroyed...
			AddTrigger(
				(DestroyCardAction d) => d.WasCardDestroyed
					&& !d.CardToDestroy.Card.IsLimited
					&& d.CardToDestroy.Card.Owner == this.TurnTaker,
				RecycleResponse,
				new TriggerType[] { TriggerType.DiscardCard, TriggerType.MoveCard },
				TriggerTiming.After
			);

			base.AddTriggers();
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int damageNumeral = GetPowerNumeral(0, 2);
			_playedCard = false;

			// reveal the top card of each deck, then replace it.
			List<Location> allDecks = new List<Location>();
			foreach (TurnTaker tt in Game.TurnTakers)
			{
				if (tt.IsIncapacitatedOrOutOfGame) continue;
				if (tt.Deck.IsRealDeck && GameController.IsLocationVisibleToSource(tt.Deck, GetCardSource()))
				{
					allDecks.Add(tt.Deck);
				}
				allDecks = allDecks.Concat(tt.SubDecks.Where(l => l.IsRealDeck
					&& GameController.IsLocationVisibleToSource(l, GetCardSource()))).ToList();
			}

			List<Card> revealedCards = new List<Card>();

			IEnumerator revealAllCR = GameController.SelectLocationsAndDoAction(
				DecisionMaker,
				SelectionType.RevealTopCardOfDeck,
				l => allDecks.Contains(l) && l.NumberOfCards > 0,
				(Location deck) => RevealTopCardAndReturn(
					deck,
					revealedCards
				),
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(revealAllCR);
			}
			else
			{
				GameController.ExhaustCoroutine(revealAllCR);
			}

			IEnumerable<Card> choices = allDecks.Where(deck => deck.NumberOfCards > 0).Select(deck => deck.TopCard);
			IEnumerable<CardController> cardsToFlip = choices.Except(revealedCards).Select(
				c => FindCardController(c)
			);

			IEnumerator flipCR = GameController.FlipCards(cardsToFlip, GetCardSource());
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(flipCR);
			}
			else
			{
				GameController.ExhaustCoroutine(flipCR);
			}

			// select a deck and either discard or put into play its top card.
			IEnumerator selectDiscardFlipCR = SelectAndDiscardWithFlipBack(
				DecisionMaker,
				choices,
				cardsToFlip
			);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selectDiscardFlipCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selectDiscardFlipCR);
			}

			// if a card was put into play this way...
			if (_playedCard)
			{
				// ...{Nexus} deals herself 2 irreducible psychic damage.
				IEnumerator selfDamageCR = DealDamage(
					this.CharacterCard,
					this.CharacterCard,
					2,
					DamageType.Psychic,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(selfDamageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(selfDamageCR);
				}
			}
			yield break;
		}

		private IEnumerator RevealTopCardAndReturn(Location deck, List<Card> revealedCards)
		{
			List<Card> cards = new List<Card>();
			List<RevealCardsAction> actionResult = new List<RevealCardsAction>();

			IEnumerator revealCR = GameController.RevealCards(
				DecisionMaker,
				deck,
				1,
				cards,
				revealedCardDisplay: RevealedCardDisplay.ShowRevealedCards,
				storedResultsAction: actionResult,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(revealCR);
			}
			else
			{
				GameController.ExhaustCoroutine(revealCR);
			}

			IEnumerator cleanUpCR = CleanupCardsAtLocations(
				new List<Location> { deck.OwnerTurnTaker.Revealed },
				deck,
				cardsInList: cards
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(cleanUpCR);
			}
			else
			{
				GameController.ExhaustCoroutine(cleanUpCR);
			}

			if (!actionResult.Any())
			{
				yield break;
			}

			RevealCardsAction revealAction = actionResult.First();
			if (!revealAction.RevealedCards.Any() && !revealAction.RemovedFromRevealedCards.Any())
			{
				yield break;
			}

			Card revealedCard = revealAction.RevealedCards.Any()
				? revealAction.RevealedCards.First()
				: revealAction.RemovedFromRevealedCards.First();
			revealedCards.Add(revealedCard);

			yield break;
		}

		private IEnumerator SelectAndDiscardWithFlipBack(
			HeroTurnTakerController hero,
			IEnumerable<Card> choices,
			IEnumerable<CardController> cardsToFlip)
		{
			SelectCardDecision selectCardDecision = new SelectCardDecision(
				GameController,
				hero,
				SelectionType.MoveCard,
				choices,
				cardSource: GetCardSource()
			);
			IEnumerator selectDiscardCR = GameController.SelectCardAndDoAction(
				selectCardDecision,
				(SelectCardDecision d) => FlipAndDiscardCard(
					hero,
					d.SelectedCard,
					cardsToFlip
				)
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selectDiscardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selectDiscardCR);
			}

			yield break;
		}

		private IEnumerator FlipAndDiscardCard(
			HeroTurnTakerController hero,
			Card selectedCard,
			IEnumerable<CardController> cardsToFlip)
		{
			IEnumerator flipCR = GameController.FlipCards(cardsToFlip, GetCardSource());
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(flipCR);
			}
			else
			{
				GameController.ExhaustCoroutine(flipCR);
			}

			List<MoveCardAction> movedCards = new List<MoveCardAction>();
			IEnumerator discardPlayCR = GameController.SelectLocationAndMoveCard(
				DecisionMaker,
				selectedCard,
				new MoveCardDestination[] {
					new MoveCardDestination( selectedCard.Owner.Trash ),
					new MoveCardDestination( selectedCard.Owner.PlayArea )
				},
				isPutIntoPlay: true,
				storedResults: movedCards,
				cardSource: GetCardSource()
			);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(discardPlayCR);
			}
			else
			{
				GameController.ExhaustCoroutine(discardPlayCR);
			}

			MoveCardAction theCard = movedCards.FirstOrDefault();
			if (theCard != null && theCard.Destination == theCard.CardToMove.Owner.PlayArea)
			{
				_playedCard = true;
			}

			yield break;
		}

		private IEnumerator RecycleResponse(DestroyCardAction dca)
		{
			// ...you may discard a card.
			List<DiscardCardAction> storedCards = new List<DiscardCardAction>();
			IEnumerator discardCR = SelectAndDiscardCards(
				HeroTurnTakerController,
				1,
				optional: true,
				null,
				storedCards
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(discardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(discardCR);
			}

			// if you do...
			if (DidDiscardCards(storedCards, 1))
			{
				// ...move the destroyed card into your hand.
				dca.PostDestroyDestinationCanBeChanged = false;
				dca.AddAfterDestroyedAction(delegate
				{
					return GameController.MoveCard(
						TurnTakerController,
						dca.CardToDestroy.Card,
						this.HeroTurnTaker.Hand,
						cardSource: GetCardSource()
					);
				}, this);

				/* my old non-working code
				IEnumerator moveCR = GameController.MoveCard(
					TurnTakerController,
					dca.CardToDestroy.Card,
					this.HeroTurnTaker.Hand,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(moveCR);
				}
				else
				{
					GameController.ExhaustCoroutine(moveCR);
				}
				*/
			}

			yield break;
		}
	}
}