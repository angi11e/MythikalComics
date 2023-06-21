using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Speedrunner
{
	public class TurboControllerCardController : SpeedrunnerBaseCardController
	{
		/*
		 * If you play a card outside of your play phase, you may draw a card.
		 * If you use a power outside of your power phase, {Speedrunner} deals 1 target 2 fire damage.
		 * 
		 * POWER
		 * {Speedrunner} deals 1 target 1 melee damage X times, where X = the number of your glitch and strat cards in play.
		 */

		public TurboControllerCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// If you play a card outside of your play phase, you may draw a card.
			AddTrigger(
				(PlayCardAction pca) =>
					pca.ResponsibleTurnTaker == this.HeroTurnTaker
					&& (Game.ActiveTurnPhase.TurnTaker != TurnTaker || Game.ActiveTurnPhase.Phase != Phase.PlayCard),
				(PlayCardAction pca) => DrawCard(HeroTurnTaker, true),
				TriggerType.DrawCard,
				TriggerTiming.After
			);

			// If you use a power outside of your power phase, {Speedrunner} deals 1 target 2 fire damage.
			AddTrigger(
				(UsePowerAction upa) =>
					upa.HeroUsingPower == this.HeroTurnTakerController
					&& (Game.ActiveTurnPhase.TurnTaker != TurnTaker || Game.ActiveTurnPhase.Phase != Phase.UsePower),
				(UsePowerAction upa) => GameController.SelectTargetsAndDealDamage(
					DecisionMaker,
					new DamageSource(GameController, this.CharacterCard),
					2,
					DamageType.Fire,
					1,
					false,
					1,
					cardSource: GetCardSource()
				),
				TriggerType.DealDamage,
				TriggerTiming.After
			);

			base.AddTriggers();
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int targetNumeral = GetPowerNumeral(0, 1);
			int damageNumeral = GetPowerNumeral(1, 1);

			// ...where X = the number of your glitch and strat cards in play.
			int strikeNumeral = GameController.FindCardsWhere(
				new LinqCardCriteria((Card c) => c.IsInPlayAndNotUnderCard && !c.IsOneShot && (IsGlitch(c) || IsStrat(c)))
			).Count();

			if (strikeNumeral > 0)
			{
				List<DealDamageAction> storedDamage = new List<DealDamageAction>();

				if (strikeNumeral == 1)
				{
					// not sure why, but SelectTargetsAndDealMultipleInstancesOfDamage() doesn't seem to like one instance?
					IEnumerator dealDamageCR = GameController.SelectTargetsAndDealDamage(
						DecisionMaker,
						new DamageSource(GameController, this.CharacterCard),
						damageNumeral,
						DamageType.Melee,
						1,
						false,
						1,
						storedResultsDamage: storedDamage,
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
					List<DealDamageAction> damageInfo = new List<DealDamageAction>();
					DealDamageAction plink = new DealDamageAction(
						GetCardSource(),
						new DamageSource(GameController, this.CharacterCard),
						null,
						damageNumeral,
						DamageType.Melee
					);

					for (int i = 0; i < strikeNumeral; i++)
					{
						damageInfo.Add(plink);
					}
			
					// {Speedrunner} deals 1 target 1 melee damage X times...
					IEnumerator dealDamageCR = SelectTargetsAndDealMultipleInstancesOfDamage(
						damageInfo,
						minNumberOfTargets: targetNumeral,
						maxNumberOfTargets: targetNumeral,
						storedResultsAction: storedDamage
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

				// If this damage destroys that target, destroy this card.
				if (storedDamage.Any((DealDamageAction dd) => dd.DidDestroyTarget))
				{
					IEnumerator destroyCR = GameController.DestroyCard(
						DecisionMaker,
						this.Card,
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
			}
			else
			{
				IEnumerator emptyMessageCR = GameController.SendMessageAction(
					"no glitch or strat cards in play. save the frames!",
					Priority.Medium,
					GetCardSource(),
					showCardSource: true
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(emptyMessageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(emptyMessageCR);
				}
			}

			yield break;
		}
	}
}