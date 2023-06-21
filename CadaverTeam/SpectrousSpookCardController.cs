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
	public class SpectrousSpookCardController : CardController
	{
		private const string _FirstDamage = "FirstDamage";

		public SpectrousSpookCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
			SpecialStringMaker.ShowHasBeenUsedThisTurn(
				_FirstDamage,
				"{0} has already spooked someone this turn.",
				"{0} has not yet spooked someone this turn."
			);
		}

		public override void AddTriggers()
		{
			// The first time each turn a villain target deals damage to a hero target...
			AddTrigger(
				(DealDamageAction dda) =>
					dda.DamageSource.IsVillainTarget
					&& IsHero(dda.Target)
					&& !IsPropertyTrue(_FirstDamage),
				TeamworkResponse,
				TriggerType.DealDamage,
				TriggerTiming.After
			);

			AddAfterLeavesPlayAction(
				(GameAction ga) => ResetFlagAfterLeavesPlay(_FirstDamage),
				TriggerType.Hidden
			);

			base.AddTriggers();
		}

		private IEnumerator TeamworkResponse(DealDamageAction dda)
		{
			SetCardPropertyToTrueIfRealAction(_FirstDamage);

			// ...this card deals that hero target 1 infernal and 1 toxic damage.
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
					DamageType.Toxic
				)
			};

			IEnumerator dealDamageCR = DealMultipleInstancesOfDamage(
				retributionDamage,
				(Card c) => dda.Target == c
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
