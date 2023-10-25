using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Starblade
{
	public class ValeTudoCardController : PosturaBaseCardController
	{
		/* when this card enters play, destroy your other postura cards,
		 * then put a [i]chain and spikes[/i] into play from your trash.
		 * 
		 * whenever a construct card is destroyed, {Starblade} deals 1 target 1 melee damage.
		 * 
		 * POWER
		 * {Starblade} deals up to 2 targets 2 melee damage each.
		 * activate a [u]technique[/u] text.
		 */

		public ValeTudoCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController, "ChainAndSpikes")
		{
		}

		public override void AddTriggers()
		{
			// whenever a construct card is destroyed...
			AddTrigger(
				(DestroyCardAction d) => d.CardToDestroy.Card.IsConstruct && d.WasCardDestroyed,
				(DestroyCardAction d) => GameController.SelectTargetsAndDealDamage(
					// ...{Starblade} deals 1 target 1 melee damage.
					DecisionMaker,
					new DamageSource(GameController, this.CharacterCard),
					1,
					DamageType.Melee,
					1,
					false,
					1,
					cardSource: GetCardSource()
				),
				new List<TriggerType> { TriggerType.DealDamage },
				TriggerTiming.After
			);

			base.AddTriggers();
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int targetNumeral = GetPowerNumeral(0, 2);
			int damageNumeral = GetPowerNumeral(1, 2);

			// {Starblade} deals up to 2 targets 2 melee damage each.
			IEnumerator dealDamageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				damageNumeral,
				DamageType.Melee,
				targetNumeral,
				false,
				0,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(dealDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(dealDamageCR);
			}

			// activate a [u]technique[/u] text.
			IEnumerator activateCR = GameController.SelectAndActivateAbility(
				DecisionMaker,
				"technique",
				optional: false,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(activateCR);
			}
			else
			{
				GameController.ExhaustCoroutine(activateCR);
			}

			yield break;
		}
	}
}