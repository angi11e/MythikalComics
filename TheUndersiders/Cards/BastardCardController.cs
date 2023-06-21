using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using System.Collections;
using Handelabra;

namespace Angille.TheUndersiders
{
	public class BastardCardController : TheUndersidersBaseCardController
	{
		public BastardCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
			SpecialStringMaker.ShowHeroTargetWithLowestHP(1, 2);
			SpecialStringMaker.ShowSpecialString(() => GetSpecialStringIcons("dog", "skull"));
		}

		public override void AddTriggers()
		{
			// At the end of the villain turn, this card deals the 2 hero targets with the lowest hp 2 melee damage each.
			AddEndOfTurnTrigger(
				(TurnTaker tt) => tt == this.TurnTaker,
				(PhaseChangeAction p) => DealDamageToLowestHP(
					this.Card,
					1,
					(Card c) => IsHeroTarget(c),
					(Card c) => 2,
					DamageType.Melee,
					numberOfTargets: 2
				),
				TriggerType.DealDamage
			);

			// Dog: At the start of the villain turn, this card regains 2 HP.
			AddStartOfTurnTrigger(
				(TurnTaker tt) => tt == this.TurnTaker && IsEnabled("dog"),
				(PhaseChangeAction p) => GameController.GainHP(
					this.Card,
					2,
					cardSource: GetCardSource()
				),
				TriggerType.GainHP
			);

			// Skull: Whenever this card deals melee damage to a target, it also deals that target 1 infernal damage.
			AddTrigger(
				(DealDamageAction dd) =>
					dd.DamageSource.IsSameCard(this.Card)
					&& dd.DidDealDamage
					&& dd.DamageType == DamageType.Melee
					&& IsEnabled("skull"),
				(DealDamageAction dd) => DealDamage(
					this.Card,
					dd.Target,
					1,
					DamageType.Infernal,
					cardSource: GetCardSource()
				),
				TriggerType.DealDamage,
				TriggerTiming.After
			);

			base.AddTriggers();
		}
	}
}
