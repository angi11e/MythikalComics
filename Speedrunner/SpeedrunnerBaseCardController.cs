using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Speedrunner
{
	public abstract class SpeedrunnerBaseCardController : CardController
	{
		protected SpeedrunnerBaseCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
		}

		protected LinqCardCriteria IsGlitchCriteria(Func<Card, bool> additionalCriteria = null)
		{
			var result = new LinqCardCriteria(c => IsGlitch(c), "glitch", true);
			if (additionalCriteria != null)
			{
				result = new LinqCardCriteria(result, additionalCriteria);
			}

			return result;
		}

		protected bool IsGlitch(Card card, bool evenIfUnderCard = false, bool evenIfFaceDown = false)
		{
			return card != null && base.GameController.DoesCardContainKeyword(card, "glitch", evenIfUnderCard, evenIfFaceDown);
		}

		protected LinqCardCriteria IsStratCriteria(Func<Card, bool> additionalCriteria = null)
		{
			var result = new LinqCardCriteria(c => IsStrat(c), "strat", true);
			if (additionalCriteria != null)
			{
				result = new LinqCardCriteria(result, additionalCriteria);
			}

			return result;
		}

		protected bool IsStrat(Card card, bool evenIfUnderCard = false, bool evenIfFaceDown = false)
		{
			return card != null && base.GameController.DoesCardContainKeyword(card, "strat", evenIfUnderCard, evenIfFaceDown);
		}
	}
}