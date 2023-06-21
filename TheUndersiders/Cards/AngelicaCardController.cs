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
			SpecialStringMaker.ShowHasBeenUsedThisTurn(
				FirstDamageToVCC,
				"{0} has already tanked damage this turn.",
				"{0} has not yet tanked damage this turn."
			).Condition = () => this.Card.IsInPlayAndHasGameText && IsEnabled("dog");

			SpecialStringMaker.ShowListOfCards(new LinqCardCriteria(
				(Card c) => c.IsInPlayAndNotUnderCard && c.IsTarget && c.HitPoints <= 2 && !IsVillainTarget(c),
				"non-villain targets with 2 HP or less"
			)).Condition = () => IsEnabled("tattle");

			SpecialStringMaker.ShowSpecialString(() => GetSpecialStringIcons("dog", "tattle"));
		}

		public override void AddTriggers()
		{
			// At the end of the villain turn, this card deals each non-villain target X melee damage,
			// where X = the number of dog cards in play.
			AddDealDamageAtEndOfTurnTrigger(
				this.TurnTaker,
				this.Card,
				(Card c) => c.IsTarget && !IsVillainTarget(c),
				TargetType.All,
				FindCardsWhere(new LinqCardCriteria(
					(Card c) => c.DoKeywordsContain("dog") && c.IsInPlayAndHasGameText && !c.IsOneShot
				)).Count(),
				DamageType.Melee
			);

			// Dog: The first time each turn a villain character card would be dealt damage, redirect it to this card.
			AddFirstTimePerTurnRedirectTrigger(
				(DealDamageAction dd) => dd.Target.IsVillainCharacterCard && IsEnabled("dog"),
				FirstDamageToVCC,
				TargetType.HighestHP,
				(Card c) => c == this.Card
			);

			AddAfterLeavesPlayAction(
				(GameAction ga) => ResetFlagAfterLeavesPlay(FirstDamageToVCC),
				TriggerType.Hidden
			);

			// Tattle: At the end of the villain turn, destroy each non-villain target with 2 HP or less.
			AddEndOfTurnTrigger(
				(TurnTaker tt) => tt == this.TurnTaker && IsEnabled("tattle"),
				(PhaseChangeAction p) => GameController.DestroyCards(
					DecisionMaker,
					new LinqCardCriteria((Card c) =>
						c.IsTarget
						&& c.HitPoints.HasValue
						&& c.HitPoints.Value <= 2
						&& !IsVillainTarget(c)
					),
					cardSource: GetCardSource()
				),
				TriggerType.DestroyCard
			);

			base.AddTriggers();
		}
	}
}
