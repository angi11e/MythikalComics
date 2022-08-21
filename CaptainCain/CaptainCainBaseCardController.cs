using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.CaptainCain
{
	public abstract class CaptainCainBaseCardController : CardController
	{
		protected CaptainCainBaseCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
		{
		}

		protected bool IsBloodActive => HeroTurnTaker.GetCardsWhere(
			(Card c) => c.IsInPlayAndNotUnderCard && IsBlood(c) && c.Owner == this.Card.Owner
		).Count() > 0;

		protected bool IsFistActive => HeroTurnTaker.GetCardsWhere(
			(Card c) => c.IsInPlayAndNotUnderCard && IsFist(c) && c.Owner == this.Card.Owner
		).Count() > 0;

		protected LinqCardCriteria IsBloodCriteria(Func<Card, bool> additionalCriteria = null)
		{
			var result = new LinqCardCriteria(c => IsBlood(c), "blood", true);
			if (additionalCriteria != null)
			{
				result = new LinqCardCriteria(result, additionalCriteria);
			}

			return result;
		}

		protected bool IsBlood(Card card, bool evenIfUnderCard = false, bool evenIfFaceDown = false)
		{
			return card != null && GameController.DoesCardContainKeyword(card, "blood", evenIfUnderCard, evenIfFaceDown);
		}

		protected LinqCardCriteria IsFistCriteria(Func<Card, bool> additionalCriteria = null)
		{
			var result = new LinqCardCriteria(c => IsFist(c), "fist", true);
			if (additionalCriteria != null)
			{
				result = new LinqCardCriteria(result, additionalCriteria);
			}

			return result;
		}

		protected bool IsFist(Card card, bool evenIfUnderCard = false, bool evenIfFaceDown = false)
		{
			return card != null && GameController.DoesCardContainKeyword(card, "fist", evenIfUnderCard, evenIfFaceDown);
		}
	}
}