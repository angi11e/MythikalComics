using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace Angille.Athena
{
	public class AthenaTurnTakerController : HeroTurnTakerController
	{
		public AthenaTurnTakerController(
			TurnTaker turnTaker,
			GameController gameController
		) : base(turnTaker, gameController)
		{
		}
	}
}
