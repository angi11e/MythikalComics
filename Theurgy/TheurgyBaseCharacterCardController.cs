using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.Theurgy
{
	public class TheurgyBaseCharacterCardController : HeroCharacterCardController
	{
		public TheurgyBaseCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController) {
		}

		public override void AddStartOfGameTriggers()
		{
			AddTrigger(
				(GameAction ga) => TurnTakerController is TheurgyTurnTakerController ttc && !ttc.ArePromosSetup,
				SetupPromos,
				TriggerType.Hidden,
				TriggerTiming.Before,
				priority: TriggerPriority.High
			);
		}

		public IEnumerator SetupPromos(GameAction ga)
		{
			if (TurnTakerController is TheurgyTurnTakerController ttc && !ttc.ArePromosSetup)
			{
				ttc.SetupPromos(ttc.availablePromos);
				ttc.ArePromosSetup = true;
			}
			return DoNothing();
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
			return card != null && GameController.DoesCardContainKeyword(card, "charm", evenIfUnderCard, evenIfFaceDown);
		}

		protected int CharmCardsInPlay => FindCardsWhere(
			(Card c) => c.IsInPlayAndHasGameText && IsCharm(c) && !c.IsOneShot
		).Count();
	}
}