using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Nexus
{
	public abstract class NexusEquipmentCardController : CardController
	{
		/*
		 * the first time each turn {Nexus} deals [baseDamage] damage to a target,
		 * she also deals that target 1 [upgradeDamage] damage.
		 */

		private readonly DamageType _baseDamage;
		private readonly DamageType _upgradeDamage;
		private const string HasDoneExtraDamage = "HasDoneExtraDamage";

		public NexusEquipmentCardController(
			Card card,
			TurnTakerController turnTakerController,
			DamageType baseDamage,
			DamageType upgradeDamage
		) : base(card, turnTakerController)
		{
			_baseDamage = baseDamage;
			_upgradeDamage = upgradeDamage;
		}

		public override void AddTriggers()
		{
			// the first time each turn {Nexus} deals [baseDamage] damage to a target,
			AddTrigger(
				(DealDamageAction dd) =>
					dd.DamageSource.IsCard
					&& dd.DamageSource.Card == this.CharacterCard
					&& dd.DidDealDamage
					&& dd.DamageType == _baseDamage
					&& !HasBeenSetToTrueThisTurn(HasDoneExtraDamage),
				DamageResponse,
				TriggerType.DealDamage,
				TriggerTiming.After
			);

			AddAfterLeavesPlayAction(
				(GameAction ga) => ResetFlagAfterLeavesPlay(HasDoneExtraDamage),
				TriggerType.Hidden
			);

			base.AddTriggers();
		}

		private IEnumerator DamageResponse(DealDamageAction dda)
		{
			SetCardPropertyToTrueIfRealAction(HasDoneExtraDamage);

			if (!dda.DidDestroyTarget)
			{
				// she also deals that target 1 [upgradeDamage] damage.
				IEnumerator strikeCR = DealDamage(
					this.CharacterCard,
					dda.Target,
					1,
					_upgradeDamage,
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