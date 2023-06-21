using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.Patina
{
	public class InternPatinaCharacterCardController : HeroCharacterCardController
	{
		public InternPatinaCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int discardNumeral = GetPowerNumeral(0, 3);
			int drawNumeral = GetPowerNumeral(1, 1);
			int playNumeral = GetPowerNumeral(2, 1);

			// Discard up to 3 [u]water[/u] cards.
			List<DiscardCardAction> discardResults = new List<DiscardCardAction>();
			IEnumerator discardCR = SelectAndDiscardCards(
				DecisionMaker,
				discardNumeral,
				requiredDecisions: 0,
				storedResults: discardResults,
				cardCriteria: IsWaterCriteria()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(discardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(discardCR);
			}

			// For each card discarded this way,
			int cardsDiscarded = GetNumberOfCardsDiscarded(discardResults);
			if (cardsDiscarded > 0)
			{
				string drawPlural = drawNumeral == 1 ? "" : "s";
				string playPlural = playNumeral == 1 ? "" : "s";

				for (int i = 0; i < cardsDiscarded; i++)
				{
					// draw 1 card or play 1 non-[u]water[/u] card.
					List<Function> choices = new List<Function>();
					choices.Add(new Function(
						this.HeroTurnTakerController,
						$"Draw {drawNumeral} card{drawPlural}",
						SelectionType.DrawCard,
						() => DrawCards(this.HeroTurnTakerController, drawNumeral),
						CanDrawCards(this.HeroTurnTakerController),
						$"{this.Card.Title} has no non-water cards to play, so must draw {drawNumeral} card{drawPlural}."
					));
					choices.Add(new Function(
						this.HeroTurnTakerController,
						$"Play {playNumeral} non-water card{playPlural}",
						SelectionType.PlayCard,
						() => SelectAndPlayCardsFromHand(
							this.HeroTurnTakerController,
							playNumeral,
							requiredDecisions: playNumeral,
							cardCriteria: new LinqCardCriteria((Card c) => !IsWater(c))
						),
						HeroTurnTaker.GetCardsWhere(
							(Card c) => c.Location == this.HeroTurnTaker.Hand && !IsWater(c)
						).Count() > 0,
						$"{this.Card.Title} cannot draw cards, so must play {playNumeral} non-water card{playPlural}."
					));

					SelectFunctionDecision drawOrPlay = new SelectFunctionDecision(
						GameController,
						this.HeroTurnTakerController,
						choices,
						false,
						null,
						$"{this.Card.Title} cannot draw nor play cards.",
						cardSource: GetCardSource()
					);
					IEnumerator drawPlayCR = GameController.SelectAndPerformFunction(drawOrPlay);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(drawPlayCR);
					}
					else
					{
						GameController.ExhaustCoroutine(drawPlayCR);
					}
				}
			}

			yield break;
		}

		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			IEnumerator incapCR = DoNothing();

			switch (index)
			{
				case 0:
					// One player may draw a card.
					incapCR = GameController.SelectHeroToDrawCard(
						DecisionMaker,
						cardSource: GetCardSource()
					);
					break;
				case 1:
					// Up to two hero equipment cards may be played now.
					incapCR = GameController.SelectTurnTakersAndDoAction(
						DecisionMaker,
						new LinqTurnTakerCriteria((TurnTaker tt) => !tt.IsIncapacitatedOrOutOfGame && IsHero(tt)),
						SelectionType.PlayCard,
						(TurnTaker tt) => SelectAndPlayCardFromHand(
							FindHeroTurnTakerController(tt.ToHero()),
							cardCriteria: new LinqCardCriteria((Card c) => IsEquipment(c))
						),
						2,
						cardSource: GetCardSource()
					);
					break;
				case 2:
					// Each hero target with an equipment card in their play area regains 1 HP.
					incapCR = GameController.GainHP(
						DecisionMaker,
						(Card c) => IsHero(c) && c.Location.OwnerTurnTaker.PlayArea.Cards.Any((Card d) => IsEquipment(d)),
						1,
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
			return card != null && GameController.DoesCardContainKeyword(card, "water", evenIfUnderCard, evenIfFaceDown);
		}
	}
}