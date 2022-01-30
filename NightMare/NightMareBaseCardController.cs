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
					&& m.IsDiscard && m.CanChangeDestination,
				DiscardResponse,
				TriggerType.Other,
				TriggerTiming.Before,
				outOfPlayTrigger: true
			);

			base.AddStartOfGameTriggers();
		}

		protected abstract IEnumerator DiscardResponse(GameAction ga);
	}
}