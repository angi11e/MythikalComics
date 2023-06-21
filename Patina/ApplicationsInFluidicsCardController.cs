using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Patina
{
	public class ApplicationsInFluidicsCardController : PatinaBaseCardController
	{
		/*
		 * whenever a water card is destroyed, do one of the following:
		 * { 1 target regains 1 HP.
		 * { 1 player draws 1 card.
		 * { {Patina} deals 1 target 1 cold or melee damage.{BR}
		 */

		public ApplicationsInFluidicsCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// whenever a water card is destroyed...
			AddTrigger(
				(DestroyCardAction d) => IsWater(d.CardToDestroy.Card) && d.WasCardDestroyed,
				DestructionResponse,
				new List<TriggerType> {
					TriggerType.GainHP,
					TriggerType.DrawCard,
					TriggerType.DealDamage
				},
				TriggerTiming.After
			);

			base.AddTriggers();
		}

		private IEnumerator DestructionResponse(DestroyCardAction dd)
		{
			// do one of the following:
			List<Function> functionList = new List<Function>();

			// { 1 target regains 1 HP.
			functionList.Add(
				new Function(
					DecisionMaker,
					"1 target regains 1 hp",
					SelectionType.GainHP,
					() => GameController.SelectAndGainHP(
						DecisionMaker,
						1,
						cardSource: GetCardSource()
					)
				)
			);

			// { 1 player draws 1 card.
			functionList.Add(
				new Function(
					DecisionMaker,
					"1 player draws 1 card",
					SelectionType.DrawCard,
					() => GameController.SelectHeroToDrawCard(
						DecisionMaker,
						optionalDrawCard: false,
						cardSource: GetCardSource()
					)
				)
			);

			// { {Patina} deals 1 target 1 cold or melee damage.
			functionList.Add(
				new Function(
					DecisionMaker,
					"[i]Patina[/i] deals 1 target 1 cold or melee damage",
					SelectionType.DealDamage,
					() => SplashResponse()
				)
			);

			SelectFunctionDecision selectFunction = new SelectFunctionDecision(
				GameController,
				DecisionMaker,
				functionList,
				false,
				associatedCards: new Card[1] { dd.CardToDestroy.Card },
				cardSource: GetCardSource()
			);

			IEnumerator selectFunctionCR = GameController.SelectAndPerformFunction(
				selectFunction,
				associatedCards: new Card[1] { dd.CardToDestroy.Card }
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selectFunctionCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selectFunctionCR);
			}

			yield break;
		}

		private IEnumerator SplashResponse()
		{
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
					1,
					damageType.Value,
					1,
					false,
					1,
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