using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Linq;
using Handelabra;
using System.Collections.Generic;
using System;

namespace Angille.WhatsHerFace
{
	public class UmbraPromoCardUnlockController : PromoCardUnlockController
	{

		public UmbraPromoCardUnlockController(GameController gameController) : base(
			gameController,
			"Angille.WhatsHerFace",
			"UmbraCharacter"
		)
		{
		}

		public override bool IsUnlockPossibleThisGame()
		{
			return AreInGame(
				new string[2] { "NightMare", "WhatsHerFace" },
				new Dictionary<string, string> {
					{ "NightMare", "MoonStalkerCharacter" },
					{ "WhatsHerFace", "WhatsHerFaceCharacter" }
				}
			);
		}

		public override bool CheckForUnlock(GameAction action)
		{
			if (action is DealDamageAction) {
				DealDamageAction dda = (DealDamageAction)action;
				if (
					dda.DidDealDamage
					&& dda.Target == FindCard("WhatsHerFaceCharacter")
					&& dda.DamageSource.IsTarget
					&& dda.DamageSource.Card == FindCard("MoonStalkerCharacter")
				)
				{
					IsUnlocked = true;
				}
			}

			return IsUnlocked;
		}
	}
}