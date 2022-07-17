using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;

namespace Angille.NightMare
{
	public class NightMareTurnTakerController : AngilleHeroTurnTakerController
	{
		public NightMareTurnTakerController(
			TurnTaker turnTaker,
			GameController gameController
		) : base(turnTaker, gameController)
		{
		}

		protected override IEnumerable<string> VillainsToAugment => new[] {
			"ProgenyCharacter",
			"TheChairmanCharacter",
			"TheOperative",
			"AeonMasterCharacter",
			"DarkMindCharacter",
			"FaultlessCharacter",
			"SergeantSteelTeamCharacter",
			"TheOperativeTeamCharacter",
			"Choke",
			"Heartbreaker",
			"ZhuLong",
			"TheTrueForm",
			"DynamoCharacter"
		};
	}
}
