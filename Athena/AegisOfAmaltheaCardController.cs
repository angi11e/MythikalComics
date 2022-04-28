using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Athena
{
	public class AegisOfAmaltheaCardController : AthenaBaseCardController
	{
		/*
		 * Reduce Damage dealt to this card by 1.
		 * Whenever a Hero Target in your play area would be dealt Damage, you may redirect it to this card.
		 */

		public AegisOfAmaltheaCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// Reduce Damage dealt to this card by 1.
			AddReduceDamageTrigger((Card c) => c == base.Card, 1);

			// Whenever a Hero Target in your play area would be dealt Damage, you may redirect it to this card.
			AddRedirectDamageTrigger(
				(DealDamageAction dd) =>
					dd.Target != base.Card
					&& dd.Target.IsHero
					&& dd.Target.Location.IsPlayAreaOf(base.TurnTaker),
				() => base.Card,
				optional: true
			);

			base.AddTriggers();
		}
	}
}