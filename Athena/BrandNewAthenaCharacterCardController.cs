using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.Athena
{
	public class BrandNewAthenaCharacterCardController : HeroCharacterCardController
	{
		public BrandNewAthenaCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator UsePower(int index = 0)
		{
			// If there is an [u]aspect[/u] card in play,
			
			// {Athena} deals 1 target 1 melee and 1 radiant damage.

			// If not, draw 2 cards, then discard 1 card.

			yield break;
		}

		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			switch (index)
			{
				case 0:
					// One player may draw a card.
					break;
				case 1:
					// One player may play a card.
					break;
				case 2:
					// Increase the next damage dealt by a hero target by 1.
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