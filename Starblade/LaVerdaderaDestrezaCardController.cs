using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Starblade
{
	public class LaVerdaderaDestrezaCardController : PosturaBaseCardController
	{
		/*
		 * when this card enters play, destroy your other postura cards,
		 * then put a [i]rapier and buckler[/i] into play from your trash.
		 * 
		 * the first time {Starblade} is dealt damage each turn, draw 1 card.
		 * 
		 * POWER
		 * {Starblade} deals 1 target 1 melee damage.
		 * reduce the next damage dealt by a target dealt damage this way by 2.
		 * activate a [u]technique[/u] text.
		 */

		private const string FirstTimeWouldBeDealtDamage = "FirstTimeWouldBeDealtDamage";

		public LaVerdaderaDestrezaCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController, "RapierAndBuckler")
		{
			SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstTimeWouldBeDealtDamage, null, null, null);
		}

		private int _reduceNumeral;

		public override void AddTriggers()
		{
			// the first time {Starblade} is dealt damage each turn...
			AddTrigger(
				(DealDamageAction dd) =>
					dd.DidDealDamage
					&& dd.Target == this.CharacterCard
					&& !IsPropertyTrue(FirstTimeWouldBeDealtDamage),
				DamageDrawResponse,
				TriggerType.DrawCard,
				TriggerTiming.After
			);

			AddAfterLeavesPlayAction(
				(GameAction ga) => ResetFlagAfterLeavesPlay(FirstTimeWouldBeDealtDamage),
				TriggerType.Hidden
			);

			base.AddTriggers();
		}

		private IEnumerator DamageDrawResponse(DealDamageAction dda)
		{
			SetCardPropertyToTrueIfRealAction(FirstTimeWouldBeDealtDamage);

			// ...draw 1 card.
			IEnumerator drawCR = DrawCards(this.HeroTurnTakerController, 1);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(drawCR);
			}
			else
			{
				GameController.ExhaustCoroutine(drawCR);
			}

			yield break;
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int targetNumeral = GetPowerNumeral(0, 1);
			int damageNumeral = GetPowerNumeral(1, 1);
			_reduceNumeral = GetPowerNumeral(2, 2);

			// {Starblade} deals 1 target 1 melee damage.
			IEnumerator dealDamageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				damageNumeral,
				DamageType.Melee,
				targetNumeral,
				false,
				targetNumeral,
				// reduce the next damage dealt by a target dealt damage this way by 2.
				addStatusEffect: ReduceNextDamageResponse,
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

			// activate a [u]technique[/u] text.
			IEnumerator activateCR = GameController.SelectAndActivateAbility(
				DecisionMaker,
				"technique",
				optional: false,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(activateCR);
			}
			else
			{
				GameController.ExhaustCoroutine(activateCR);
			}

			yield break;
		}

		private IEnumerator ReduceNextDamageResponse(DealDamageAction dda)
		{
			// reduce the next damage dealt by a target dealt damage this way by 2.
			if (dda.DidDealDamage)
			{
				ReduceDamageStatusEffect reduceDamageSE = new ReduceDamageStatusEffect(_reduceNumeral);
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
	}
}