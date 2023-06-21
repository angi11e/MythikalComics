using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Supplicate
{
	public class TogalStrideCardController : SupplicateBaseCardController
	{
		/*
		 * you may move an environment card in play to the bottom of the environment deck.
		 * 
		 * reveal the top 3 cards of the environment deck.
		 * replace one, play one, and discard the rest.
		 */

		public TogalStrideCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// you may move an environment card in play to the bottom of the environment deck.
			SelectCardDecision selectCard = new SelectCardDecision(
				GameController,
				this.HeroTurnTakerController,
				SelectionType.MoveCardOnBottomOfDeck,
				Game.OrderedCardsInPlay,
				true,
				additionalCriteria: (Card c) => c.IsEnvironment,
				cardSource: GetCardSource()
			);
			IEnumerator selectCardCR = GameController.SelectCardAndDoAction(
				selectCard,
				BuryCardResponse
			);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selectCardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selectCardCR);
			}

			// reveal the top 3 cards of the environment deck.
			TurnTaker env = FindEnvironment().TurnTaker;
			IEnumerator revealCR = RevealCardsFromDeckToMoveToOrderedDestinations(
				DecisionMaker,
				env.Deck,
				new List<MoveCardDestination>
				{
					// replace one, play one, and discard the rest.
					new MoveCardDestination(env.Deck),
					new MoveCardDestination(env.PlayArea),
					new MoveCardDestination(env.Trash)
				},
				numberOfCardsToReveal: 3
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(revealCR);
			}
			else
			{
				GameController.ExhaustCoroutine(revealCR);
			}
			yield break;
		}

		private IEnumerator BuryCardResponse(SelectCardDecision scd)
		{
			if (scd == null || scd.SelectedCard == null)
			{
				yield break;
			}

			IEnumerator moveCardCR = GameController.MoveCard(
				DecisionMaker,
				scd.SelectedCard,
				scd.SelectedCard.Owner.Deck,
				true,
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
	}
}