using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.Athena
{
	public class ClassicAthenaCharacterCardController : HeroCharacterCardController
	{
		public ClassicAthenaCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator UsePower(int index = 0)
		{
			// Reveal the top 3 cards of your deck.

			// If an [u]aspect[/u] card is revealed you may play it now.

			// Move 1 revealed card to the bottom of your deck, and move the rest to your hand.

			yield break;
		}

		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			switch (index)
			{
				case 0:
					// One hero may use a power now.
					break;
				case 1:
					// Look at the bottom card of each deck and replace or discard each one.
					break;
				case 2:
					// One hero may discard a card to reduce damage dealt to them by 1 until the start of your turn.
					break;
			}
			yield break;
		}

		protected LinqCardCriteria IsAspectCriteria(Func<Card, bool> additionalCriteria = null)
		{
			var result = new LinqCardCriteria(c => IsAspect(c), "aspect", true);
			if (additionalCriteria != null)
			{
				result = new LinqCardCriteria(result, additionalCriteria);
			}

			return result;
		}

		protected bool IsAspect(Card card, bool evenIfUnderCard = false, bool evenIfFaceDown = false)
		{
			return card != null && base.GameController.DoesCardContainKeyword(card, "aspect", evenIfUnderCard, evenIfFaceDown);
		}
	}
}