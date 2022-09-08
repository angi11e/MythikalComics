using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.PecosBill
{
	public class TallTaleJusticeCardController : PecosBillBaseCardController
	{
		/*
		 * If there are no [u]folk[/u] cards in play, you may move one from your hand or trash into play.
		 * 
		 * If no [u]hyperbole[/u] cards in your hand refer to a target in play,
		 * search your trash and deck for a [u]hyperbole[/u] card and put it in your hand.
		 * If you searched your deck, shuffle your deck.
		 * 
		 * You may play a [u]hyperbole[/u] card.
		 * 
		 * You may activate a [u]tall tale[/u] text.
		 */

		public TallTaleJusticeCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			SpecialStringMaker.ShowListOfCardsInPlay(IsFolkCriteria());
			SpecialStringMaker.ShowListOfCardsAtLocation(this.HeroTurnTaker.Hand, IsHyperboleCriteria());
		}

		public override IEnumerator Play()
		{
			// If there are no [u]folk[/u] cards in play...
			if (!FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && IsFolk(c)).Any())
			{
				// ...you may move one from your hand or trash into play.
				List<Function> list = new List<Function>()
				{
					new Function(
						DecisionMaker,
						"Move a folk card from your hand",
						SelectionType.MoveCardToTrash,
						() => GameController.SelectCardFromLocationAndMoveIt(
							DecisionMaker,
							this.HeroTurnTaker.Hand,
							IsFolkCriteria(),
							new MoveCardDestination(this.TurnTaker.PlayArea).ToEnumerable(),
							isPutIntoPlay: true,
							cardSource: GetCardSource()
						),
						this.HeroTurnTaker.Hand.Cards.Where((Card c) => IsFolk(c)).Any()
					),
					new Function(
						DecisionMaker,
						"Move a folk card from your trash",
						SelectionType.MoveCardToTrash,
						() => GameController.SelectCardFromLocationAndMoveIt(
							DecisionMaker,
							this.TurnTaker.Trash,
							IsFolkCriteria(),
							new MoveCardDestination(this.TurnTaker.PlayArea).ToEnumerable(),
							isPutIntoPlay: true,
							cardSource: GetCardSource()
						),
						this.TurnTaker.Trash.Cards.Where((Card c) => IsFolk(c)).Any()
					)
				};

				SelectFunctionDecision selectFunction = new SelectFunctionDecision(
					GameController,
					DecisionMaker,
					list,
					optional: true,
					noSelectableFunctionMessage: "There are no folk cards in your hand or trash.",
					cardSource: GetCardSource()
				);
				IEnumerator selectCR = GameController.SelectAndPerformFunction(selectFunction);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(selectCR);
				}
				else
				{
					GameController.ExhaustCoroutine(selectCR);
				}
			}

			// If no [u]hyperbole[/u] cards in your hand refer to a target in play...
			if (!PlayableHyperboles())
			{
				// ...search your trash and deck for a [u]hyperbole[/u] card and put it in your hand.
				// If you searched your deck, shuffle your deck.
				IEnumerator discoverHypeCR = SearchForCards(
					HeroTurnTakerController,
					searchDeck: true,
					searchTrash: true,
					1,
					1,
					IsHyperboleCriteria(),
					putIntoPlay: false,
					putInHand: true,
					putOnDeck: false
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(discoverHypeCR);
				}
				else
				{
					GameController.ExhaustCoroutine(discoverHypeCR);
				}
			}

			// You may play a [u]hyperbole[/u] card.
			IEnumerator playHypeCR = SelectAndPlayCardFromHand(
				this.HeroTurnTakerController,
				cardCriteria: IsHyperboleCriteria()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(playHypeCR);
			}
			else
			{
				GameController.ExhaustCoroutine(playHypeCR);
			}

			// You may activate a [u]tall tale[/u] text.
			IEnumerator activateCR = GameController.SelectAndActivateAbility(
				DecisionMaker,
				"tall tale",
				optional: true,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(activateCR);
			}
			else
			{
				GameController.ExhaustCoroutine(activateCR);
			}

			yield break;
		}

		private bool PlayableHyperboles()
		{
			if (!this.HeroTurnTaker.HasCardsInHand)
			{
				return false;
			}

			if (FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && c.Identifier == "LoyalLightning").Any())
			{
				if (FindCardsWhere(
					(Card c) => c.IsInLocation(this.HeroTurnTaker.Hand)
					&& (c.Identifier == "Rustlin" || c.Identifier == "WidowMaker")
				).Any())
				{
					return true;
				}
			}

			if (FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && c.Identifier == "ShakeTheSnake").Any())
			{
				if (FindCardsWhere(
					(Card c) => c.IsInLocation(this.HeroTurnTaker.Hand)
					&& (c.Identifier == "ShakeEmUp" || c.Identifier == "UnraveledShake")
				).Any())
				{
					return true;
				}
			}

			if (FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && c.Identifier == "TamedTwister").Any())
			{
				if (FindCardsWhere(
					(Card c) => c.IsInLocation(this.HeroTurnTaker.Hand)
					&& (c.Identifier == "DunGetTwisted" || c.Identifier == "TwisterOfFate")
				).Any())
				{
					return true;
				}
			}

			return false;
		}
	}
}