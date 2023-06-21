using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Patina
{
	public class WhipSplashCardController : PatinaBaseCardController
	{
		/*
		 * Whenever a villain card would be played, you may discard a card.
		 * If you do, remove this card from the game instead.
		 * 
		 * POWER
		 * Deal 1 target X cold or melee damage, where X = the number of water cards in play plus 1.
		 */

		public WhipSplashCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			SpecialStringMaker.ShowNumberOfCardsInPlay(IsWaterCriteria((Card c) => !c.IsOneShot));
		}

		public override void AddTriggers()
		{
			// Whenever a villain card would be played...
			AddTrigger(
				(PlayCardAction pc) => IsVillain(pc.CardToPlay) && !pc.IsPutIntoPlay,
				DestructionResponse,
				new TriggerType[3]
				{
					TriggerType.CancelAction,
					TriggerType.DiscardCard,
					TriggerType.RemoveFromGame
				},
				TriggerTiming.Before,
				orderMatters: true
			);

			base.AddTriggers();
		}

		private IEnumerator DestructionResponse(PlayCardAction p)
		{
			// ...you may discard a card.
			List<DiscardCardAction> storedResults = new List<DiscardCardAction>();
			IEnumerator discardCR = GameController.SelectAndDiscardCard(
				DecisionMaker,
				true,
				storedResults: storedResults,
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

			// If you do...
			if (DidDiscardCards(storedResults))
			{
				// ...remove this card from the game instead.
				IEnumerator cancelCR = CancelAction(p);
				IEnumerator moveThisCardCR = GameController.MoveCard(
					this.TurnTakerController,
					this.Card,
					this.TurnTaker.OutOfGame,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(cancelCR);
					yield return GameController.StartCoroutine(moveThisCardCR);
				}
				else
				{
					GameController.ExhaustCoroutine(cancelCR);
					GameController.ExhaustCoroutine(moveThisCardCR);
				}
			}

			yield break;
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int targetNumeral = GetPowerNumeral(0, 1);
			int modNumeral = GetPowerNumeral(1, 1);

			// Deal 1 target X cold or melee damage...
			List<SelectDamageTypeDecision> chosenType = new List<SelectDamageTypeDecision>();
			IEnumerator chooseTypeCR = GameController.SelectDamageType(
				DecisionMaker,
				chosenType,
				new DamageType[] { DamageType.Cold, DamageType.Melee },
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(chooseTypeCR);
			}
			else
			{
				GameController.ExhaustCoroutine(chooseTypeCR);
			}

			DamageType? damageType = GetSelectedDamageType(chosenType);
			if (damageType != null)
			{
				IEnumerator strikeCR = GameController.SelectTargetsAndDealDamage(
					DecisionMaker,
					new DamageSource(GameController, this.CharacterCard),
					// ...where X = the number of water cards in play plus 1.
					WaterCardsInPlay + modNumeral,
					damageType.Value,
					targetNumeral,
					false,
					targetNumeral,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(strikeCR);
				}
				else
				{
					GameController.ExhaustCoroutine(strikeCR);
				}
			}

			yield break;
		}
	}
}