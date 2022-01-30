using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.NightMare
{
	public class HoldYerHorsesCardController : NightMareBaseCardController
	{
		/*
		 * Reduce damage taken by {NightMare} by 1.
		 * When any damage is redirected to {NightMare}, reduce it by 1.
		 * 
		 * POWER
		 * {NightMare} deals 1 target 1 Melee damage.
		 * Targets dealt Damage this way cannot deal Damage until the start of your next turn.
		 * 
		 * DISCARD
		 * One hero target regains 1 HP.
		 */

		private ITrigger _reduceTrigger;

		public HoldYerHorsesCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// Reduce damage taken by {NightMare} by 1.
			AddReduceDamageTrigger((Card c) => c == base.CharacterCard, 1);

			// When any damage is redirected to {NightMare}, reduce it by 1.
			_reduceTrigger = AddTrigger(
				(RedirectDamageAction rda) => rda.NewTarget == base.CharacterCard,
				(RedirectDamageAction rda) => GameController.ReduceDamage(
					rda.DealDamageAction,
					1,
					_reduceTrigger,
					GetCardSource()
				),
				TriggerType.ReduceDamage,
				TriggerTiming.Before
			);

			base.AddTriggers();
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int targetNumeral = GetPowerNumeral(0, 1);
			int damageNumeral = GetPowerNumeral(1, 1);

			// {NightMare} deals 1 target 1 Melee damage.
			IEnumerator dealDamageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, base.CharacterCard),
				damageNumeral,
				DamageType.Melee,
				targetNumeral,
				false,
				targetNumeral,
				// Targets dealt Damage this way cannot deal Damage until the start of your next turn.
				addStatusEffect: TargetsDealtDamageCannotDealDamageUntilTheStartOfNextTurnResponse,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(dealDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(dealDamageCR);
			}

			yield break;
		}

		protected override IEnumerator DiscardResponse(GameAction ga)
		{
			// One hero target regains 1 HP.
			List<SelectTargetDecision> selectedTarget = new List<SelectTargetDecision>();
			IEnumerable<Card> choices = FindCardsWhere(new LinqCardCriteria((Card c) =>
				c.IsTarget && c.IsInPlayAndHasGameText && c.IsHero
			));
			IEnumerator selectTargetCR = GameController.SelectTargetAndStoreResults(
				DecisionMaker,
				choices,
				selectedTarget,
				selectionType: SelectionType.GainHP,
				cardSource: GetCardSource()
			);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selectTargetCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selectTargetCR);
			}

			if (selectedTarget != null && selectedTarget.Any())
			{
				SelectTargetDecision selectedTargetDecision = selectedTarget.FirstOrDefault();
				if (selectedTargetDecision != null && selectedTargetDecision.SelectedCard != null)
				{
					IEnumerator healTargetCR = GameController.GainHP(
						selectedTarget.FirstOrDefault().SelectedCard,
						1,
						cardSource: GetCardSource()
					);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(healTargetCR);
					}
					else
					{
						GameController.ExhaustCoroutine(healTargetCR);
					}
				}
			}

			yield break;
		}
	}
}