using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.Speedrunner
{
	public class RetroSpeedrunnerCharacterCardController : HeroCharacterCardController
	{
		public RetroSpeedrunnerCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator UsePower(int index = 0)
		{
			// put a one-shot card from a hero trash into play.
			List<SelectTurnTakerDecision> selectHero = new List<SelectTurnTakerDecision>();
			IEnumerator selectHeroCR = GameController.SelectHeroTurnTaker(
				HeroTurnTakerController,
				SelectionType.MoveCard,
				optional: false,
				allowAutoDecide: false,
				selectHero,
				new LinqTurnTakerCriteria(
					(TurnTaker tt) => tt.IsHero && !tt.ToHero().IsIncapacitatedOrOutOfGame,
					"active heroes"
				),
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selectHeroCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selectHeroCR);
			}

			if (selectHero.Count() > 0 && selectHero.FirstOrDefault().SelectedTurnTaker != null)
			{
				TurnTaker selectedTurnTaker = selectHero.FirstOrDefault().SelectedTurnTaker;
				List<SelectCardDecision> cardSelection = new List<SelectCardDecision>();
				MoveCardDestination[] possibleDestinations = new MoveCardDestination[1]
				{
					new MoveCardDestination(selectedTurnTaker.PlayArea)
				};

				IEnumerator selectCardCR = GameController.SelectCardFromLocationAndMoveIt(
					DecisionMaker,
					selectedTurnTaker.Trash,
					new LinqCardCriteria((Card card) => card.IsOneShot, "one-shot"),
					possibleDestinations,
					isPutIntoPlay: true,
					storedResults: cardSelection,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(selectCardCR);
				}
				else
				{
					GameController.ExhaustCoroutine(selectCardCR);
				}

				// when that card finishes resolving, move it to the bottom of its deck.
				if (cardSelection.Count() > 0)
				{
					IEnumerator finalMoveCR = GameController.MoveCard(
						this.TurnTakerController,
						cardSelection.FirstOrDefault().SelectedCard,
						selectedTurnTaker.Deck,
						true,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(finalMoveCR);
					}
					else
					{
						GameController.ExhaustCoroutine(finalMoveCR);
					}
				}
			}
			yield break;
		}

		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			switch (index)
			{
				case 0:
					// One player may draw or play a card.
					List<Function> choices = new List<Function>();
					choices.Add(new Function(
						DecisionMaker,
						"One player may draw a card",
						SelectionType.DrawCard,
						() => GameController.SelectHeroToDrawCard(
							DecisionMaker,
							cardSource: GetCardSource()
						),
						FindTurnTakersWhere(
							(TurnTaker tt) => tt.IsHero && CanDrawCards(FindHeroTurnTakerController(tt.ToHero()))
						).Count() > 0
					));
					choices.Add(new Function(
						DecisionMaker,
						"One player may play a card",
						SelectionType.PlayCard,
						() => SelectHeroToPlayCard(DecisionMaker),
						FindTurnTakersWhere(
							(TurnTaker tt) => tt.IsHero && CanPlayCards(FindHeroTurnTakerController(tt.ToHero()))
						).Count() > 0
					));

					SelectFunctionDecision selectFunction = new SelectFunctionDecision(
						GameController,
						DecisionMaker,
						choices,
						optional: false,
						noSelectableFunctionMessage: "There are no heroes who can play or draw cards.",
						cardSource: GetCardSource()
					);
					IEnumerator selectFunctionCR = GameController.SelectAndPerformFunction(selectFunction);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(selectFunctionCR);
					}
					else
					{
						GameController.ExhaustCoroutine(selectFunctionCR);
					}
					break;

				case 1:
					// One Player may take a card from their trash into their hand.
					List<SelectCardDecision> cardSelection = new List<SelectCardDecision>();
					IEnumerator selectTrashCR = GameController.SelectCardAndStoreResults(
						HeroTurnTakerController,
						SelectionType.MoveCardToHandFromTrash,
						new LinqCardCriteria(
							(Card c) => c.IsInTrash && c.Location.IsHero,
							"cards in a player's trash",
							useCardsSuffix: false
						),
						cardSelection,
						optional: false,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(selectTrashCR);
					}
					else
					{
						GameController.ExhaustCoroutine(selectTrashCR);
					}

					if (cardSelection.Any((SelectCardDecision d) => d.Completed && d.SelectedCard != null))
					{
						Card selectedCard = cardSelection.FirstOrDefault().SelectedCard;
						IEnumerator moveCardCR = GameController.MoveCard(
							TurnTakerController,
							selectedCard,
							selectedCard.Location.OwnerTurnTaker.ToHero().Hand,
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
					break;

				case 2:
					// One player discards 1 card.
					List<DiscardCardAction> discardSelection = new List<DiscardCardAction>();
					IEnumerator discardCR = GameController.SelectHeroToDiscardCard(
						DecisionMaker,
						optionalDiscardCard: false,
						storedResultsDiscard: discardSelection,
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

					if (!DidDiscardCards(discardSelection, 1))
					{
						break;
					}

					Card card = discardSelection.First().CardToDiscard;
					IEnumerator discardBenefitCR = null;
					MoveCardJournalEntry moveJournal = (
						from dcje in Journal.DiscardCardEntriesThisTurn() where dcje.Card == card select dcje
					).LastOrDefault();

					if (moveJournal != null)
					{
						if (moveJournal.DidCardHaveKeyword("one-shot"))
						{
							// If the discarded card was a one-shot, that player may draw 2 cards.
							discardBenefitCR = DrawCards(
								discardSelection.First().HeroTurnTakerController,
								2,
								optional: true
							);
						}
						else if (moveJournal.DidCardHaveKeyword("ongoing") || moveJournal.DidCardHaveKeyword("equipment"))
						{
							// If the discarded card was an ongoing or equipment card, 1 hero target regains 2 HP.
							discardBenefitCR = GameController.SelectAndGainHP(
								DecisionMaker,
								2,
								optional: false,
								(Card c) => c.IsHero,
								1,
								cardSource: GetCardSource()
							);
						}
					}

					if (discardBenefitCR != null)
					{
						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(discardBenefitCR);
						}
						else
						{
							GameController.ExhaustCoroutine(discardBenefitCR);
						}
					}
					break;
			}
			yield break;
		}
	}
}