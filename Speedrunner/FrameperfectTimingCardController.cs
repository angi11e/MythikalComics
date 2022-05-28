using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Speedrunner
{
	public class FrameperfectTimingCardController : SpeedrunnerBaseCardController
	{
		/*
		 * When {Speedrunner} would be dealt damage,
		 *  you may redirect that damage to the villain target with the lowest HP.
		 * If you do so, you may either increase it by 2, draw 1 card, or play 1 card.
		 * Then destroy this card.
		 * 
		 * POWER
		 * {Speedrunner} deals 1 target 1 irreducible melee damage.
		 *  This damage cannot be redirected.
		 */

		public FrameperfectTimingCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			this.AllowFastCoroutinesDuringPretend = false;
		}

		public override void AddTriggers()
		{
			// When {Speedrunner} would be dealt damage, you may redirect that damage to the villain target with the lowest HP.
			AddTrigger(
				(DealDamageAction dd) =>
					dd.Target == this.CharacterCard
					&& dd.Amount > 0,
				RedirectResponse,
				TriggerType.RedirectDamage,
				TriggerTiming.Before,
				ActionDescription.DamageTaken
			);

			this.AddTriggers();
		}

		private IEnumerator RedirectResponse(DealDamageAction dda)
		{
			var storedYesNo = new List<YesNoCardDecision> { };
			IEnumerator yesOrNoCR = GameController.MakeYesNoCardDecision(
				DecisionMaker,
				SelectionType.RedirectDamage,
				dda.Target,
				action: dda,
				storedResults: storedYesNo,
				cardSource: GetCardSource()
			);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(yesOrNoCR);
			}
			else
			{
				GameController.ExhaustCoroutine(yesOrNoCR);
			}

			if (DidPlayerAnswerYes(storedYesNo))
			{
				// If you do so...
				IEnumerator redirectCR = RedirectDamage(
					dda,
					TargetType.LowestHP,
					(Card c) => c.IsTarget && c.IsVillain
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(redirectCR);
				}
				else
				{
					GameController.ExhaustCoroutine(redirectCR);
				}

				// ...you may either...
				List<Function> functionList = new List<Function>();

				// ...increase it by 2
				functionList.Add(
					new Function(
						DecisionMaker,
						"increase damage by 2",
						SelectionType.IncreaseDamage,
						() =>  GameController.IncreaseDamage(
							dda,
							2,
							cardSource: GetCardSource()
						)
					)
				);

				// ...draw 1 card
				functionList.Add(
					new Function(
						DecisionMaker,
						"Draw 1 card",
						SelectionType.DrawCard,
						() => GameController.DrawCards(
							HeroTurnTakerController,
							1
						)
					)
				);

				// ...or play 1 card
				functionList.Add(
					new Function(
						DecisionMaker,
						"Play 1 card",
						SelectionType.PlayCard,
						() => SelectAndPlayCardFromHand(
							HeroTurnTakerController,
							false
						),
						onlyDisplayIfTrue: this.HeroTurnTaker.HasCardsInHand
					)
				);

				SelectFunctionDecision selectFunction = new SelectFunctionDecision(
					GameController,
					DecisionMaker,
					functionList,
					false,
					cardSource: GetCardSource()
				);

				IEnumerator selectFunctionCR = GameController.SelectAndPerformFunction(selectFunction);
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(selectFunctionCR);
				}
				else
				{
					GameController.ExhaustCoroutine(selectFunctionCR);
				}

				// Then destroy this card.
				IEnumerator destroyCR = GameController.DestroyCard(
					DecisionMaker,
					this.Card,
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
			}

			yield break;
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int targetNumeral = GetPowerNumeral(0, 1);
			int damageNumeral = GetPowerNumeral(1, 1);

			// This damage cannot be redirected.
			var noRedirectTrigger = AddMakeDamageNotRedirectableTrigger(
				(DealDamageAction dda) => dda.DamageSource.IsSameCard(this.CharacterCard)
			);

			// {Speedrunner} deals 1 target 1 irreducible melee damage.
			IEnumerator damageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				damageNumeral,
				DamageType.Melee,
				targetNumeral,
				optional: false,
				targetNumeral,
				isIrreducible: true,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(damageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(damageCR);
			}

			RemoveTrigger(noRedirectTrigger);

			yield break;
		}
	}
}