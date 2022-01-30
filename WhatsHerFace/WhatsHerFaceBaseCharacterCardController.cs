using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.WhatsHerFace
{
	public class WhatsHerFaceBaseCharacterCardController : HeroCharacterCardController
	{
		public WhatsHerFaceBaseCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		protected LinqCardCriteria IsRecallCriteria(Func<Card, bool> additionalCriteria = null)
		{
			var result = new LinqCardCriteria(c => IsRecall(c), "recall", true);
			if (additionalCriteria != null)
			{
				result = new LinqCardCriteria(result, additionalCriteria);
			}

			return result;
		}

		protected bool IsRecall(Card card, bool evenIfUnderCard = false, bool evenIfFaceDown = false)
		{
			return card != null && base.GameController.DoesCardContainKeyword(
				card,
				"recall",
				evenIfUnderCard,
				evenIfFaceDown
			);
		}
	}
}