using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Starblade
{
	public class MakeTheNameCardController : CardController
	{
		/*
		 * you may shuffle your trash into your deck.
		 * 
		 * reveal cards from the top of your deck until 1 construct card and 1 postura card have been revealed.
		 * you may put each of those cards either into your hand or into play.
		 * discard the other revealed cards.
		 * 
		 * if no cards entered play this way, you may play a card.
		 */

		private bool _foundPostura;
		private bool _foundConstruct;

		public MakeTheNameCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// you may shuffle your trash into your deck.
			List<YesNoCardDecision> storedYesNoResults = new List<YesNoCardDecision>();
			IEnumerator askTrashCR = GameController.MakeYesNoCardDecision(
				DecisionMaker,
				SelectionType.ShuffleTrashIntoDeck,
				this.Card,
				storedResults: storedYesNoResults,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(askTrashCR);
			}
			else
			{
				GameController.ExhaustCoroutine(askTrashCR);
			}

			if (DidPlayerAnswerYes(storedYesNoResults))
			{
				// Shuffle trash into deck
				IEnumerator shuffleCR = GameController.ShuffleTrashIntoDeck(
					this.TurnTakerController,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(shuffleCR);
				}
				else
				{
					GameController.ExhaustCoroutine(shuffleCR);
				}
			}

			// reveal cards from the top of your deck until 1 construct card and 1 postura card have been revealed.
			_foundPostura = false;
			_foundConstruct = false;

			// discard the other revealed cards.
			List<RevealCardsAction> revealedCards = new List<RevealCardsAction>();
			IEnumerator revealCR = GameController.RevealCards(
				this.TurnTakerController,
				this.TurnTaker.Deck,
				(Card c) =>
				{
					if (!_foundPostura && c.DoKeywordsContain("postura"))
					{
						_foundPostura = true;
						return true;
					}
					if (!_foundConstruct && c.DoKeywordsContain("construct"))
					{
						_foundConstruct = true;
						return true;
					}
					return false;
				},
				2,
				revealedCards,
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

			List<MoveCardAction> movedCards = new List<MoveCardAction>();
			List<Card> workingCards = new List<Card>();
			if (_foundPostura)
			{
				workingCards.Add(GetRevealedCards(revealedCards).Where(
					(Card c) => c.DoKeywordsContain("postura")
				).FirstOrDefault());
			}
			if (_foundConstruct)
			{
				workingCards.Add(GetRevealedCards(revealedCards).Where(
					(Card c) => c.DoKeywordsContain("construct")
				).FirstOrDefault());
			}
			List<Card> otherCards = GetRevealedCards(revealedCards).Where(c => !workingCards.Contains(c)).ToList();
			if (workingCards.Any())
			{
				// you may put each of those cards either into your hand or into play.
				IEnumerator moveCardsCR = GameController.SelectCardsFromLocationAndMoveThem(
					DecisionMaker,
					this.TurnTaker.Revealed,
					2,
					2,
					new LinqCardCriteria((Card c) => workingCards.Contains(c), "revealed"),
					new MoveCardDestination[]
					{
						new MoveCardDestination(this.HeroTurnTaker.Hand),
						new MoveCardDestination(this.HeroTurnTaker.PlayArea)
					},
					true,
					storedResultsMove: movedCards,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(moveCardsCR);
				}
				else
				{
					GameController.ExhaustCoroutine(moveCardsCR);
				}
			}

			if (otherCards.Any())
			{
				IEnumerator cleanupCR = GameController.MoveCards(
					DecisionMaker,
					otherCards,
					this.TurnTaker.Trash,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(cleanupCR);
				}
				else
				{
					GameController.ExhaustCoroutine(cleanupCR);
				}
			}


			/*
			// discard the other revealed cards.
			IEnumerator revealCR = RevealCards_SelectSome_MoveThem_DiscardTheRest(
				this.HeroTurnTakerController,
				this.TurnTakerController,
				this.TurnTaker.Deck,
				(Card c) =>
				{
					if (!_foundPostura && c.DoKeywordsContain("postura"))
					{
						_foundPostura = true;
						return true;
					}
					if (!_foundConstruct && c.DoKeywordsContain("construct"))
					{
						_foundConstruct = true;
						return true;
					}
					return false;
				},
				2,
				2,
				// you may put each of those cards either into your hand or into play.
				true,
				true,
				true,
				"1 construct card and 1 postura card"
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(revealCR);
			}
			else
			{
				GameController.ExhaustCoroutine(revealCR);
			}

			// if no cards entered play this way, you may play a card.
			if (!Journal.PlayCardEntriesThisTurn().Any(
				(PlayCardJournalEntry p) => p.PlayIndex > this.Card.PlayIndex && p.CardSource == this.Card
			))*/

			if (!movedCards.Where((MoveCardAction mca) => mca.Destination == this.TurnTaker.PlayArea).Any())
			{
				IEnumerator playCardCR = SelectAndPlayCardFromHand(this.HeroTurnTakerController);
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(playCardCR);
				}
				else
				{
					GameController.ExhaustCoroutine(playCardCR);
				}
			}

			yield break;
		}
	}
}