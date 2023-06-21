using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using System.Collections;
using Handelabra;

namespace Angille.TheUndersiders
{
	public class AdvancePlanningCardController : TheUndersidersBaseCardController
	{
		public AdvancePlanningCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
			SpecialStringMaker.ShowSpecialString(() => GetSpecialStringIcons("tattle", "bear"));
		}

		public override IEnumerator Play()
		{
			// Each player discards 1 card. Each player discards the top card and the bottom card of their deck.
			IEnumerator discardFromHandCR = GameController.EachPlayerDiscardsCards(
				1,
				1,
				cardSource: GetCardSource()
			);
			IEnumerator discardFromTopCR = GameController.DiscardTopCardsOfDecks(
				null,
				(Location l) => !l.OwnerTurnTaker.IsIncapacitatedOrOutOfGame && IsHero(l.OwnerTurnTaker),
				1,
				responsibleTurnTaker: this.TurnTaker,
				cardSource: GetCardSource()
			);
			IEnumerator discardFromBottomCR = GameController.MoveCards(
				null,
				new LinqCardCriteria((Card c) => c == c.Owner.Deck.BottomCard && IsHero(c), "bottom card"),
				(Card c) => c.Owner.Trash,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(discardFromHandCR);
				yield return GameController.StartCoroutine(discardFromTopCR);
				yield return GameController.StartCoroutine(discardFromHandCR);
			}
			else
			{
				GameController.ExhaustCoroutine(discardFromHandCR);
				GameController.ExhaustCoroutine(discardFromTopCR);
				GameController.ExhaustCoroutine(discardFromBottomCR);
			}

			// Tattle: Play the top card of the villain deck.
			if (IsEnabled("tattle"))
			{
				IEnumerator playVillainCR = PlayTheTopCardOfTheVillainDeckResponse(null);
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(playVillainCR);
				}
				else
				{
					GameController.ExhaustCoroutine(playVillainCR);
				}
			}

			// Bear: Destroy 1 environment card. Play the top card of the environment deck.
			if (IsEnabled("bear"))
			{
				IEnumerator destroyCR = GameController.SelectAndDestroyCard(
					DecisionMaker,
					new LinqCardCriteria((Card c) => c.IsEnvironment, "environment"),
					false,
					cardSource: GetCardSource()
				);
				IEnumerator playEnvironment = PlayTheTopCardOfTheEnvironmentDeckResponse(null);
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(destroyCR);
					yield return GameController.StartCoroutine(playEnvironment);
				}
				else
				{
					GameController.ExhaustCoroutine(destroyCR);
					GameController.ExhaustCoroutine(playEnvironment);
				}
			}
		}
	}
}
