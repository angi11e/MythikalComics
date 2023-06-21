using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Linq;
using Handelabra;
using System.Collections.Generic;
using System;

namespace Angille.WhatsHerFace
{
	public class WhatsHerFaceGadgeteerPromoCardUnlockController : PromoCardUnlockController
	{

		public WhatsHerFaceGadgeteerPromoCardUnlockController(GameController gameController) : base(
			gameController,
			"Angille.WhatsHerFace",
			"WhatsHerFaceGadgeteerCharacter"
		)
		{
		}

		public override bool IsUnlockPossibleThisGame()
		{
			return IsInGame("Theurgy")
				&& IsInGame("RedRifle")
				&& IsInGame("NightMare")
				&& !IsInGame("WhatsHerFace")
				&& IsInGame("RookCity");
		}

		public override bool CheckForUnlock(GameAction action)
		{
			if (IsGameOverVictory(action) && IsOnlyIncapacitatedHero("Theurgy", "TheurgyCharacter")) {
				IsUnlocked = true;
			}

			return IsUnlocked;
		}
	}
}