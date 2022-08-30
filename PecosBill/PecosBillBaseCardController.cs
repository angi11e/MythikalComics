using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.PecosBill
{
	public abstract class PecosBillBaseCardController : CardController
	{
		protected PecosBillBaseCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
		{
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