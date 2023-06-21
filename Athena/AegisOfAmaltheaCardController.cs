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
		 * Whenever exactly 1 Damage would be dealt to {Athena}, prevent that Damage.
		 * Damage dealt to {Athena} cannot be increased.
		 */

		public AegisOfAmaltheaCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// Whenever exactly 1 Damage would be dealt to {Athena}, prevent that Damage.
			AddPreventDamageTrigger(
				(DealDamageAction dd) => dd.Target == this.CharacterCard && dd.Amount == 1,
				isPreventEffect: true
			);

			// Damage dealt to {Athena} cannot be increased.
			AddTrigger(
				(DealDamageAction dd) => dd.Target == this.CharacterCard,
				(DealDamageAction dd) => GameController.MakeDamageUnincreasable(dd, GetCardSource()),
				TriggerType.MakeDamageUnincreasable,
				TriggerTiming.Before
			);

			base.AddTriggers();
		}

		public override bool CanOrderAffectOutcome(GameAction action)
		{
			if (action is DealDamageAction)
			{
				return (action as DealDamageAction).Target == this.CharacterCard;
			}
			return false;
		}
	}
}