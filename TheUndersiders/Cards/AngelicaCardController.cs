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
	public class AngelicaCardController : TheUndersidersBaseCardController
	{
		private const string FirstDamageToVCC = "FirstDamageToVCC";

		public AngelicaCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
			base.SpecialStringMaker.ShowHasBeenUsedThisTurn(
				FirstDamageToVCC,
				"{0} has already tanked damage this turn.",
				"{0} has not yet tanked damage this turn."
			);
		}

		public override void AddTriggers()
		{
			// At the end of the villain turn, this card deals each non-villain target {H - 2} melee damage.
			AddDealDamageAtEndOfTurnTrigger(
				base.TurnTaker,
				base.Card,
				(Card c) => c.IsNonVillainTarget,
				TargetType.All,
				base.H - 2,
				DamageType.Melee
			);

			// Dog: The first time each turn a villain character card would be dealt damage, redirect it to this card.
			AddFirstTimePerTurnRedirectTrigger(
				(DealDamageAction dd) => dd.Target.IsVillainCharacterCard && IsEnabled("dog"),
				FirstDamageToVCC,
				TargetType.HighestHP,
				(Card c) => c == base.Card
			);

			AddAfterLeavesPlayAction(
				(GameAction ga) => ResetFlagAfterLeavesPlay(FirstDamageToVCC),
				TriggerType.Hidden
			);

			// Tattle: At the end of the villain turn, destroy each non-villain target with 2 HP or less.
			AddEndOfTurnTrigger(
				(TurnTaker tt) => tt == base.TurnTaker && IsEnabled("tattle"),
				(PhaseChangeAction p) => GameController.DestroyCards(
					DecisionMaker,
					new LinqCardCriteria((Card c) =>
						c.IsNonVillainTarget
						&& c.HitPoints.HasValue
						&& c.HitPoints.Value <= 2
					),
					cardSource: GetCardSource()
				),
				TriggerType.DestroyCard
			);

			base.AddTriggers();
		}

		public override IEnumerator Play()
		{
			yield break;
		}
	}
}
