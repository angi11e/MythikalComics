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
	public class WatchfulSwarmCardController : TheUndersidersBaseCardController
	{
		public WatchfulSwarmCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// Whenever a villain character target would take damage, redirect it to the {WatchfulSwarm} card with the lowest HP.
			AddTrigger(
				(DealDamageAction dd) => dd.Target.IsVillainCharacterCard && dd.Amount != 0,
				(DealDamageAction dd) => RedirectDamage(
					dd,
					TargetType.LowestHP,
					(Card c) => c.Identifier.ToString() == "WatchfulSwarm"
				),
				TriggerType.RedirectDamage,
				TriggerTiming.Before
			);

			// Spider: This card is immune to melee and projectile damage.
			AddImmuneToDamageTrigger(
				(DealDamageAction dd) =>
					IsEnabled("spider")
					&& dd.Target == base.Card
					&& (dd.DamageType == DamageType.Melee || dd.DamageType == DamageType.Projectile)
			);

			// Tattle: When this card is destroyed, the hero with the most cards in hand must discard a card.
			AddWhenDestroyedTrigger(
				BugsEatThingsResponse,
				new TriggerType[] { TriggerType.DiscardCard },
				(DestroyCardAction d) => IsEnabled("tattle")
			);

			base.AddTriggers();
		}

		private IEnumerator BugsEatThingsResponse(DestroyCardAction d)
		{
			List<TurnTaker> storedResults = new List<TurnTaker>();
			IEnumerator mostCardsCR = FindHeroWithMostCardsInHand(storedResults);
			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(mostCardsCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(mostCardsCR);
			}
			if (storedResults.Count() <= 0)
			{
				yield break;
			}

			TurnTaker turnTaker = storedResults.First();
			if (turnTaker != null && turnTaker.IsHero)
			{
				IEnumerator discardCR = SelectAndDiscardCards(
					FindHeroTurnTakerController(turnTaker.ToHero()),
					1
				);
				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(discardCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(discardCR);
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
