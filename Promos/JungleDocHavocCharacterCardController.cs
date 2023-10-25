using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cauldron.DocHavoc;
using Handelabra;

namespace Angille.DocHavoc
{
	public class JungleDocHavocCharacterCardController : HeroCharacterCardController
	{
		public JungleDocHavocCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator UsePower(int index = 0)
		{
			// {DocHavoc} deals 1 target 1 toxic damage.
			// until the start of your next turn,
			// reduce hp recovery for that target by 2.
			yield break;
		}

		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			switch (index)
			{
				case 0:
					// One player may play a card.

					break;

				case 1:
					// select a target. until the start of your next turn, reduce their hp recovery by 2.
					break;

				case 2:
					// One target regains 2 HP.
					break;
			}
			yield break;
		}
	}
}