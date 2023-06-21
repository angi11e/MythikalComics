using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;

namespace Angille.Supplicate
{
	public class SupplicateTurnTakerController : AngilleHeroTurnTakerController
	{
		public SupplicateTurnTakerController(
			TurnTaker turnTaker,
			GameController gameController
		) : base(turnTaker, gameController)
		{
		}

		public string[] availablePromos = new string[] { "" };
		public bool ArePromosSetup { get; set; } = false;

		protected override IEnumerable<string> VillainsToAugment => new[] {
			"WagerMasterCharacter"
		};
	}
}
