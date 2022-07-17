using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.Patina
{
	public class VerdigrisCharacterCardController : HeroCharacterCardController
	{
		public VerdigrisCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator UsePower(int index = 0)
		{
			/*
			 * [i]Verdigris[i] deals 1 target 2 melee damage and 2 cold damage,
			 * in either order.
			 * That target deals [i]Verdigris[/i] 3 melee damage.
			 */

			yield break;
		}

		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			switch (index)
			{
				case 0:
					// One player may play a card.
					break;
				case 1:
					// One hero deals 1 target 1 melee damage. A different hero deals 1 target 1 cold damage.
					break;
				case 2:
					// Each Hero may deal themselves 3 Cold damage. A Hero dealt damage this way may use a Power.
					break;
			}

			yield break;
		}

		protected LinqCardCriteria IsWaterCriteria(Func<Card, bool> additionalCriteria = null)
		{
			var result = new LinqCardCriteria(c => IsWater(c), "water", true);
			if (additionalCriteria != null)
			{
				result = new LinqCardCriteria(result, additionalCriteria);
			}

			return result;
		}

		protected bool IsWater(Card card, bool evenIfUnderCard = false, bool evenIfFaceDown = false)
		{
			return card != null && base.GameController.DoesCardContainKeyword(card, "water", evenIfUnderCard, evenIfFaceDown);
		}
	}
}