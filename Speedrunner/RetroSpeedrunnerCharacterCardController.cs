using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.Speedrunner
{
	public class RetroSpeedrunnerCharacterCardController : HeroCharacterCardController
	{
		public RetroSpeedrunnerCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator UsePower(int index = 0)
		{
			// put a one-shot card from a hero trash into play.
			// when that card finishes resolving, move it to the bottom of its deck.

			yield break;
		}

		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			switch (index)
			{
				case 0:
					// One player may draw or play a card.
					break;
				case 1:
					// One Player may take a card from their trash into their hand.
					break;
				case 2:
					// One player discards 1 card.
					// If the discarded card was a one-shot, that player may draw 2 cards.
					// If the discarded card was an ongoing or equipment card, 1 hero target regains 2 HP.
					break;
			}
			yield break;
		}
	}
}