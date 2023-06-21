using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.WhatsHerFace
{
	public class LostAndForgottenCardController : RecallBaseCardController
	{
		/*
		 * Play this card next to a non-character card.
		 * If that card is destroyed, remove it from the game instead, then remove this card from the game.
		 * If that card leaves play in any other way, return this card to your hand.
		 */

		public LostAndForgottenCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		// Play this card next to a non-character card.
		protected override LinqCardCriteria CustomCriteria => new LinqCardCriteria(
			(Card c) => c.IsInPlayAndHasGameText && !c.IsCharacter,
			"non-character"
		);

		public override void AddTriggers()
		{
			// If that card is destroyed, remove it from the game instead, then remove this card from the game.
			AddTrigger(
				(DestroyCardAction dca) => dca.CardToDestroy.Card == GetCardThisCardIsNextTo(),
				DestroyRemoveResponse,
				TriggerType.RemoveFromGame,
				TriggerTiming.Before,
				priority: TriggerPriority.High
			);

			// it's possible for this to cause an unwinnable state
			AddTrigger(
				(GameAction a) => !(a is MessageAction) && !GameController.IsGameWinnable(),
				UnwinnableGameOver,
				TriggerType.Hidden,
				TriggerTiming.After
			);

			// If that card leaves play in any other way, return this card to your hand.
			AddIfTheCardThatThisCardIsNextToLeavesPlayMoveItToYourHandTrigger();

			base.AddTriggers();
		}

		private IEnumerator UnwinnableGameOver(GameAction a)
		{
			IEnumerator messageCR = GameController.SendMessageAction(
				"Victory is now impossible for the heroes, as the fog of forgotten memories takes over...",
				Priority.Critical,
				GetCardSource()
			);
			IEnumerator endGameCR = GameController.GameOver(
				EndingResult.AlternateDefeat,
				"The heroes have doomed themselves to be forgotten.",
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(messageCR);
				yield return GameController.StartCoroutine(endGameCR);
			}
			else
			{
				GameController.ExhaustCoroutine(messageCR);
				GameController.ExhaustCoroutine(endGameCR);
			}
		}

		private IEnumerator DestroyRemoveResponse(DestroyCardAction dca)
		{
			if (dca.CardToDestroy.CanBeMovedOutOfGame)
			{
				IEnumerator cancelCR = CancelAction(dca, false);
				IEnumerator moveOtherCardCR = GameController.MoveCard(
					dca.CardToDestroy.TurnTakerController,
					dca.CardToDestroy.Card,
					dca.CardToDestroy.TurnTaker.OutOfGame,
					cardSource: GetCardSource()
				);
				IEnumerator moveThisCardCR = GameController.MoveCard(
					TurnTakerController,
					this.Card,
					TurnTaker.OutOfGame,
					cardSource: GetCardSource()
				);
				IEnumerator messageCR = GameController.SendMessageAction(
					this.Card.Title + " removes itself and " + dca.CardToDestroy.Card.Title + " from the game!",
					Priority.Medium,
					GetCardSource(),
					new Card[1] {dca.CardToDestroy.Card},
					true
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(cancelCR);
					yield return GameController.StartCoroutine(moveOtherCardCR);
					yield return GameController.StartCoroutine(moveThisCardCR);
					yield return GameController.StartCoroutine(messageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(cancelCR);
					GameController.ExhaustCoroutine(moveOtherCardCR);
					GameController.ExhaustCoroutine(moveThisCardCR);
					GameController.ExhaustCoroutine(messageCR);
				}
			}
			GameController.RemoveInhibitorException(dca.CardToDestroy);

			yield break;
		}
	}
}