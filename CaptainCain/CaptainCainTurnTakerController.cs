using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;

namespace Angille.CaptainCain
{
	public class CaptainCainTurnTakerController : AngilleHeroTurnTakerController
	{
		public CaptainCainTurnTakerController(
			TurnTaker turnTaker,
			GameController gameController
		) : base(turnTaker, gameController)
		{
		}

		protected override IEnumerable<string> VillainsToAugment => new[] {
			"BloodCountessBathory"
		};
	}
}
