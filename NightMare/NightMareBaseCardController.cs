using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.NightMare
{
	public abstract class NightMareBaseCardController : CardController
	{
		public NightMareBaseCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddStartOfGameTriggers()
		{
			/* do we need this one? let's find out
			AddTrigger(
				(DiscardCardAction d) => d.CardToDiscard == Card,
				DiscardResponse,
				TriggerType.Other,
				TriggerTiming.Before,
				outOfPlayTrigger: true
			);
			*/

			AddTrigger(
				(MoveCardAction m) =>
					m.CardToMove == Card
					&& m.Destination == HeroTurnTaker.Trash
					&& (m.Origin.IsHand || m.Origin.IsDeck || m.Origin.IsRevealed)
					&& m.IsDiscard && m.CanChangeDestination
					&& !(
						FindCard("Gust") != null
						&& FindCard("Gust").Owner.Identifier == "CadaverTeam"
						&& FindCard("Gust").IsInPlayAndNotUnderCard
					),
				DiscardWrapper,
				TriggerType.Other,
				// TriggerTiming.Before, // some bugs from this
				TriggerTiming.After, // gonna try this instead
				outOfPlayTrigger: true
			);

			base.AddStartOfGameTriggers();
		}

		private IEnumerator DiscardWrapper(GameAction ga)
		{
			IEnumerator messageCR = GameController.SendMessageAction(
				$"Discarding {this.Card.Title} activates its discard text.",
				Priority.Medium,
				GetCardSource(),
				showCardSource: true
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(messageCR);
				yield return GameController.StartCoroutine(DiscardResponse(ga));
			}
			else
			{
				GameController.ExhaustCoroutine(messageCR);
				GameController.ExhaustCoroutine(DiscardResponse(ga));
			}

			yield break;
		}

		protected abstract IEnumerator DiscardResponse(GameAction ga);
	}
}