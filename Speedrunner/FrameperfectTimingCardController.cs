using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Speedrunner
{
	public class FrameperfectTimingCardController : SpeedrunnerBaseCardController
	{
		/*
		 * When {Speedrunner} would be dealt damage,
		 *  you may redirect that damage to the villain target with the lowest HP.
		 * If you do so, you may either increase it by 2, draw 1 card, or play 1 card.
		 * Then destroy this card.
		 * 
		 * POWER
		 * {Speedrunner} deals 1 target 1 irreducible melee damage.
		 *  This damage cannot be redirected.
		 */

		public FrameperfectTimingCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// When {Speedrunner} would be dealt damage, you may redirect that damage to the villain target with the lowest HP.

			this.AddTriggers();
		}

		private IEnumerator DamageResponse(DealDamageAction dda)
		{
			// If you do so, you may either...

			// ...increase it by 2
			
			// ...draw 1 card
			
			// ...or play 1 card
			
			// Then destroy this card.

			yield break;
		}

		public override IEnumerator UsePower(int index = 0)
		{
			// {Speedrunner} deals 1 target 1 irreducible melee damage.
			
			// This damage cannot be redirected.
			
			yield break;
		}
	}
}