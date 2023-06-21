using System;
using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ArtifactComics.Chronan;

namespace Angille.Chronan
{
	public class MythikalChronanCharacterCardController : HeroCharacterCardController
	{
		public MythikalChronanCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int equipNumeral = GetPowerNumeral(0, 1);
			int drawNumeral = GetPowerNumeral(1, 2);

			// Play or destroy 1 equipment card.
			List<Function> functionList = new List<Function>();
			List<DestroyCardAction> destroyedCards = new List<DestroyCardAction>();

			// play...
			functionList.Add(
				new Function(
					DecisionMaker,
					$"play {equipNumeral} equipment {equipNumeral.ToString_CardOrCards()}",
					SelectionType.PlayCard,
					() => SelectAndPlayCardsFromHand(
						DecisionMaker,
						equipNumeral,
						false,
						equipNumeral,
						new LinqCardCriteria((Card c) => IsEquipment(c))
					),
					this.HeroTurnTaker.Hand.Cards.Any((Card c) => IsEquipment(c)),
					"no equipment cards in play to destroy"
				)
			);

			// destroy...
			functionList.Add(
				new Function(
					DecisionMaker,
					$"destroy {equipNumeral} equipment {equipNumeral.ToString_CardOrCards()}",
					SelectionType.DestroyCard,
					() => GameController.SelectAndDestroyCards(
						HeroTurnTakerController,
						new LinqCardCriteria((Card c) => IsEquipment(c) && c.IsInPlayAndHasGameText),
						equipNumeral,
						false,
						equipNumeral,
						storedResultsAction: destroyedCards,
						cardSource: GetCardSource()
					),
					FindCardsWhere(c => c.IsInPlay && IsEquipment(c)).Any(),
					"no equipment cards in hand to play"
				)
			);

			SelectFunctionDecision selectFunction = new SelectFunctionDecision(
				GameController,
				DecisionMaker,
				functionList,
				false,
				noSelectableFunctionMessage: "no equipment cards in hand or in play",
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

			// If you destroyed a card this way...
			if (destroyedCards.Any())
			{
				// ...that card's owner draws 2 cards.
				for (int i = 0; i < destroyedCards.Count; i++)
				{
					if (destroyedCards[i].WasCardDestroyed && IsHero(destroyedCards[i].CardToDestroy.Card))
					{
						IEnumerator drawCR = GameController.DrawCards(
							destroyedCards[i].CardToDestroy.HeroTurnTakerController,
							drawNumeral,
							cardSource: GetCardSource()
						);

						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(drawCR);
						}
						else
						{
							GameController.ExhaustCoroutine(drawCR);
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
					// One player may draw a card.
					IEnumerator drawCardCR = GameController.SelectHeroToDrawCard(
						DecisionMaker,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(drawCardCR);
					}
					else
					{
						GameController.ExhaustCoroutine(drawCardCR);
					}
					break;
				case 1:
					// Up to two equipment cards may be played now.
					IEnumerator equipsCR = GameController.SelectTurnTakersAndDoAction(
						DecisionMaker,
						new LinqTurnTakerCriteria((TurnTaker tt) => !tt.IsIncapacitatedOrOutOfGame && IsHero(tt)),
						SelectionType.PlayCard,
						(TurnTaker tt) => SelectAndPlayCardFromHand(
							FindHeroTurnTakerController(tt.ToHero()),
							cardCriteria: new LinqCardCriteria((Card c) => IsEquipment(c))
						),
						2,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(equipsCR);
					}
					else
					{
						GameController.ExhaustCoroutine(equipsCR);
					}
					break;
				case 2:
					// One player takes a full turn now, then remove this card from the game.
					List<SelectTurnTakerDecision> storedTurnTaker = new List<SelectTurnTakerDecision>();
					IEnumerator selectCR = GameController.SelectHeroTurnTaker(
						DecisionMaker,
						SelectionType.TakeFullTurnNow,
						optional: false,
						allowAutoDecide: false,
						storedTurnTaker,
						new LinqTurnTakerCriteria((TurnTaker tt) => !tt.IsIncapacitatedOrOutOfGame && IsHero(tt)),
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(selectCR);
					}
					else
					{
						GameController.ExhaustCoroutine(selectCR);
					}

					TurnTaker selectedTurnTaker = GetSelectedTurnTaker(storedTurnTaker);
					if (selectedTurnTaker != null)
					{
						IEnumerator fullTurnCR = TakeAFullTurnNow(FindHeroTurnTakerController(selectedTurnTaker.ToHero()));
						IEnumerator messageCR = GameController.SendMessageAction(
							this.Card.Title + " is removed from the game.",
							Priority.Medium,
							GetCardSource(),
							showCardSource: true
						);
						IEnumerator removeCR = GameController.MoveCard(
							TurnTakerController,
							this.Card,
							this.TurnTaker.OutOfGame,
							cardSource: GetCardSource()
						);

						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(fullTurnCR);
							yield return GameController.StartCoroutine(messageCR);
							yield return GameController.StartCoroutine(removeCR);
						}
						else
						{
							GameController.ExhaustCoroutine(fullTurnCR);
							GameController.ExhaustCoroutine(messageCR);
							GameController.ExhaustCoroutine(removeCR);
						}
					}
					break;
			}
			yield break;
		}
	}
}