﻿using System;
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
		// Reduce the next damage taken by that hero by 2.
		// That hero gains the [b]power:[/b] destroy this card.
		// Before this card is destroyed, the hero it's next to regains 4 hp.
		// All other hero targets regain 1 hp.

		public WeatherTheHitsCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		protected override TriggerType[] DestructionTriggers => new TriggerType[1] {
			TriggerType.GainHP
		};

		public override IEnumerator Play()
		{
			ReduceDamageStatusEffect reduceDamageSE = new ReduceDamageStatusEffect(2);
			reduceDamageSE.NumberOfUses = 1;
			reduceDamageSE.TargetCriteria.IsSpecificCard = CharmedHero();
			reduceDamageSE.CardDestroyedExpiryCriteria.Card = CharmedHero();

			return AddStatusEffect(reduceDamageSE);
		}

		protected override IEnumerator CharmDestroyResponse(GameAction ga)
		{
			HeroTurnTakerController httc = FindHeroTurnTakerController(CharmedHero().Owner.ToHero());

			// heal this hero for 4
			IEnumerator healTargetCR = GameController.GainHP(
				CharmedHero(),
				4,
				cardSource: GetCardSource()
			);

			// heal other hero targets for 1
			IEnumerator healTargetsCR = GameController.GainHP(
				httc,
				(Card c) => IsHeroTarget(c) && (c != CharmedHero()),
				1,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(healTargetCR);
				yield return GameController.StartCoroutine(healTargetsCR);
			}
			else
			{
				GameController.ExhaustCoroutine(healTargetCR);
				GameController.ExhaustCoroutine(healTargetsCR);
			}

			yield break;
		}
	}
}