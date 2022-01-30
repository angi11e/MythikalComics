using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.NightMare
{
	public class NightMareCharacterCardController : HeroCharacterCardController
	{
		public NightMareCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int targetNumeral = GetPowerNumeral(0, 1);
			int damageNumeral = GetPowerNumeral(1, 1);

			// Draw a card.
			IEnumerator drawCardCR = DrawCard(HeroTurnTaker);

			// Discard a card.
			IEnumerator discardCR = SelectAndDiscardCards(DecisionMaker, 1);

			// Deal 1 target 1 Melee damage.
			IEnumerator dealDamageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, base.CharacterCard),
				damageNumeral,
				DamageType.Melee,
				targetNumeral,
				false,
				targetNumeral,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(drawCardCR);
				yield return GameController.StartCoroutine(discardCR);
				yield return GameController.StartCoroutine(dealDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(drawCardCR);
				GameController.ExhaustCoroutine(discardCR);
				GameController.ExhaustCoroutine(dealDamageCR);
			}

			yield break;
		}

		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			switch (index)
			{
				case 0:
					// One player may draw a card.
					IEnumerator drawCardCR = GameController.SelectHeroToDrawCard(
						DecisionMaker,
						cardSource: GetCardSource()
					);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(drawCardCR);
					}
					else
					{
						GameController.ExhaustCoroutine(drawCardCR);
					}
					break;

				case 1:
					// Destroy a target with 1 HP.
					IEnumerator destroyWeakCR = GameController.SelectAndDestroyCard(
						DecisionMaker,
						new LinqCardCriteria(
							(Card c) => c.IsTarget && c.HitPoints.Value == 1,
							"targets with 1 HP",
							useCardsSuffix: false
						),
						optional: false,
						cardSource: GetCardSource()
					);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(destroyWeakCR);
					}
					else
					{
						GameController.ExhaustCoroutine(destroyWeakCR);
					}
					break;

				case 2:
					// Select a Hero. Increase the next Damage dealt by that hero by 2.
					IEnumerator increaseDamageCR = GameController.SelectHeroAndIncreaseNextDamageDealt(
						DecisionMaker,
						2,
						1,
						cardSource: GetCardSource()
					);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(increaseDamageCR);
					}
					else
					{
						GameController.ExhaustCoroutine(increaseDamageCR);
					}
					break;
			}
			yield break;
		}
	}
}