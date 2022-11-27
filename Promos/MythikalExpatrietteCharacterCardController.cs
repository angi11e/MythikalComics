using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.Expatriette
{
	public class MythikalExpatrietteCharacterCardController : HeroCharacterCardController
	{
		public MythikalExpatrietteCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int targetNumeral = GetPowerNumeral(0, 1);
			int damageNumeral = GetPowerNumeral(1, 3);
			int drawNumeral = GetPowerNumeral(2, 2);

			// Discard a card.
			List<DiscardCardAction> storedDiscard = new List<DiscardCardAction>();
			IEnumerator discardCR = SelectAndDiscardCards(
				DecisionMaker,
				1,
				false,
				1,
				storedDiscard
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(discardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(discardCR);
			}

			// If it was an ammo card...
			if (DidDiscardCards(storedDiscard) && storedDiscard.FirstOrDefault().CardToDiscard.IsAmmo)
			{
				// ...{Expatriette} deals 1 target 3 irreducible projectile damage.
				IEnumerator dealDamageCR = GameController.SelectTargetsAndDealDamage(
					DecisionMaker,
					new DamageSource(GameController, this.CharacterCard),
					damageNumeral,
					DamageType.Projectile,
					targetNumeral,
					false,
					targetNumeral,
					true,
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
			}
			else
			{
				// Otherwise, draw 2 cards.
				IEnumerator drawCR = DrawCards(DecisionMaker,2);
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(drawCR);
				}
				else
				{
					GameController.ExhaustCoroutine(drawCR);
				}
			}

			yield break;
		}

		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			switch (index)
			{
				case 0:
					// One Hero may use a power.
					IEnumerator usePowerCR = GameController.SelectHeroToUsePower(DecisionMaker);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(usePowerCR);
					}
					else
					{
						GameController.ExhaustCoroutine(usePowerCR);
					}
					break;
				case 1:
					// One Hero may deal 1 target 1 irreducible projectile damage.
					IEnumerator dealDamageCR = GameController.SelectHeroToSelectTargetAndDealDamage(
						DecisionMaker,
						1,
						DamageType.Projectile,
						true,
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
					break;
				case 2:
					// Each player may discard a card. Any player that does draws 2 cards.
					List<DiscardCardAction> discardedCards = new List<DiscardCardAction>();
					IEnumerator discardCR = GameController.EachPlayerDiscardsCards(
						0,
						1,
						discardedCards,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(discardCR);
					}
					else
					{
						GameController.ExhaustCoroutine(discardCR);
					}

					foreach (DiscardCardAction item in discardedCards)
					{
						if (item.WasCardDiscarded)
						{
							IEnumerator drawCR = DrawCards(item.HeroTurnTakerController, 2);
							if (UseUnityCoroutines)
							{
								yield return GameController.StartCoroutine(drawCR);
							}
							else
							{
								GameController.ExhaustCoroutine(drawCR);
							}
						}
					}
					break;
			}
			yield break;
		}
	}
}