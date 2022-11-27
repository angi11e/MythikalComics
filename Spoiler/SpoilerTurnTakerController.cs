using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;

namespace Angille.Spoiler
{
	public class SpoilerTurnTakerController : AngilleHeroTurnTakerController
	{
		public SpoilerTurnTakerController(
			TurnTaker turnTaker,
			GameController gameController
		) : base(turnTaker, gameController)
		{
		}

		protected override IEnumerable<string> VillainsToAugment => new[] {
			"LaCapitanCharacter",
			"LaCapitanTeamCharacter",
			"GrandfatherCharacter"
		};
	}
}
