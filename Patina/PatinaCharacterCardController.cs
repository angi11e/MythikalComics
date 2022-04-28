using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.Patina
{
	public class PatinaCharacterCardController : HeroCharacterCardController
	{
		public PatinaCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator UsePower(int index = 0)
		{
			/*
			 * Up to 3 targets regain 1 HP each.
			 * A player may draw 1 card.
			 */

			int targetNumeral = GetPowerNumeral(0, 3);
			int gainNumeral = GetPowerNumeral(1, 1);
			int drawNumeral = GetPowerNumeral(2, 1);

			IEnumerator healCR = GameController.SelectAndGainHP(
				DecisionMaker,
				gainNumeral,
				false,
				(Card c) => c.IsTarget,
				targetNumeral,
				0,
				cardSource: GetCardSource()
			);
			IEnumerator drawCR = GameController.SelectHeroToDrawCards(
				DecisionMaker,
				drawNumeral,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(healCR);
				yield return GameController.StartCoroutine(drawCR);
			}
			else
			{
				GameController.ExhaustCoroutine(healCR);
				GameController.ExhaustCoroutine(drawCR);
			}

			/* old version that was super TLT
			List<GainHPAction> gainHPActions = new List<GainHPAction>();
			IEnumerator healCR = GameController.GainHP(
				DecisionMaker,
				(Card c) => c.IsHeroCharacterCard && c.IsInPlay && !c.IsIncapacitatedOrOutOfGame,
				1,
				storedResultsAction: gainHPActions,
				cardSource: GetCardSource()
			);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(healCR);
			}
			else
			{
				GameController.ExhaustCoroutine(healCR);
			}

			foreach (GainHPAction item in gainHPActions)
			{
				if (item.AmountActuallyGained < 1)
				{
					IEnumerator drawCR = DrawCard(item.HpGainer.Owner.ToHero(), optional: true);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(drawCR);
					}
					else
					{
						GameController.ExhaustCoroutine(drawCR);
					}
				}
			}
			*/

			yield break;
		}

		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			IEnumerator incapCR = null;

			switch (index)
			{
				case 0:
					// One target regains 2 HP.
					incapCR = GameController.SelectAndGainHP(
						DecisionMaker,
						2,
						cardSource: GetCardSource()
					);
					break;
				case 1:
					// One player may draw a card.
					incapCR = GameController.SelectHeroToDrawCard(
						DecisionMaker,
						cardSource: GetCardSource()
					);
					break;
				case 2:
					// One hero may use a power now.
					incapCR = GameController.SelectHeroToUsePower(
						DecisionMaker,
						cardSource: GetCardSource()
					);
					break;
			}

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(incapCR);
			}
			else
			{
				GameController.ExhaustCoroutine(incapCR);
			}

			yield break;
		}

		protected LinqCardCriteria IsWaterCriteria(Func<Card, bool> additionalCriteria = null)
		{
			var result = new LinqCardCriteria(c => IsWater(c), "water", true);
			if (additionalCriteria != null)
			{
				result = new LinqCardCriteria(result, additionalCriteria);
			}

			return result;
		}

		protected bool IsWater(Card card, bool evenIfUnderCard = false, bool evenIfFaceDown = false)
		{
			return card != null && base.GameController.DoesCardContainKeyword(card, "water", evenIfUnderCard, evenIfFaceDown);
		}
	}
}