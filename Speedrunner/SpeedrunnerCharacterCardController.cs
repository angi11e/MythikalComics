using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.Speedrunner
{
	public class SpeedrunnerCharacterCardController : HeroCharacterCardController
	{
		public SpeedrunnerCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator UsePower(int index = 0)
		{
			// The next time {Speedrunner} would be dealt damage, reduce it by 1 and draw 1 card.
			yield break;
		}

		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			switch (index)
			{
				case 0:
					// One Player may Play a card now.
					break;
				case 1:
					// One Player may Discard 2 cards. If they do, they may Draw 2 cards.
					break;
				case 2:
					// Reveal the Top card of the Villain Deck, then replace it.
					break;
			}
			yield break;
		}
	}
}