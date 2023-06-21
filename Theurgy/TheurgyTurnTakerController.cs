using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Angille.Theurgy
{
	public class TheurgyTurnTakerController : AngilleHeroTurnTakerController
	{
		public TheurgyTurnTakerController(
			TurnTaker turnTaker,
			GameController gameController
		) : base(turnTaker, gameController)
		{
		}

		public string[] availablePromos = new string[] { "TheurgyFateweaver" };
		public bool ArePromosSetup { get; set; } = false;

		protected override IEnumerable<string> VillainsToAugment => new[] {
			"AkashBhutaCharacter",
			"GloomWeaverCharacter",
			"BugbearTeamCharacter",
			"Balarian",
			"Heartbreaker",
			"ManGrove",
			"Ruin",
			"VoidsoulCharacter",
			"TheInfernalChoirCharacter"
		};
	}
}
