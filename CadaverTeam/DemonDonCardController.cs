using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using System.Collections;
using Handelabra;

namespace Angille.CadaverTeam
{
	public class DemonDonCardController : CardController
	{
		public DemonDonCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
			SpecialStringMaker.ShowHighestHP(2, cardCriteria: new LinqCardCriteria(
				(Card c) => c.IsHero
			));
		}

		public override void AddTriggers()
		{
			// Increase infernal damage by 1.
			AddIncreaseDamageTrigger(
				(DealDamageAction dda) => dda.DamageType == DamageType.Infernal,
				1
			);

			// At the end of {CadaverTeam}'s turn...
			AddEndOfTurnTrigger(
				(TurnTaker tt) => tt == this.TurnTaker,
				EndOfTurnResponse,
				TriggerType.DealDamage
			);

			// If {Angille.RedRifle} is active in this game, he cannot add tokens to his trueshot pool.
			AddTrigger<AddTokensToPoolAction>(
				(AddTokensToPoolAction attpa) =>
					IsHeroActiveInThisGame("RedRifleCharacter")
					// && attpa.TokenPool.CardWithTokenPool == FindCard("RedRifleCharacter")
					&& attpa.TokenPool.Identifier == "RedRifleTrueshotPool",
				(AddTokensToPoolAction attpa) => CancelAction(attpa),
				TriggerType.CancelAction,
				TriggerTiming.Before
			);

			base.AddTriggers();
		}

		private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
		{
			// ...this card deals the hero target with the second highest HP 3 fire damage and 1 infernal damage.
			List<DealDamageAction> endOfTurnDamage = new List<DealDamageAction>
			{
				new DealDamageAction(
					GetCardSource(),
					new DamageSource(GameController, this.Card),
					null,
					3,
					DamageType.Fire
				),
				new DealDamageAction(
					GetCardSource(),
					new DamageSource(GameController, this.Card),
					null,
					1,
					DamageType.Infernal
				)
			};

			IEnumerator dealDamageCR = DealMultipleInstancesOfDamageToHighestLowestHP(
				endOfTurnDamage,
				(Card c) => c.IsHero,
				HighestLowestHP.HighestHP,
				2
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(dealDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(dealDamageCR);
			}

			yield break;
		}
	}
}
