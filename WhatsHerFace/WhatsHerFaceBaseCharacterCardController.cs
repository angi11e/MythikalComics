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

		public override void AddStartOfGameTriggers()
		{
			AddTrigger(
				(GameAction ga) => TurnTakerController is WhatsHerFaceTurnTakerController ttc && !ttc.ArePromosSetup,
				SetupPromos,
				TriggerType.Hidden,
				TriggerTiming.Before,
				priority: TriggerPriority.High
			);
		}

		public IEnumerator SetupPromos(GameAction ga)
		{
			if (TurnTakerController is WhatsHerFaceTurnTakerController ttc && !ttc.ArePromosSetup)
			{
				ttc.SetupPromos(ttc.availablePromos);
				ttc.ArePromosSetup = true;
			}
			return DoNothing();
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
			return card != null && GameController.DoesCardContainKeyword(
				card,
				"recall",
				evenIfUnderCard,
				evenIfFaceDown
			);
		}
	}
}