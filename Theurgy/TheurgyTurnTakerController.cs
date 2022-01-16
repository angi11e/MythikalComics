using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;

namespace Angille.Theurgy
{
	public class TheurgyTurnTakerController : HeroTurnTakerController
	{
		public TheurgyTurnTakerController(
			TurnTaker turnTaker,
			GameController gameController
		) : base(turnTaker, gameController)
		{
		}
	}
}
