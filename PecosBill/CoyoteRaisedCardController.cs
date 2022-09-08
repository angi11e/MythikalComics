using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.PecosBill
{
	public class CoyoteRaisedCardController : PecosBillBaseCardController
	{
		/*
		 * when this card enters play, you may draw a card.
		 * 
		 * increase damage dealt by {PecosBill} by 1.
		 * 
		 * POWER
		 * {PecosBill} deals up to 3 targets 1 melee damage each.
		 */

		public CoyoteRaisedCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			base.AddTriggers();

			// increase damage dealt by {PecosBill} by 1.
			AddIncreaseDamageTrigger((DealDamageAction dd) => dd.DamageSource.IsSameCard(this.CharacterCard), 1);
		}

		public override IEnumerator Play()
		{
			// when this card enters play, you may draw a card.
			return DrawCards(HeroTurnTakerController, 1, true);
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int targetNumeral = GetPowerNumeral(0, 3);
			int damageNumeral = GetPowerNumeral(1, 1);

			// {PecosBill} deals up to 3 targets 1 melee damage each.
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

			yield break;
		}
	}
}