using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.RedRifle
{
	public class EnergyBurstCardController : RedRifleBaseCardController
	{
		/*
		 * {RedRifle} deals each non-hero target 1 energy damage.
		 * If you have 5 or more tokens in your trueshot pool, the damage is irreducible.
		 * If you have 10 or more tokens in your trueshot pool, destroy 1 hero Ongoing or Equipment card.
		 */

		public EnergyBurstCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			SpecialStringMaker.ShowTokenPool(TrueshotPool);
		}

		public override IEnumerator Play()
		{
			// {RedRifle} deals each non-hero target 1 energy damage.
			IEnumerator dealDamageCR = GameController.DealDamage(
				DecisionMaker,
				this.CharacterCard,
				(Card c) => !IsHeroTarget(c),
				1,
				DamageType.Energy,
				// If you have 5 or more tokens in your trueshot pool, the damage is irreducible.
				TrueshotPool.CurrentValue >= 5,
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

			// If you have 10 or more tokens in your trueshot pool, destroy 1 hero Ongoing or Equipment card.
			if (TrueshotPool.CurrentValue >= 10)
			{
				IEnumerator destroyCR = GameController.SelectAndDestroyCard(
					DecisionMaker,
					new LinqCardCriteria((Card c) => IsHero(c) && (IsOngoing(c) || IsEquipment(c))),
					false,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(destroyCR);
				}
				else
				{
					GameController.ExhaustCoroutine(destroyCR);
				}
			}
			yield break;
		}
	}
}