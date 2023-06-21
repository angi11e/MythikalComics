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
		 * 
		 * POWER
		 * {NightMare} deals 1 target 2 Melee damage.
		 * Reduce the next damage dealt by targets dealt damage this way by 1.
		 * 
		 * DISCARD
		 * One hero target regains 1 HP.
		 */

		private int reduceNumeral;

		public HoldYerHorsesCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// Reduce damage taken by {NightMare} by 1.
			AddReduceDamageTrigger((Card c) => c == this.CharacterCard, 1);

			base.AddTriggers();
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int targetNumeral = GetPowerNumeral(0, 1);
			int damageNumeral = GetPowerNumeral(1, 2);
			reduceNumeral = GetPowerNumeral(2, 1);

			// {NightMare} deals 1 target 2 Melee damage.
			IEnumerator dealDamageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				damageNumeral,
				DamageType.Melee,
				targetNumeral,
				false,
				targetNumeral,
				// Reduce the next damage dealt by targets dealt damage this way by 1.
				addStatusEffect: ReduceNextDamageByDamageResponse,
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

		private IEnumerator ReduceNextDamageByDamageResponse(DealDamageAction dda)
		{
			// Reduce the next damage dealt by targets dealt damage this way by the damage they take.
			if (dda.DidDealDamage)
			{
				ReduceDamageStatusEffect reduceDamageSE = new ReduceDamageStatusEffect(reduceNumeral);
				reduceDamageSE.SourceCriteria.IsSpecificCard = dda.Target;
				reduceDamageSE.NumberOfUses = 1;
				reduceDamageSE.UntilCardLeavesPlay(dda.Target);

				IEnumerator reduceDamageCR = AddStatusEffect(reduceDamageSE);
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(reduceDamageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(reduceDamageCR);
				}
			}
		}

		protected override IEnumerator DiscardResponse(GameAction ga)
		{
			// One hero target regains 1 HP.
			List<SelectTargetDecision> selectedTarget = new List<SelectTargetDecision>();
			IEnumerable<Card> choices = FindCardsWhere(new LinqCardCriteria((Card c) =>
				c.IsInPlayAndHasGameText && IsHeroTarget(c)
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