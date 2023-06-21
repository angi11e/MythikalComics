using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;

namespace Angille.Athena
{
	public class AthenaTurnTakerController : AngilleHeroTurnTakerController
	{
		public AthenaTurnTakerController(
			TurnTaker turnTaker,
			GameController gameController
		) : base(turnTaker, gameController)
		{
		}

		public string[] availablePromos = new string[] {  };
		public bool ArePromosSetup { get; set; } = false;

		protected override IEnumerable<string> VillainsToAugment => new[] {
			"Atum", "Geb", "Isis", "Nephthys", "Nuit", "Osiris", "Set", "Shu", "Tefnut",
			"KaargraWarfangCharacter",
			"NixiousTheChosenCharacter",
			"Ammit",
			"Anubis",
			"Calypso",
			"Tantrum",
			"CeladrochCharacter"
		};
	}
}
