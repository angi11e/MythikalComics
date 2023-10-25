using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cauldron.Impact;
using Handelabra;

namespace Angille.Impact
{
	public class JungleImpactCharacterCardController : HeroCharacterCardController
	{
		public JungleImpactCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator UsePower(int index = 0)
		{
			// Play up to 2 ongoing cards. Return each of those cards which are still in play to your hand.

			yield break;
		}

		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			switch (index)
			{
				case 0:
					// One player may draw a card.
					
					break;

				case 1:
					// Up to two hero ongoing cards may be played now. (RR)

					break;

				case 2:
					// Move up to 3 non-character hero cards from play to their owner's hands. (LaCom)

					break;
			}
			yield break;
		}
	}
}