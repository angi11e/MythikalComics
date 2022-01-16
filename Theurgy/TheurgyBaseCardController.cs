using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Theurgy
{
	public abstract class TheurgyBaseCardController : CardController
	{
		protected TheurgyBaseCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
		{
		}

		protected LinqCardCriteria IsCharmCriteria(Func<Card, bool> additionalCriteria = null)
		{
			var result = new LinqCardCriteria(c => IsCharm(c), "charm", true);
			if (additionalCriteria != null)
			{
				result = new LinqCardCriteria(result, additionalCriteria);
			}

			return result;
		}

		protected bool IsCharm(Card card, bool evenIfUnderCard = false, bool evenIfFaceDown = false)
		{
			return card != null && base.GameController.DoesCardContainKeyword(card, "charm", evenIfUnderCard, evenIfFaceDown);
		}
	}
}