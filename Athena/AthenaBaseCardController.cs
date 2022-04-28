using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Athena
{
	public abstract class AthenaBaseCardController : CardController
	{
		protected AthenaBaseCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
		}

		protected bool AspectInPlay => HeroTurnTaker.GetCardsWhere(
			(Card c) => c.IsInPlayAndNotUnderCard && IsAspect(c)
		).Count() > 0;

		protected LinqCardCriteria IsLegendCriteria(Func<Card, bool> additionalCriteria = null)
		{
			var result = new LinqCardCriteria(c => IsLegend(c), "legend", true);
			if (additionalCriteria != null)
			{
				result = new LinqCardCriteria(result, additionalCriteria);
			}

			return result;
		}

		protected bool IsLegend(Card card, bool evenIfUnderCard = false, bool evenIfFaceDown = false)
		{
			return card != null && base.GameController.DoesCardContainKeyword(card, "legend", evenIfUnderCard, evenIfFaceDown);
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