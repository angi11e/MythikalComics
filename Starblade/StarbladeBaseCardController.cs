using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Starblade
{
	public abstract class StarbladeBaseCardController : CardController
	{
		protected StarbladeBaseCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
		}

		/*
		protected bool ManifestInPlay => HeroTurnTaker.GetCardsWhere(
			(Card c) => c.IsInPlayAndNotUnderCard && IsManifest(c)
		).Any();
		*/

		protected LinqCardCriteria IsPosturaCriteria(Func<Card, bool> additionalCriteria = null)
		{
			var result = new LinqCardCriteria(c => IsPostura(c), "postura", true);
			if (additionalCriteria != null)
			{
				result = new LinqCardCriteria(result, additionalCriteria);
			}

			return result;
		}

		protected bool IsPostura(Card card, bool evenIfUnderCard = false, bool evenIfFaceDown = false)
		{
			return card != null && GameController.DoesCardContainKeyword(card, "postura", evenIfUnderCard, evenIfFaceDown);
		}
	}
}