using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;

namespace Angille.RedRifle
{
	public class RedRifleTurnTakerController : AngilleHeroTurnTakerController
	{
		public RedRifleTurnTakerController(
			TurnTaker turnTaker,
			GameController gameController
		) : base(turnTaker, gameController)
		{
		}

		protected override IEnumerable<string> VillainsToAugment => new[] {
			"ApostateCharacter",
			"CitizenDawnCharacter",
			"DarkMindCharacter",
			"FaultlessCharacter",
			"NixiousTheChosenCharacter",
			"VoidsoulCharacter",
			"Balarian",
			"Heartbreaker",
			"TheIdolater",
			"TheSeer",
			"AnathemaCharacter"
		};
	}
}
