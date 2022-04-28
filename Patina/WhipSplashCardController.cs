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
		 * Whenever a villain card would be played, you may destroy this card.
		 * If you do so, one player other than {Patina} may draw a card instead.
		 * 
		 * POWER
		 * Deal 1 target X cold or melee damage, where X = the number of water cards in play plus 1.
		 */

		public WhipSplashCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			SpecialStringMaker.ShowNumberOfCardsInPlay(IsWaterCriteria());
		}

		public override void AddTriggers()
		{
			// Whenever a villain card would be played...
			AddTrigger(
				(PlayCardAction pc) => IsVillain(pc.CardToPlay) && !pc.IsPutIntoPlay,
				DestructionResponse,
				new TriggerType[3]
				{
					TriggerType.DestroySelf,
					TriggerType.CancelAction,
					TriggerType.DrawCard
				},
				TriggerTiming.Before,
				orderMatters: true
			);

			base.AddTriggers();
		}

		private IEnumerator DestructionResponse(PlayCardAction p)
		{
			// ...you may destroy this card.
			List<YesNoCardDecision> storedResults = new List<YesNoCardDecision>();
			IEnumerator yesNoCR = GameController.MakeYesNoCardDecision(
				DecisionMaker,
				SelectionType.DestroySelf,
				this.Card,
				storedResults: storedResults,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(yesNoCR);
			}
			else
			{
				GameController.ExhaustCoroutine(yesNoCR);
			}

			// If you do so...
			if (DidPlayerAnswerYes(storedResults))
			{
				// ...one player other than {Patina} may draw a card instead.
				IEnumerator cancelCR = CancelAction(p);
				IEnumerator grantDrawCR = GameController.SelectHeroToDrawCard(
					DecisionMaker,
					additionalCriteria: new LinqTurnTakerCriteria(
						(TurnTaker tt) => !tt.IsIncapacitatedOrOutOfGame && tt != this.TurnTaker
					),
					cardSource: GetCardSource()
				);
				IEnumerator destroyCR = GameController.DestroyCard(
					DecisionMaker,
					this.Card,
					optional: false,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(cancelCR);
					yield return GameController.StartCoroutine(grantDrawCR);
					yield return GameController.StartCoroutine(destroyCR);
				}
				else
				{
					GameController.ExhaustCoroutine(cancelCR);
					GameController.ExhaustCoroutine(grantDrawCR);
					GameController.ExhaustCoroutine(destroyCR);
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
				// ...where X = the number of water cards in play plus 1.
				int damageNumeral = FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && IsWater(c)).Count() + modNumeral;

				IEnumerator strikeCR = GameController.SelectTargetsAndDealDamage(
					DecisionMaker,
					new DamageSource(GameController, this.CharacterCard),
					damageNumeral,
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