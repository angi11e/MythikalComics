using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.PecosBill
{
	public class PecosBillCharacterCardController : HeroCharacterCardController
	{
		public PecosBillCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController) {
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
					break;

				case 1:
					break;

				case 2:
					break;
			}
			yield break;
		}

		protected LinqCardCriteria IsFolkCriteria(Func<Card, bool> additionalCriteria = null)
		{
			var result = new LinqCardCriteria(c => IsFolk(c), "folk", true);
			if (additionalCriteria != null)
			{
				result = new LinqCardCriteria(result, additionalCriteria);
			}

			return result;
		}

		protected bool IsFolk(Card card, bool evenIfUnderCard = false, bool evenIfFaceDown = false)
		{
			return card != null && GameController.DoesCardContainKeyword(card, "folk", evenIfUnderCard, evenIfFaceDown);
		}

		protected LinqCardCriteria IsHyperboleCriteria(Func<Card, bool> additionalCriteria = null)
		{
			var result = new LinqCardCriteria(c => IsHyperbole(c), "hyperbole", true);
			if (additionalCriteria != null)
			{
				result = new LinqCardCriteria(result, additionalCriteria);
			}

			return result;
		}

		protected bool IsHyperbole(Card card, bool evenIfUnderCard = false, bool evenIfFaceDown = false)
		{
			return card != null && GameController.DoesCardContainKeyword(card, "hyperbole", evenIfUnderCard, evenIfFaceDown);
		}
	}
}