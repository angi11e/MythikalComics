using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Supplicate
{
	public class TortoiseSpiritCardController : YaojingBaseCardController
	{
		/*
		 * at the start of your turn,
		 * this card deals itself or {Supplicate} 2 irreducible psychic damage.
		 * 
		 * reduce damage dealt to {Supplicate} and yaojing cards by 1.
		 * 
		 * Damage dealt to yaojing cards cannot be increased.
		 */

		public TortoiseSpiritCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			base.AddTriggers();

			// reduce damage dealt to {Supplicate} and yaojing cards by 1.
			AddReduceDamageTrigger(
				(Card c) => c == this.CharacterCard || IsYaojing(c),
				1
			);

			// Damage dealt to yaojing cards cannot be increased.
			AddTrigger(
				(DealDamageAction dd) => IsYaojing(dd.Target),
				(DealDamageAction dd) => GameController.MakeDamageUnincreasable(dd, GetCardSource()),
				TriggerType.MakeDamageUnincreasable,
				TriggerTiming.Before
			);
		}
	}
}