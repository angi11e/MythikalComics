using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;

namespace Angille.Speedrunner
{
	public class SpeedrunnerTurnTakerController : AngilleHeroTurnTakerController
	{
		public SpeedrunnerTurnTakerController(
			TurnTaker turnTaker,
			GameController gameController
		) : base(turnTaker, gameController)
		{
		}

		protected override IEnumerable<string> VillainsToAugment => new[] {
			"MissInformationCharacter",
			"AeonMasterCharacter",
			"VoidsoulCharacter",
			"Argentium",
			"Cueball",
			"GreenGrosser",
			"Highbrow",
			"Rahazar",
			"SwarmEaterCharacter"
		};
	}
}
