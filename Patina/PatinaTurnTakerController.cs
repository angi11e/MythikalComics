using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;

namespace Angille.Patina
{
	public class PatinaTurnTakerController : AngilleHeroTurnTakerController
	{
		public PatinaTurnTakerController(
			TurnTaker turnTaker,
			GameController gameController
		) : base(turnTaker, gameController)
		{
		}

		protected override IEnumerable<string> VillainsToAugment => new[] {
			"GrandWarlordVossCharacter",
			"IronLegacyCharacter",
			"TheMatriarchCharacter",
			"EmpyreonCharacter",
			"ProgenyScionCharacter",
			"VoidsoulCharacter",
			"FrictionTeamCharacter",
			"MissInformationTeamCharacter",
			"Balarian",
			"Glamour",
			"Vyktor",
			"MenagerieCharacter"
		};
	}
}
