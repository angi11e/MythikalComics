using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.Patina
{
	public class InternPatinaCharacterCardController : HeroCharacterCardController
	{
		public InternPatinaCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator UsePower(int index = 0)
		{
			/*
			 * 
			 */

			yield break;
		}

		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			IEnumerator incapCR = null;

			switch (index)
			{
				case 0:
					break;
				case 1:
					break;
				case 2:
					break;
			}

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(incapCR);
			}
			else
			{
				GameController.ExhaustCoroutine(incapCR);
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