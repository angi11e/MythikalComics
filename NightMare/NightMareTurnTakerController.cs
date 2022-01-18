using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace Angille.NightMare
{
	public class NightMareTurnTakerController : HeroTurnTakerController
	{
		public NightMareTurnTakerController(
			TurnTaker turnTaker,
			GameController gameController
		) : base(turnTaker, gameController)
		{
		}
	}
}
