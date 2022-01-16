using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using System.Collections;
using Handelabra;

namespace Angille.TheUndersiders
{
	public class TheUndersidersTurnTakerController : TurnTakerController
	{
		public TheUndersidersTurnTakerController(TurnTaker turnTaker, GameController gameController)
			: base(turnTaker, gameController)
		{
		}

		public override IEnumerator StartGame()
		{
			// At the start of the game, this card enters play �Gather Forces� side up.
			// Warlords of Brockton is put into play. The Villain deck is shuffled.
			// The 8 Villain character cards are shuffled and placed beneath Warlords of Brockton.
			// The top {H - 2} cards from beneath Warlords of Brockton are moved into the villain play area.

			List<Card> villains = (from c in base.TurnTaker.GetAllCards()
				where c.IsVillainCharacterCard && !c.Location.IsOutOfGame
				select c).ToList();
			List<Card> list = villains.TakeRandom(base.GameController.Game.H - 2, base.GameController.Game.RNG).ToList();

			foreach (Card villain in list)
			{
				IEnumerator playVillainCR = base.GameController.PlayCard(this, villain, true);
				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(playVillainCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(playVillainCR);
				}
				villains.Remove(villain);
			}

			Card warlords = base.TurnTaker.FindCard("WarlordsOfBrockton");
			IEnumerator moveCR = base.GameController.BulkMoveCards(this, villains, warlords.UnderLocation);
			IEnumerator shuffleCR = base.GameController.ShuffleLocation(warlords.UnderLocation);
			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(moveCR);
				yield return base.GameController.StartCoroutine(shuffleCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(moveCR);
				base.GameController.ExhaustCoroutine(shuffleCR);
			}
		}
	}
}
