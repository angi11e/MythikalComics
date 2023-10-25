using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;

namespace Angille.Starblade
{
	public class StarbladeTurnTakerController : AngilleHeroTurnTakerController
	{
		public StarbladeTurnTakerController(
			TurnTaker turnTaker,
			GameController gameController
		) : base(turnTaker, gameController)
		{
		}

		protected override IEnumerable<string> VillainsToAugment => new[] {
			"InfinitorCharacter",
			"ProgenyCharacter",
			"MenagerieCharacter",
			"PhaseVillainCharacter"
		};
	}
}
