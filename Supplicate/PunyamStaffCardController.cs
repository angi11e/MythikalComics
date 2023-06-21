using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Supplicate
{
	public class PunyamStaffCardController : SupplicateBaseCardController
	{
		/*
		 * the first time any yaojing card deals damage each turn,
		 * {Supplicate} deals 1 target 2 radiant damage.
		 * 
		 * POWER:
		 * {Supplicate} deals 1 target 1 melee damage.
		 * a yaojing card deals 1 target 2 psychic damage.
		 */

		private const string FirstDamageYaojingPerTurn = "FirstDamageYaojingPerTurn";
		private ITrigger _teamDamage;

		public PunyamStaffCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			_teamDamage = null;
			AllowFastCoroutinesDuringPretend = false;
		}

		public override void AddTriggers()
		{
			base.AddTriggers();

			// the first time each yaojing card deals damage each turn,
			_teamDamage = AddTrigger(
				(DealDamageAction dda) => dda.DamageSource.IsTarget
					&& IsYaojing(dda.DamageSource.Card)
					// && !IsPropertyTrue(GeneratePerTargetKey(FirstDamageYaojingPerTurn, dda.DamageSource.Card))
					&& !IsPropertyTrue(FirstDamageYaojingPerTurn)
					&& dda.Amount > 0,
				TeamDamageResponse,
				TriggerType.DealDamage,
				TriggerTiming.After
			);

			AddAfterLeavesPlayAction(
				(GameAction ga) => ResetFlagsAfterLeavesPlay(FirstDamageYaojingPerTurn),
				TriggerType.Hidden
			);
		}

		private IEnumerator TeamDamageResponse(DealDamageAction dda)
		{
			SetCardPropertyToTrueIfRealAction(FirstDamageYaojingPerTurn);

			// {Supplicate} deals 1 target 2 radiant damage.
			IEnumerator damageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				2,
				DamageType.Radiant,
				1,
				false,
				1,
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

			yield break;
		}

		public override IEnumerator UsePower(int index = 0)
		{
			// {Supplicate} deals 1 target 1 melee damage.
			int suppTargetNumeral = GetPowerNumeral(0, 1);
			int suppDamageNumeral = GetPowerNumeral(1, 1);
			int yaoTargetNumeral = GetPowerNumeral(2, 1);
			int yaoDamageNumeral = GetPowerNumeral(3, 2);

			IEnumerator suppDamageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				suppDamageNumeral,
				DamageType.Melee,
				suppTargetNumeral,
				false,
				suppTargetNumeral,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(suppDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(suppDamageCR);
			}

			// a yaojing card deals 1 target 2 psychic damage.
			List<SelectCardDecision> storedResult = new List<SelectCardDecision>();
			IEnumerator pickTargetCR = GameController.SelectCardAndStoreResults(
				DecisionMaker,
				SelectionType.SelectTargetFriendly,
				new LinqCardCriteria(
					(Card c) => c.IsTarget && c.IsInPlayAndNotUnderCard && IsYaojing(c),
					"yaojing",
					useCardsSuffix: false
				),
				storedResult,
				optional: false,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(pickTargetCR);
			}
			else
			{
				GameController.ExhaustCoroutine(pickTargetCR);
			}

			SelectCardDecision selection = storedResult.FirstOrDefault();
			if (selection != null && selection.SelectedCard != null)
			{
				IEnumerator yaojingDamageCR = GameController.SelectTargetsAndDealDamage(
					DecisionMaker,
					new DamageSource(GameController, selection.SelectedCard),
					yaoDamageNumeral,
					DamageType.Psychic,
					yaoTargetNumeral,
					false,
					yaoTargetNumeral,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(yaojingDamageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(yaojingDamageCR);
				}
			}

			yield break;
		}
	}
}