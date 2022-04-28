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
			base.SpecialStringMaker.ShowTokenPool(base.Card.FindTokenPool("RedRifleTrueshotPool"));
		}

		public override void AddStartOfGameTriggers()
		{
			// base.AddStartOfGameTriggers();
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
				cardCriteria: new LinqCardCriteria((Card c) => c.IsOneShot, "one-shot")
			);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(addTokensCR);
				yield return base.GameController.StartCoroutine(playCardsCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(addTokensCR);
				base.GameController.ExhaustCoroutine(playCardsCR);
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
				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(removeTokensCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(removeTokensCR);
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
					if (base.UseUnityCoroutines)
					{
						yield return base.GameController.StartCoroutine(usePowerCR);
					}
					else
					{
						base.GameController.ExhaustCoroutine(usePowerCR);
					}
					break;

				case 1:
					// One player may discard 2 cards. If they do, they may draw 2 cards.
					List<SelectTurnTakerDecision> storedTurnTaker = new List<SelectTurnTakerDecision>();
					List<DiscardCardAction> storedDiscards = new List<DiscardCardAction>();

					IEnumerator discardCR = base.GameController.SelectHeroToDiscardCards(
						DecisionMaker,
						0,
						2,
						optionalDiscardCard: true,
						storedResultsTurnTaker: storedTurnTaker,
						storedResultsDiscard: storedDiscards,
						cardSource: GetCardSource()
					);

					if (base.UseUnityCoroutines)
					{
						yield return base.GameController.StartCoroutine(discardCR);
					}
					else
					{
						base.GameController.ExhaustCoroutine(discardCR);
					}

					if (!DidDiscardCards(storedDiscards, 2))
					{
						break;
					}

					SelectTurnTakerDecision selectTurnTakerDecision = storedTurnTaker.FirstOrDefault();
					if (selectTurnTakerDecision != null && selectTurnTakerDecision.SelectedTurnTaker.IsHero)
					{
						IEnumerator drawCR = DrawCards(
							FindHeroTurnTakerController(selectTurnTakerDecision.SelectedTurnTaker.ToHero()),
							2,
							optional: true
						);
						if (base.UseUnityCoroutines)
						{
							yield return base.GameController.StartCoroutine(drawCR);
						}
						else
						{
							base.GameController.ExhaustCoroutine(drawCR);
						}
					}
					break;

				case 2:
					// Destroy a target with 1 HP.
					IEnumerator destroyCR = GameController.SelectAndDestroyCard(
						base.HeroTurnTakerController,
						new LinqCardCriteria(
							(Card c) => c.IsTarget && c.HitPoints.Value == 1,
							"targets with 1 HP",
							useCardsSuffix: false
						),
						optional: false,
						cardSource: GetCardSource()
					);
					if (base.UseUnityCoroutines)
					{
						yield return base.GameController.StartCoroutine(destroyCR);
					}
					else
					{
						base.GameController.ExhaustCoroutine(destroyCR);
					}
					break;
			}
			yield break;
		}
	}
}