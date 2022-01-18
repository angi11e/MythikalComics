using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace Angille.WhatsHerFace
{
	public class WhatsHerFaceTurnTakerController : HeroTurnTakerController
	{
		public WhatsHerFaceTurnTakerController(
			TurnTaker turnTaker,
			GameController gameController
		) : base(turnTaker, gameController)
		{
		}
	}
}
