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
	public class AnimatePlushCardController : TheUndersidersBaseCardController
	{
		private const string FirstDamageToThis = "FirstDamageToThis";

		public AnimatePlushCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// At the end of the villain turn, this card deals the hero target with the highest HP {H - 1} melee damage.
			// Spider: Damage dealt by this card is Psychic and irreducible.
			AddDealDamageAtEndOfTurnTrigger(
				TurnTaker,
				base.Card,
				(Card c) => c.IsHero,
				TargetType.HighestHP,
				base.H - 1,
				IsEnabled("spider") ? DamageType.Psychic : DamageType.Melee,
				isIrreducible: IsEnabled("spider")
			);

			// Bear: The first time each turn this card would be dealt damage, redirect it to the environment target with the lowest HP.
			AddTrigger(
				(DealDamageAction dd) =>
					!IsPropertyTrue(FirstDamageToThis)
					&& dd.DidDealDamage,
				RedirectResponse,
				TriggerType.RedirectDamage,
				TriggerTiming.Before
			);

			AddAfterLeavesPlayAction(
				(GameAction ga) => ResetFlagAfterLeavesPlay(FirstDamageToThis),
				TriggerType.Hidden
			);

			base.AddTriggers();
		}

		private IEnumerator RedirectResponse(DealDamageAction dd)
		{
			SetCardPropertyToTrueIfRealAction(FirstDamageToThis);

			if (IsEnabled("bear"))
			{
				List<Card> storedResults = new List<Card>();
				IEnumerator findEnvironmentCR = GameController.FindTargetWithLowestHitPoints(
					1,
					(Card c) => c.IsEnvironmentTarget,
					storedResults,
					cardSource: GetCardSource()
				);

				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(findEnvironmentCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(findEnvironmentCR);
				}

				Card newTarget = storedResults.FirstOrDefault();
				if (newTarget != null)
				{
					IEnumerator redirectCR = GameController.RedirectDamage(
						dd,
						newTarget,
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
			}

			yield break;
		}

		public override IEnumerator Play()
		{
			yield break;
		}
	}
}
