using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra;

namespace Angille.Theurgy
{
	public class TheurgyFateweaverCharacterCardController : TheurgyBaseCharacterCardController
	{
		public TheurgyFateweaverCharacterCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
		}

		public override IEnumerator UsePower(int index = 0)
		{
			// Choose a target.
			//  If they're a hero target, Theurgy deals them 2 psychic damage.
			//  If they're a villain target, they regain 2 hp.
			//  If they're an environment target, destroy a charm card.
			// Reveal the top card of the target's deck.
			//  Either replace it, discard it, or put it into play.

			int damageNumeral = GetPowerNumeral(0, 2);
			int healNumeral = GetPowerNumeral(1, 2);
			List<Function> functionList = new List<Function>();

			// choose a target
			List<SelectTargetDecision> selectedTarget = new List<SelectTargetDecision>();
			IEnumerable<Card> choices = FindCardsWhere(
				new LinqCardCriteria((Card c) => c.IsTarget && c.IsInPlayAndHasGameText)
			);
			IEnumerator selectTargetCR = GameController.SelectTargetAndStoreResults(
				DecisionMaker,
				choices,
				selectedTarget,
				selectionType: SelectionType.RevealTopCardOfDeck,
				cardSource: GetCardSource()
			);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selectTargetCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selectTargetCR);
			}

			if (selectedTarget != null && selectedTarget.Any())
			{
				SelectTargetDecision selectedTargetDecision = selectedTarget.FirstOrDefault();
				if (selectedTargetDecision != null && selectedTargetDecision.SelectedCard != null)
				{
					Card theCard = selectedTarget.FirstOrDefault().SelectedCard;
					// hero target? deal 2 psychic
					if (IsHeroTarget(theCard))
					{
						IEnumerator dealDamageCR = DealDamage(
							this.Card,
							theCard,
							damageNumeral,
							DamageType.Psychic,
							cardSource: GetCardSource()
						);

						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(dealDamageCR);
						}
						else
						{
							GameController.ExhaustCoroutine(dealDamageCR);
						}
					}

					// villain target? regain 2 hp
					else if (IsVillainTarget(theCard))
					{
						IEnumerator healTargetCR = GameController.GainHP(
							theCard,
							healNumeral,
							cardSource: GetCardSource()
						);
						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(healTargetCR);
						}
						else
						{
							GameController.ExhaustCoroutine(healTargetCR);
						}
					}

					// environment (or [other]) target? choose a charm card and destroy it
					else
					{
						IEnumerator destroyCR = GameController.SelectAndDestroyCard(
							DecisionMaker,
							IsCharmCriteria(),
							false,
							cardSource: GetCardSource()
						);

						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(destroyCR);
						}
						else
						{
							GameController.ExhaustCoroutine(destroyCR);
						}
					}

					// which deck was that? reveal the top card
					if (theCard.Owner.Deck.IsRealDeck)
					{
						List<Card> revealedCards = new List<Card>();
						IEnumerator revealCR = GameController.RevealCards(
							DecisionMaker,
							theCard.Owner.Deck,
							1,
							revealedCards,
							revealedCardDisplay: RevealedCardDisplay.ShowRevealedCards,
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

						Log.Debug("seamripper revealed cards: " + revealedCards.Count());
						if (revealedCards.Count() > 0)
						{
							Card revealedCard = revealedCards.First();
							CardController revealedController = FindCardController(revealedCard);

							// give choices of replace, discard, or play...
							List<MoveCardDestination> possibleDestinations = new List<MoveCardDestination>();
							possibleDestinations.Add(new MoveCardDestination(revealedCard.Owner.Deck));
							possibleDestinations.Add(revealedController.GetTrashDestination());
							if (GameController.CanPlayCard(revealedController, true) == CanPlayCardResult.CanPlay)
							{
								possibleDestinations.Add(new MoveCardDestination(revealedCard.Owner.PlayArea));
							}
							Log.Debug("seamripper destinations: " + possibleDestinations);

							// ...then ask and do so
							IEnumerator destinationCR = GameController.SelectLocationAndMoveCard(
								DecisionMaker,
								revealedCard,
								possibleDestinations,
								true,
								showOutput: true,
								responsibleTurnTaker: DecisionMaker.TurnTaker,
								isDiscardIfMovingToTrash: true,
								cardSource: GetCardSource()
							);

							if (UseUnityCoroutines)
							{
								yield return GameController.StartCoroutine(destinationCR);
							}
							else
							{
								GameController.ExhaustCoroutine(destinationCR);
							}
						}
					}
					else
					{
						IEnumerator notDeckCR = GameController.SendMessageAction(
							"This target has no parent deck to reveal from.",
							Priority.Low,
							GetCardSource()
						);

						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(notDeckCR);
						}
						else
						{
							GameController.ExhaustCoroutine(notDeckCR);
						}
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
					// One player may play a card.
					IEnumerator playCR = SelectHeroToPlayCard(DecisionMaker);
					
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(playCR);
					}
					else
					{
						GameController.ExhaustCoroutine(playCR);
					}
					break;
				
				case 1:
					// Reveal the top card of each deck, then replace it.
					// Select a deck and discard its top card.
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
					break;
				
				case 2:
					// Move any one non-character card in play to the top of its deck.
					List<SelectCardDecision> storedResults = new List<SelectCardDecision>();

					IEnumerator selectYeetCardCR = GameController.SelectCardAndStoreResults(
						DecisionMaker,
						SelectionType.MoveCardOnDeck,
						new LinqCardCriteria(
							(Card c) => c.IsInPlay
								&& !c.IsCharacter
								&& !GameController.IsCardIndestructible(c)
								&& !c.IsOneShot
								&& GameController.IsCardVisibleToCardSource(c, GetCardSource()),
							"non-indestructible non-character cards in play",
							useCardsSuffix: false
						),
						storedResults,
						optional: false,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(selectYeetCardCR);
					}
					else
					{
						GameController.ExhaustCoroutine(selectYeetCardCR);
					}

					SelectCardDecision selectCardDecision = storedResults.Where((SelectCardDecision d) => d.Completed).FirstOrDefault();
					if (selectCardDecision != null && selectCardDecision.SelectedCard != null)
					{
						IEnumerator YeetItCR = GameController.MoveCard(
							DecisionMaker,
							selectCardDecision.SelectedCard,
							selectCardDecision.SelectedCard.NativeDeck,
							cardSource: GetCardSource()
						);

						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(YeetItCR);
						}
						else
						{
							GameController.ExhaustCoroutine(YeetItCR);
						}
					}
					break;
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
			IEnumerable<CardController> cardsToFlip )
		{
			SelectCardDecision selectCardDecision = new SelectCardDecision(
				GameController,
				hero,
				SelectionType.DiscardFromDeck,
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

			IEnumerator discardCR = GameController.MoveCard(
				DecisionMaker,
				selectedCard,
				FindCardController(selectedCard).GetTrashDestination().Location,
				isDiscard: true,
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

			yield break;
		}
	}
}