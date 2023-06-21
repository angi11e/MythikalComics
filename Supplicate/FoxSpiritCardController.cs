using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Supplicate
{
	public class FoxSpiritCardController : YaojingBaseCardController
	{
		/*
		 * at the start of your turn,
		 * this card deals itself or {Supplicate} 2 irreducible psychic damage.
		 * 
		 * at the end of your turn, you may draw a card.
		 */

		public FoxSpiritCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			base.AddTriggers();

			// at the end of your turn, you may draw a card.
			AddEndOfTurnTrigger(
				(TurnTaker tt) => tt == this.TurnTaker,
				(PhaseChangeAction pca) => DrawCard(optional: true),
				TriggerType.DrawCard
			);
		}
	}
}