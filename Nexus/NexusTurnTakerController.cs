using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;

namespace Angille.Nexus
{
	public class NexusTurnTakerController : AngilleHeroTurnTakerController
	{
		public NexusTurnTakerController(
			TurnTaker turnTaker,
			GameController gameController
		) : base(turnTaker, gameController)
		{
		}

		protected override IEnumerable<string> VillainsToAugment => new[] {
			"ChokepointCharacter",
			"DeadlineCharacter",
			"GrayCharacter",
			"VectorCharacter"
		};
	}
}
