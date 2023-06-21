using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Athena
{
	public class GlaukopisCardController : AspectBaseCardController
	{
		/*
		 * When this card enters play, destroy any other [u]aspect[/u] cards.
		 * 
		 * You may look at the top card of your deck at any time.
		 * 
		 * POWER
		 * Reveal the top 2 cards of any 2 decks.
		 *  Put them back on top of their decks in any order.
		 *  1 player may draw 1 card.
		 */

		public GlaukopisCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			// You may look at the top card of your deck at any time.
			SpecialStringMaker.ShowListOfCards(
				new LinqCardCriteria(
					(Card c) => c == this.Card.Location.OwnerTurnTaker.Deck.TopCard,
					"top card of deck",
					useCardsSuffix: false
				)
			).Condition = () => this.Card.IsInPlayAndHasGameText;
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int cardsNumeral = GetPowerNumeral(0, 2);
			int decksNumeral = GetPowerNumeral(0, 2);
			int playerNumeral = GetPowerNumeral(0, 1);
			int drawNumeral = GetPowerNumeral(0, 1);

			// Reveal the top 2 cards of any 2 decks.
			List<SelectLocationDecision> storedResults = new List<SelectLocationDecision>();
			IEnumerator deckSelectCR = SelectDecks(
				DecisionMaker,
				decksNumeral,
				SelectionType.RevealCardsFromDeck,
				(Location l) => true,
				storedResults
			);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(deckSelectCR);
			}
			else
			{
				GameController.ExhaustCoroutine(deckSelectCR);
			}

			IEnumerable<Location> decks = from l in storedResults
				where l.Completed && l.SelectedLocation.Location != null
				select l.SelectedLocation.Location;
			List<Card> storedCards = new List<Card>();
			for (int i = 0; i < decks.Count(); i++)
			{
				IEnumerator revealCR = GameController.RevealCards(
					base.TurnTakerController,
					decks.ElementAt(i),
					cardsNumeral,
					storedCards,
					revealedCardDisplay: RevealedCardDisplay.Message,
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
			}

			// Put them back on top of their decks in any order.
			while (storedCards.Count() > 0 && !GameController.IsGameOver)
			{
				List<SelectCardDecision> storedTop = new List<SelectCardDecision>();
				IEnumerator cardSelectCR = GameController.SelectCardAndStoreResults(
					DecisionMaker,
					SelectionType.MoveCardOnDeck,
					storedCards,
					storedTop,
					ignoreBattleZone: true,
					cardSource: GetCardSource()
				);
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(cardSelectCR);
				}
				else
				{
					GameController.ExhaustCoroutine(cardSelectCR);
				}

				Card topCard = GetSelectedCard(storedTop);
				if (topCard == null)
				{
					continue;
				}
				
				Location topCardNativeDeck = GetNativeDeck(topCard);
				Card otherCard = storedCards.Where(
					(Card c) => GetNativeDeck(c) == topCardNativeDeck && c != topCard
				).FirstOrDefault();

				if (otherCard != null)
				{
					Location nativeDeck = GetNativeDeck(otherCard);
					IEnumerator returnFirstCR = GameController.MoveCard(
						this.TurnTakerController,
						otherCard,
						nativeDeck,
						cardSource: GetCardSource()
					);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(returnFirstCR);
					}
					else
					{
						GameController.ExhaustCoroutine(returnFirstCR);
					}
					storedCards.Remove(otherCard);
				}

				IEnumerator returnCardCR = GameController.MoveCard(
					this.TurnTakerController,
					topCard,
					topCardNativeDeck,
					cardSource: GetCardSource()
				);
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(returnCardCR);
				}
				else
				{
					GameController.ExhaustCoroutine(returnCardCR);
				}
				storedCards.Remove(topCard);
			}

			for (int i = 0; i < decks.Count(); i++)
			{
				List<Location> list = new List<Location>();
				list.Add(decks.ElementAt(i).OwnerTurnTaker.Revealed);
				IEnumerator cleanCR = CleanupCardsAtLocations(
					list,
					decks.ElementAt(i),
					cardsInList: storedCards
				);
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(cleanCR);
				}
				else
				{
					GameController.ExhaustCoroutine(cleanCR);
				}
			}

			// 1 player may draw 1 card.
			IEnumerator drawCR = GameController.SelectHeroToDrawCards(
				DecisionMaker,
				drawNumeral,
				cardSource: GetCardSource()
			);
			for (int i = 0; i < playerNumeral; i++)
			{
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(drawCR);
				}
				else
				{
					GameController.ExhaustCoroutine(drawCR);
				}
			}

			yield break;
		}
	}
}