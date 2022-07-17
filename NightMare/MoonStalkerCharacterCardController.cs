using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.NightMare
{
	public class MoonStalkerCharacterCardController : HeroCharacterCardController
	{
		public MoonStalkerCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator UsePower(int index = 0)
		{
			// [i]Moon Stalker[/i] deals up to 3 targets 1 melee damage each.
			// Each time a target is destroyed this way, draw or discard a card.
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
					// Destroy an ongoing card.
					break;

				case 2:
					// Move a card from a trash to under its associated deck.
					break;
			}
			yield break;
		}
	}
}