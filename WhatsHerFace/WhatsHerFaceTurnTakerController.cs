using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;

namespace Angille.WhatsHerFace
{
	public class WhatsHerFaceTurnTakerController : AngilleHeroTurnTakerController
	{
		public WhatsHerFaceTurnTakerController(
			TurnTaker turnTaker,
			GameController gameController
		) : base(turnTaker, gameController)
		{
		}

		protected override IEnumerable<string> VillainsToAugment => new[] {
			"IronLegacyCharacter",
			"KismetCharacter",
			"SpiteCharacter",
			"ErmineTeamCharacter",
			"MissInformationTeamCharacter",
			"PlagueRatTeamCharacter",
			"Heartbreaker",
			"MayorOverbrook",
			"Revenant",
			"ReVolt",
			"DarkMindCharacter",
			"ProgenyScionCharacter",
			"FaultlessCharacter",
			"TheMistressOfFateCharacter"
		};
	}
}
