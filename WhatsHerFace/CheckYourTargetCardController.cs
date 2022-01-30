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
		 * Play this card next to a non-hero target.
		 * Redirect the first damage dealt by that target each turn to the villain target with the highest HP.
		 * If that target leaves play, return this card to your hand.
		 */

		private ITrigger _redirectTrigger;
		private const string FirstDamageFromThis = "FirstDamageFromThis";
		public bool? PerformRedirect { get; set; }

		public override bool AllowFastCoroutinesDuringPretend {
			get
			{
				if (!GameController.PreviewMode)
				{
					return IsHighestHitPointsUnique((Card c) => IsVillainTarget(c));
				}
				return true;
			}
		}

		public CheckYourTargetCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			PerformRedirect = null;
			base.SpecialStringMaker.ShowVillainTargetWithHighestHP();
		}

		// Play this card next to a non-hero target.
		protected override LinqCardCriteria CustomCriteria => new LinqCardCriteria(
			(Card c) => !c.IsHero && c.IsTarget && c.IsInPlayAndHasGameText,
			"non-hero target"
		);

		public override void AddTriggers()
		{
			// Redirect the first damage dealt by that target each turn to the villain target with the highest HP.
			_redirectTrigger = AddTrigger(
				(DealDamageAction dd) =>
					dd.DamageSource.IsCard
					&& dd.DamageSource.Card == GetCardThisCardIsNextTo()
					&& !IsPropertyTrue(FirstDamageFromThis),
				RedirectToHighest,
				TriggerType.RedirectDamage,
				TriggerTiming.Before
			);

			// If that target leaves play, return this card to your hand.
			AddIfTheCardThatThisCardIsNextToLeavesPlayMoveItToYourHandTrigger();

			AddAfterLeavesPlayAction(
				(GameAction ga) => ResetFlagAfterLeavesPlay(FirstDamageFromThis),
				TriggerType.Hidden
			);

			base.AddTriggers();
		}

		private IEnumerator RedirectToHighest(DealDamageAction dd)
		{
			SetCardPropertyToTrueIfRealAction(FirstDamageFromThis);
			Card theTarget = null;

			// using OverchargedNullShield as an example
			if (GameController.PretendMode)
			{
				// find villain target with highest hp
				List<Card> villainList = new List<Card>();
				IEnumerator findVillainCR = GameController.FindTargetWithHighestHitPoints(
					1,
					(Card c) => c.IsVillain,
					villainList,
					cardSource: GetCardSource()
				);

				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(findVillainCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(findVillainCR);
				}

				if (villainList.Count() > 0)
				{
					theTarget = villainList.FirstOrDefault();
				}
				else
				{
					PerformRedirect = null;
				}
			}
			if (PerformRedirect.HasValue && PerformRedirect.Value && theTarget.IsTarget)
			{
				// redirect to that target
				IEnumerator redirectCR = GameController.RedirectDamage(
					dd,
					theTarget,
					cardSource: GetCardSource()
				);

				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(redirectCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(redirectCR);
				}
			}
			if (!GameController.PretendMode)
			{
				PerformRedirect = null;
			}

			yield break;
		}
	}
}