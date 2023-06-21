using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.WhatsHerFace
{
	public class NotQuiteSureWhatThisDoesCardController : WhatsHerFaceBaseCardController
	{
		/*
		 * POWER:
		 * Play the top card of your deck.
		 *  If you played a [u]recall[/u] card, play the top card of your deck.
		 *  If you played an equipment card, move 1 card from your trash to the top of your deck.
		 *  If you played a one-shot, you may draw 1 card now.
		 */

		public NotQuiteSureWhatThisDoesCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int recoverNumeral = GetPowerNumeral(0, 1);
			int drawNumeral = GetPowerNumeral(1, 1);

			// Play the top card of your deck.
			List<PlayCardAction> playedCards = new List<PlayCardAction>();
			IEnumerator playTopCR = GameController.PlayTopCard(
				DecisionMaker,
				TurnTakerController,
				storedResults: playedCards,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(playTopCR);
			}
			else
			{
				GameController.ExhaustCoroutine(playTopCR);
			}

			Card whichCard = DidPlayCards(playedCards) ? playedCards.FirstOrDefault().CardToPlay : null;
			if (whichCard == null)
			{
				yield break;
			}

			// If you played a [u]recall[/u] card, play the top card of your deck.
			if (IsRecall(whichCard))
			{
				IEnumerator playNewTopCR = GameController.PlayTopCard(
					DecisionMaker,
					TurnTakerController,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(playNewTopCR);
				}
				else
				{
					GameController.ExhaustCoroutine(playNewTopCR);
				}
			}

			// If you played an equipment card, move 1 card from your trash to the top of your deck.
			if (IsEquipment(whichCard))
			{
				IEnumerator recoverCR = GameController.SelectCardsFromLocationAndMoveThem(
					DecisionMaker,
					TurnTaker.Trash,
					recoverNumeral,
					recoverNumeral,
					new LinqCardCriteria((Card c) => true),
					new List<MoveCardDestination> { new MoveCardDestination(TurnTaker.Deck) },
					cardSource: GetCardSource()
				);
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(recoverCR);
				}
				else
				{
					GameController.ExhaustCoroutine(recoverCR);
				}
			}

			// If you played a one-shot, you may draw 1 card now.
			if (whichCard.IsOneShot)
			{
				IEnumerator drawCR = GameController.DrawCards(
					DecisionMaker,
					drawNumeral
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

			yield break;
		}
	}
}