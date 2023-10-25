using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Nexus
{
	public abstract class NexusOneShotCardController : CardController
	{
		/*
		 * {Nexus} deals 1 target 2 [firstDamage] damage
		 * and 1 different target 2 [secondDamage] damage, in either order.
		 */

		private readonly DamageType _firstDamage;
		private readonly DamageType _secondDamage;

		public NexusOneShotCardController(
			Card card,
			TurnTakerController turnTakerController,
			DamageType firstDamage,
			DamageType secondDamage
		) : base(card, turnTakerController)
		{
			_firstDamage = firstDamage;
			_secondDamage = secondDamage;
		}

		public override IEnumerator Play()
		{
			List<SelectDamageTypeDecision> chosenType = new List<SelectDamageTypeDecision>();
			IEnumerator chooseDamageCR = GameController.SelectDamageType(
				DecisionMaker,
				chosenType,
				new DamageType[] { _firstDamage, _secondDamage },
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
			).SelectedDamageType ?? _firstDamage;

			List<DealDamageAction> theTarget = new List<DealDamageAction>();
			IEnumerator damageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				2,
				damageType,
				1,
				false,
				1,
				storedResultsDamage: theTarget,
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

			Card notTheTarget = null;
			if (theTarget.Any())
			{
				notTheTarget = theTarget.FirstOrDefault().Target;
				if (!notTheTarget.IsInPlayAndHasGameText || notTheTarget.IsIncapacitatedOrOutOfGame)
				{
					notTheTarget = null;
				}
			}

			// {Starblade} deals 1 different target 1 energy damage.
			IEnumerator secondDamageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				2,
				damageType == _firstDamage ? _secondDamage : _firstDamage,
				1,
				false,
				1,
				additionalCriteria: (Card c) => notTheTarget == null || c != notTheTarget,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(secondDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(secondDamageCR);
			}

			yield break;
		}
	}
}