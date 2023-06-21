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
	public class WarlordsOfBrocktonCardController : CardController
	{
		public WarlordsOfBrocktonCardController(Card card, TurnTakerController turnTakerController)
		: base(card, turnTakerController)
		{
			SpecialStringMaker.ShowNumberOfCardsUnderCard(this.Card, () => true);
			AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
		}

		public override void AddTriggers()
		{
			AddTrigger(
				(MoveCardAction m) => m.CardToMove == this.Card || (
					m.Origin == this.Card.UnderLocation
					&& !m.Destination.IsPlayArea
				),
				(MoveCardAction m) => CancelAction(m),
				TriggerType.CancelAction,
				TriggerTiming.Before
			);
			base.AddTriggers();
		}

		public override bool AskIfCardIsIndestructible(Card card)
		{
			if (card == this.Card || card.Location == this.Card.UnderLocation)
			{
				return true;
			}

			return false;
		}
	}
}
