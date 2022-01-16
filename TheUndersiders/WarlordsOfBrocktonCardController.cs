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
			base.SpecialStringMaker.ShowNumberOfCardsUnderCard(base.Card, () => true);
			AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
		}

		public override void AddTriggers()
		{
			AddTrigger(
				(MoveCardAction m) => m.CardToMove == base.Card || (
					m.Origin == base.Card.UnderLocation &&
					m.Destination != base.TurnTaker.PlayArea
				),
				(MoveCardAction m) => CancelAction(m),
				TriggerType.CancelAction,
				TriggerTiming.Before
			);
			base.AddTriggers();
		}

		public override bool AskIfCardIsIndestructible(Card card)
		{
			if (card == base.Card || card.Location == base.Card.UnderLocation)
			{
				return true;
			}

			return false;
		}
	}
}
