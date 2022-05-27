using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace Angille.Speedrunner
{
	public class SpeedrunnerTurnTakerController : HeroTurnTakerController
	{
		public SpeedrunnerTurnTakerController(
			TurnTaker turnTaker,
			GameController gameController
		) : base(turnTaker, gameController)
		{
		}
	}
}
