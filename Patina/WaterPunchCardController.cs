using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Patina
{
	public class WaterPunchCardController : PatinaBaseCardController
	{
		/*
		 * {Patina} deals 1 target 1 melee damage and 1 cold damage, in either order.
		 * 
		 * A Non-Character Target dealt damage this way loses any End of Turn
		 *  effects on its card until the start of {Patina}'s next turn.
		 */

		public WaterPunchCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// {Patina} deals 1 target 1 melee damage and 1 cold damage.
			List<SelectTargetDecision> selectedTarget = new List<SelectTargetDecision>();
			IEnumerable<Card> choices = FindCardsWhere(
				new LinqCardCriteria((Card c) => c.IsTarget && c.IsInPlayAndHasGameText)
			);
			IEnumerator selectTargetCR = GameController.SelectTargetAndStoreResults(
				this.HeroTurnTakerController,
				choices,
				selectedTarget,
				selectionType: SelectionType.SelectTarget,
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
					Card theCard = selectedTargetDecision.SelectedCard;

					// ...in either order.
					List<SelectDamageTypeDecision> chosenType = new List<SelectDamageTypeDecision>();
					IEnumerator chooseDamageCR = GameController.SelectDamageType(
						DecisionMaker,
						chosenType,
						new DamageType[] { DamageType.Melee, DamageType.Cold },
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(chooseDamageCR);
					}
					else
					{
						GameController.ExhaustCoroutine(chooseDamageCR);
					}

					DamageType damageType = chosenType.First(
						(SelectDamageTypeDecision d) => d.Completed
					).SelectedDamageType ?? DamageType.Melee;

					IEnumerator dealMeleeCR = DealDamage(
						this.CharacterCard,
						(Card c) => c == theCard,
						1,
						DamageType.Melee,
						addStatusEffect: RemoveEndOfTurnEffect
					);
					IEnumerator dealColdCR = DealDamage(
						this.CharacterCard,
						(Card c) => c == theCard,
						1,
						DamageType.Cold,
						addStatusEffect: RemoveEndOfTurnEffect
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine((damageType == DamageType.Melee) ? dealMeleeCR : dealColdCR);
						yield return GameController.StartCoroutine((damageType == DamageType.Melee) ? dealColdCR : dealMeleeCR);
					}
					else
					{
						GameController.ExhaustCoroutine((damageType == DamageType.Melee) ? dealMeleeCR : dealColdCR);
						GameController.ExhaustCoroutine((damageType == DamageType.Melee) ? dealColdCR : dealMeleeCR);
					}
				}
			}

			yield break;
		}

		private IEnumerator RemoveEndOfTurnEffect(DealDamageAction dd)
		{
			// A Non-Character Target dealt damage this way loses any End of Turn
			//  effects on its card until the start of {Patina}'s next turn.
			if (dd != null && dd.DidDealDamage && dd.Target != null && !dd.Target.IsCharacter)
			{
				PreventPhaseEffectStatusEffect preventPhaseEffectStatusEffect = new PreventPhaseEffectStatusEffect();
				preventPhaseEffectStatusEffect.UntilStartOfNextTurn(this.TurnTaker);
				preventPhaseEffectStatusEffect.CardCriteria.IsSpecificCard = dd.Target;

				var foundEffect = Game.StatusEffects.OfType<PreventPhaseEffectStatusEffect>().FirstOrDefault(
					ppese => ppese.IsRedundant(new List<StatusEffect> { preventPhaseEffectStatusEffect })
				);

				if (foundEffect == null)
				{
					IEnumerator preventionCR = AddStatusEffect(preventPhaseEffectStatusEffect);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(preventionCR);
					}
					else
					{
						GameController.ExhaustCoroutine(preventionCR);
					}
				}
			}

			yield break;
		}
	}
}