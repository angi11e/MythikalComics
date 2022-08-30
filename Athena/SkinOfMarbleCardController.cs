using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Athena
{
	public class SkinOfMarbleCardController : AthenaBaseCardController
	{
		/*
		 * Reduce damage dealt to {Athena} by 1.
		 * 
		 * After {Athena} is dealt damage by a non-Hero target,
		 *  if there is an [u]aspect[/u] card in play, she deals that target 1 melee Damage.
		 */

		public SkinOfMarbleCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// Reduce damage dealt to {Athena} by 1.
			AddReduceDamageTrigger((Card c) => c == this.CharacterCard, 1);

			// After {Athena} is dealt damage by a non-Hero target,
			// if there is an [u]aspect[/u] card in play, she deals that target 1 melee Damage.
			AddCounterDamageTrigger(
				(DealDamageAction dd) =>
					dd.Target == this.CharacterCard
					&& dd.DamageSource.IsTarget
					&& !dd.DamageSource.IsHero
					&& dd.DidDealDamage
					&& AspectInPlay,
				() => this.CharacterCard,
				() => this.CharacterCard,
				oncePerTargetPerTurn: false,
				1,
				DamageType.Melee
			);

			base.AddTriggers();
		}
	}
}