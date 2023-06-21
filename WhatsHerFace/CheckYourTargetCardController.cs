using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.WhatsHerFace
{
	public class CheckYourTargetCardController : RecallBaseCardController
	{
		/*
		 * Play this card next to a non-character target.
		 * Redirect the first damage dealt by that target each turn to the villain target with the highest HP.
		 * If that target leaves play, return this card to your hand.
		 */

		private const string FirstDamageFromThis = "FirstDamageFromThis";

		public CheckYourTargetCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			// PerformRedirect = null;
			SpecialStringMaker.ShowVillainTargetWithHighestHP();
		}

		// Play this card next to a non-character target.
		protected override LinqCardCriteria CustomCriteria => new LinqCardCriteria(
			(Card c) => !c.IsCharacter && c.IsTarget && c.IsInPlayAndHasGameText,
			"non-character target"
		);

		public override void AddTriggers()
		{
			// Redirect the first damage dealt by that target each turn to the villain target with the highest HP.
			AddFirstTimePerTurnRedirectTrigger(
				(DealDamageAction dd) =>
					dd.DamageSource.IsTarget
					&& dd.DamageSource.Card == GetCardThisCardIsNextTo(),
				FirstDamageFromThis,
				TargetType.HighestHP,
				(Card c) => IsVillainTarget(c) && GameController.IsCardVisibleToCardSource(c, GetCardSource())
			);

			// If that target leaves play, return this card to your hand.
			AddIfTheCardThatThisCardIsNextToLeavesPlayMoveItToYourHandTrigger();

			AddAfterLeavesPlayAction(
				(GameAction ga) => ResetFlagAfterLeavesPlay(FirstDamageFromThis),
				TriggerType.Hidden
			);

			base.AddTriggers();
		}

	}
}