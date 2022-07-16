using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using System.Collections;
using Handelabra;

namespace Angille.CadaverTeam
{
	public class PhantomFrightCardController : CardController
	{
		private const string _FirstDamage = "FirstDamage";

		public PhantomFrightCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
			SpecialStringMaker.ShowHasBeenUsedThisTurn(
				_FirstDamage,
				"{0} has already frightened someone this turn.",
				"{0} has not yet frightened someone this turn."
			);
		}

		public override void AddTriggers()
		{
			// The first time each turn a hero target deals damage to a villain target...
			AddTrigger(
				(DealDamageAction dda) =>
					dda.DamageSource.IsHero
					&& dda.Target.IsVillainTarget
					&& !IsPropertyTrue(_FirstDamage),
				RetributionResponse,
				TriggerType.DealDamage,
				TriggerTiming.After
			);

			AddAfterLeavesPlayAction(
				(GameAction ga) => ResetFlagAfterLeavesPlay(_FirstDamage),
				TriggerType.Hidden
			);

			base.AddTriggers();
		}

		private IEnumerator RetributionResponse(DealDamageAction dda)
		{
			SetCardPropertyToTrueIfRealAction(_FirstDamage);

			// ...this card deals that hero target 1 infernal and 1 psychic damage.
			List<DealDamageAction> retributionDamage = new List<DealDamageAction>
			{
				new DealDamageAction(
					GetCardSource(),
					new DamageSource(GameController, this.Card),
					null,
					1,
					DamageType.Infernal
				),
				new DealDamageAction(
					GetCardSource(),
					new DamageSource(GameController, this.Card),
					null,
					1,
					DamageType.Psychic
				)
			};

			IEnumerator dealDamageCR = DealMultipleInstancesOfDamage(
				retributionDamage,
				(Card c) => dda.DamageSource.IsSameCard(c)
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(dealDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(dealDamageCR);
			}

			yield break;
		}
	}
}
