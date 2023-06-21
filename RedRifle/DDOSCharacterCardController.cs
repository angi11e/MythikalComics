using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.RedRifle
{
	public class DDOSCharacterCardController : HeroCharacterCardController
	{
		public DDOSCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			// no clue why the below is in the file? I guess I'll find out
			CardWithoutReplacements.TokenPools.ReorderTokenPool("RedRifleTrueshotPool");
			SpecialStringMaker.ShowTokenPool(this.Card.FindTokenPool("RedRifleTrueshotPool"));
		}

		public override IEnumerator UsePower(int index = 0)
		{
			TokenPool trueshotPool = RedRifleTrueshotPoolUtility.GetTrueshotPool(this);
			int targetNumeral = GetPowerNumeral(0, 3);
			int damageNumeral = GetPowerNumeral(1, 1);
			int tokenNumeral = GetPowerNumeral(2, 2);

			// For each target destroyed this way,
			// add 2 tokens to your trueshot pool.
			ITrigger tokenTrigger = AddTrigger(
				(DestroyCardAction d) => d.WasCardDestroyed && d.CardSource != null && d.CardSource.CardController == this,
				(DestroyCardAction d) => RedRifleTrueshotPoolUtility.AddTrueshotTokens(this, tokenNumeral),
				TriggerType.AddTokensToPool,
				TriggerTiming.After
			);

			// [i]D.D.O.S.[/i] deals up to 3 targets 1 projectile damage each.
			IEnumerator dealDamageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				damageNumeral,
				DamageType.Projectile,
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

			RemoveTrigger(tokenTrigger);
			yield break;
		}

		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			switch (index)
			{
				case 0:
					// One player may play a card.
					IEnumerator playCR = SelectHeroToPlayCard(DecisionMaker);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(playCR);
					}
					else
					{
						GameController.ExhaustCoroutine(playCR);
					}
					break;

				case 1:
					// One hero destroys one of their Equipment cards.
					List<DestroyCardAction> storedDestroy = new List<DestroyCardAction>();
					List<SelectTurnTakerDecision> storedHero = new List<SelectTurnTakerDecision>();

					IEnumerator destroySelectCR = GameController.SelectHeroToDestroyTheirCard(
						DecisionMaker,
						(httc) => new LinqCardCriteria(
							c => c.Owner == httc.TurnTaker && c.IsInPlayAndHasGameText && IsEquipment(c),
							"equipment"
						),
						additionalCriteria: new LinqTurnTakerCriteria(
							tt => tt.GetCardsWhere((Card c) => c.IsInPlayAndHasGameText && IsEquipment(c)).Any()
						),
						storedResultsTurnTaker: storedHero,
						storedResultsAction: storedDestroy,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(destroySelectCR);
					}
					else
					{
						GameController.ExhaustCoroutine(destroySelectCR);
					}

					if (DidDestroyCard(storedDestroy))
					{
						// If they do, they deal 1 target 4 energy damage.
						IEnumerator energyDamageCR = GameController.SelectTargetsAndDealDamage(
							FindHeroTurnTakerController(GetSelectedTurnTaker(storedHero).ToHero()),
							new DamageSource(GameController, GetSelectedTurnTaker(storedHero).CharacterCard),
							4,
							DamageType.Energy,
							1,
							false,
							1,
							cardSource: GetCardSource()
						);

						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(energyDamageCR);
						}
						else
						{
							GameController.ExhaustCoroutine(energyDamageCR);
						}
					}
					break;

				case 2:
					// Until the start of your turn, increase all projectile damage by 2.
					IncreaseDamageStatusEffect increaseDamageStatusEffect = new IncreaseDamageStatusEffect(2);
					increaseDamageStatusEffect.UntilStartOfNextTurn(this.TurnTaker);
					increaseDamageStatusEffect.DamageTypeCriteria.AddType(DamageType.Projectile);
					IEnumerator effectCR = AddStatusEffect(increaseDamageStatusEffect);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(effectCR);
					}
					else
					{
						GameController.ExhaustCoroutine(effectCR);
					}
					break;
			}
			yield break;
		}
	}
}