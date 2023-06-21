using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Linq;
using Handelabra;
using System.Collections.Generic;
using System;

namespace Angille.Theurgy
{
	public class TheurgyFateweaverPromoCardUnlockController : PromoCardUnlockController
	{

		public TheurgyFateweaverPromoCardUnlockController(GameController gameController) : base(
			gameController,
			"Angille.Theurgy",
			"TheurgyFateweaverCharacter"
		)
		{
		}

		public override bool IsUnlockPossibleThisGame()
		{
			return IsInGame("TheScholar") && IsInGame("TheWraith");
		}

		public override bool CheckForUnlock(GameAction action)
		{
			if (action is UsePowerAction) {
				UsePowerAction upa = (UsePowerAction)action;
				if (
					upa.HeroUsingPower.HeroTurnTaker.Identifier == "TheWraith"
					&& IsInPlayAndNotUnderCard("ProverbsAndAxioms")
				)
				{
					IsUnlocked = true;
				}
			}

			return IsUnlocked;
		}
	}
}