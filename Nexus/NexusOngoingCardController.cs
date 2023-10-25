using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Nexus
{
	public abstract class NexusOngoingCardController : CardController
	{
		/*
		 * increase [baseDamage] and [upgradeDamage] damage by 1.
		 * 
		 * whenever {Nexus} deals [baseDamage] damage to a target, X
		 */

		private readonly DamageType _baseDamage;
		private readonly DamageType _upgradeDamage;
		private readonly TriggerType[] _riderTriggers;

		public NexusOngoingCardController(
			Card card,
			TurnTakerController turnTakerController,
			DamageType baseDamage,
			DamageType upgradeDamage,
			TriggerType[] riderTriggers
		) : base(card, turnTakerController)
		{
			_baseDamage = baseDamage;
			_upgradeDamage = upgradeDamage;
			_riderTriggers = riderTriggers;
		}

		public override void AddTriggers()
		{
			// increase [baseDamage] and [upgradeDamage] damage by 1.
			AddIncreaseDamageTrigger(
				(DealDamageAction dda) => dda.DamageType == _baseDamage || dda.DamageType == _upgradeDamage,
				1
			);

			// whenever {Nexus} deals [baseDamage] damage to a target, X
			AddTrigger(
				(DealDamageAction dd) =>
					dd.DidDealDamage
					&& dd.DamageSource.IsCard
					&& dd.DamageSource.Card == this.CharacterCard
					&& dd.DamageType == _baseDamage,
				BaseDamageRewardResponse,
				_riderTriggers,
				TriggerTiming.After
			);

			base.AddTriggers();
		}

		protected abstract IEnumerator BaseDamageRewardResponse(DealDamageAction dd);
	}
}