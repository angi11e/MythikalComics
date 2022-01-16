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
		}

		public override void AddTriggers()
		{
			base.AddTriggers();
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
				(Location l) => l.OwnerTurnTaker.IsHero && !l.OwnerTurnTaker.IsIncapacitatedOrOutOfGame,
				1,
				responsibleTurnTaker: base.TurnTaker,
				cardSource: GetCardSource()
			);
			IEnumerator discardFromBottomCR = GameController.MoveCards(
				null,
				new LinqCardCriteria((Card c) => c.IsHero && c == c.Owner.Deck.BottomCard, "bottom card"),
				(Card c) => c.Owner.Trash,
				cardSource: GetCardSource()
			);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(discardFromHandCR);
				yield return base.GameController.StartCoroutine(discardFromTopCR);
				yield return base.GameController.StartCoroutine(discardFromHandCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(discardFromHandCR);
				base.GameController.ExhaustCoroutine(discardFromTopCR);
				base.GameController.ExhaustCoroutine(discardFromBottomCR);
			}

			// Tattle: Play the top card of the villain deck.
			if (IsEnabled("tattle"))
			{
				IEnumerator playVillainCR = PlayTheTopCardOfTheVillainDeckResponse(null);
				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(playVillainCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(playVillainCR);
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
				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(destroyCR);
					yield return base.GameController.StartCoroutine(playEnvironment);
				}
				else
				{
					base.GameController.ExhaustCoroutine(destroyCR);
					base.GameController.ExhaustCoroutine(playEnvironment);
				}
			}
		}
	}
}
