using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.WhatsHerFace
{
	public class UmbraCharacterCardController : WhatsHerFaceBaseCharacterCardController
	{
		/*
		 * Reveal the top 2 cards of your deck.
		 * Play 1 of them and put the rest in your hand.
		 */

		public UmbraCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator UsePower(int index = 0)
		{

			yield break;
		}

		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			switch (index)
			{
				case 0:
					// Discard the top 3 cards of 1 deck.
					// (swarming protocol cypher)
					break;

				case 1:
					// Select a target.
					// Increase the next damage dealt to that target by 1 and
					//  reduce the next damage dealt by that target by 1.
					// (cosmic inventor writhe)
					break;

				case 2:
					// Destroy up to two ongoing cards. Only one may be a villain card.
					break;
			}
			yield break;
		}
	}
}