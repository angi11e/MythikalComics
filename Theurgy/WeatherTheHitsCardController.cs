using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Theurgy
{
	internal class WeatherTheHitsCardController : CharmBaseCardController
	{
		// Play this card next to a hero character card.
		// Reduce damage taken by hero targets in that hero's play area by 1.
		// That hero gains the following power:
		// Power: this hero regains 5 hp. All other hero targets regain 1 hp.
		//  Destroy this card.

		public WeatherTheHitsCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		protected override string CharmPowerText => "This hero regains 5 hp. All other hero targets regain 1 hp. Destroy Weather the Hits.";

		public override void AddTriggers()
		{
			base.AddTriggers();

			// reduce damage taken by hero targets in that hero's play area by 1.
			AddReduceDamageTrigger(
				(Card c) =>
					c.IsInLocation(base.Card.Location.OwnerTurnTaker.PlayArea) &&
					c.IsHero,
				1
			);
		}

		protected override IEnumerator CharmPowerResponse(CardController cc)
		{
			int heroHealNumeral = GetPowerNumeral(0, 5);
			int groupHealNumeral = GetPowerNumeral(1, 1);

			HeroTurnTakerController httc = cc.HeroTurnTakerController;

			// should handle both SW Sentinels and Guise
			Card targetHero = GetCardThisCardIsNextTo();
			if (targetHero == null)
			{
				targetHero = base.Card.Location.OwnerTurnTaker.CharacterCard;
			}

			// heal this hero for 5
			IEnumerator healTargetCR = base.GameController.GainHP(
				targetHero,
				heroHealNumeral,
				cardSource: base.GetCardSource()
			);
			if (UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(healTargetCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(healTargetCR);
			}

			// heal other hero targets for 1
			IEnumerator healTargetsCR = base.GameController.GainHP(
				httc,
				(Card c) => c.IsHero && (c != targetHero),
				groupHealNumeral,
				cardSource: base.GetCardSource()
			);
			if (UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(healTargetsCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(healTargetsCR);
			}

			// destroy this card.
			IEnumerator destructionCR = GameController.DestroyCard(
				httc,
				base.Card,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(destructionCR);
			}
			else
			{
				GameController.ExhaustCoroutine(destructionCR);
			}
			yield break;
		}
	}
}