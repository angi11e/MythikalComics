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
			base.SpecialStringMaker.ShowTokenPool(base.TrueshotPool);
		}

		public override IEnumerator Play()
		{
			int damageNumeral = GetPowerNumeral(0, 1);
			int lowTokenNumeral = GetPowerNumeral(1, 5);
			int highTokenNumeral = GetPowerNumeral(2, 10);
			int destroyNumeral = GetPowerNumeral(3, 1);

			// If you have 5 or more tokens in your trueshot pool, the damage is irreducible.
			bool isIrreducible = false;
			if (base.TrueshotPool.CurrentValue >= lowTokenNumeral)
			{
				isIrreducible = true;
			}

			// {RedRifle} deals each non-hero target 1 energy damage.
			IEnumerator dealDamageCR = GameController.DealDamage(
				DecisionMaker,
				base.Card,
				(Card c) => !c.IsHero,
				damageNumeral,
				DamageType.Energy,
				isIrreducible,
				cardSource: GetCardSource()
			);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(dealDamageCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(dealDamageCR);
			}

			// If you have 10 or more tokens in your trueshot pool, destroy 1 hero Ongoing or Equipment card.
			if (base.TrueshotPool.CurrentValue >= highTokenNumeral)
			{
				IEnumerator destroyCR = GameController.SelectAndDestroyCard(
					DecisionMaker,
					new LinqCardCriteria((Card c) => c.IsHero && (c.IsOngoing || IsEquipment(c))),
					false,
					cardSource: GetCardSource()
				);

				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(destroyCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(destroyCR);
				}
			}
			yield break;
		}
	}
}