using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.Supplicate
{
	public class SupplicateBaseCharacterCardController : HeroCharacterCardController
	{
		public SupplicateBaseCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController) {
		}

		public override void AddStartOfGameTriggers()
		{
			AddTrigger(
				(GameAction ga) => TurnTakerController is SupplicateTurnTakerController ttc && !ttc.ArePromosSetup,
				SetupPromos,
				TriggerType.Hidden,
				TriggerTiming.Before,
				priority: TriggerPriority.High
			);
		}

		public IEnumerator SetupPromos(GameAction ga)
		{
			if (TurnTakerController is SupplicateTurnTakerController ttc && !ttc.ArePromosSetup)
			{
				ttc.SetupPromos(ttc.availablePromos);
				ttc.ArePromosSetup = true;
			}
			return DoNothing();
		}

		protected LinqCardCriteria IsYaojingCriteria(Func<Card, bool> additionalCriteria = null)
		{
			var result = new LinqCardCriteria(c => IsYaojing(c), "yaojing", true);
			if (additionalCriteria != null)
			{
				result = new LinqCardCriteria(result, additionalCriteria);
			}

			return result;
		}

		protected bool IsYaojing(Card card, bool evenIfUnderCard = false, bool evenIfFaceDown = false)
		{
			return card != null && GameController.DoesCardContainKeyword(
				card,
				"yaojing",
				evenIfUnderCard,
				evenIfFaceDown
			);
		}
	}
}