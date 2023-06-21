using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Supplicate
{
	public abstract class YaojingBaseCardController : SupplicateBaseCardController
	{
		public YaojingBaseCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			base.AddTriggers();

			// at the start of your turn
			// this card deals itself or {Supplicate} 2 irreducible psychic damage.
			AddDealDamageAtStartOfTurnTrigger(
				this.TurnTaker,
				this.Card,
				(Card c) => c == this.Card || c == this.CharacterCard,
				TargetType.SelectTarget,
				2,
				DamageType.Psychic,
				true
			);
		}
	}
}