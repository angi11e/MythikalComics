using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.RedRifle
{
	public class SeeingRedRedRifleCharacterCardController : HeroCharacterCardController
	{
		public SeeingRedRedRifleCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			SpecialStringMaker.ShowTokenPool(this.Card.FindTokenPool("RedRifleTrueshotPool"));
		}

		public override IEnumerator UsePower(int index = 0)
		{
			TokenPool trueshotPool = RedRifleTrueshotPoolUtility.GetTrueshotPool(this);
			int addNumeral = GetPowerNumeral(0, 3);
			int cardNumeral = GetPowerNumeral(1, 1);
			int removeNumeral = GetPowerNumeral(2, 2);

			// Add 3 tokens to your trueshot pool.
			IEnumerator addTokensCR = RedRifleTrueshotPoolUtility.AddTrueshotTokens(this, addNumeral);

			// Play 1 one-shot card.
			IEnumerator playCardsCR = SelectAndPlayCardsFromHand(
				DecisionMaker,
				cardNumeral,
				false,
				cardNumeral,
				cardCriteria: new LinqCardCriteria((Card c) => c.IsOneShot, "one-shot")
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(addTokensCR);
				yield return GameController.StartCoroutine(playCardsCR);
			}
			else
			{
				GameController.ExhaustCoroutine(addTokensCR);
				GameController.ExhaustCoroutine(playCardsCR);
			}

			// Remove 2 tokens from your trueshot pool.
			if (removeNumeral > trueshotPool.CurrentValue)
			{
				removeNumeral = trueshotPool.CurrentValue;
			}

			if (removeNumeral > 0)
			{
				IEnumerator removeTokensCR = RedRifleTrueshotPoolUtility.RemoveTrueshotTokens<GameAction>(
					this,
					removeNumeral
				);
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(removeTokensCR);
				}
				else
				{
					GameController.ExhaustCoroutine(removeTokensCR);
				}
			}

			yield break;
		}

		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			switch (index)
			{
				case 0:
					// One hero may use a power.
					IEnumerator usePowerCR = GameController.SelectHeroToUsePower(DecisionMaker);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(usePowerCR);
					}
					else
					{
						GameController.ExhaustCoroutine(usePowerCR);
					}
					break;

				case 1:
					// One player may discard 2 cards. If they do, they may draw 2 cards.
					List<SelectTurnTakerDecision> storedTurnTaker = new List<SelectTurnTakerDecision>();
					List<DiscardCardAction> storedDiscards = new List<DiscardCardAction>();

					IEnumerator discardCR = GameController.SelectHeroToDiscardCards(
						DecisionMaker,
						0,
						2,
						optionalDiscardCard: true,
						storedResultsTurnTaker: storedTurnTaker,
						storedResultsDiscard: storedDiscards,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(discardCR);
					}
					else
					{
						GameController.ExhaustCoroutine(discardCR);
					}

					if (!DidDiscardCards(storedDiscards, 2))
					{
						break;
					}

					SelectTurnTakerDecision selectTurnTakerDecision = storedTurnTaker.FirstOrDefault();
					if (selectTurnTakerDecision != null && IsHero(selectTurnTakerDecision.SelectedTurnTaker))
					{
						IEnumerator drawCR = DrawCards(
							FindHeroTurnTakerController(selectTurnTakerDecision.SelectedTurnTaker.ToHero()),
							2,
							optional: true
						);
						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(drawCR);
						}
						else
						{
							GameController.ExhaustCoroutine(drawCR);
						}
					}
					break;

				case 2:
					// Destroy a target with 1 HP.
					IEnumerator destroyCR = GameController.SelectAndDestroyCard(
						this.HeroTurnTakerController,
						new LinqCardCriteria(
							(Card c) => c.IsTarget && c.HitPoints.Value == 1,
							"targets with 1 HP",
							useCardsSuffix: false
						),
						optional: false,
						cardSource: GetCardSource()
					);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(destroyCR);
					}
					else
					{
						GameController.ExhaustCoroutine(destroyCR);
					}
					break;
			}
			yield break;
		}
	}
}