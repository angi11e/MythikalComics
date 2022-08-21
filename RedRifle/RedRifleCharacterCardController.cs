using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.RedRifle
{
	public class RedRifleCharacterCardController : HeroCharacterCardController
	{
		// private int AddTokensNumeral => GetPowerNumeral(0, 1);

		public RedRifleCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			// no clue why the below is in the file? I guess I'll find out
			CardWithoutReplacements.TokenPools.ReorderTokenPool("RedRifleTrueshotPool");
			base.SpecialStringMaker.ShowTokenPool(base.Card.FindTokenPool("RedRifleTrueshotPool"));
		}

		public override void AddStartOfGameTriggers()
		{
			// base.AddStartOfGameTriggers();
		}

		public override IEnumerator UsePower(int index = 0)
		{
			TokenPool trueshotPool = RedRifleTrueshotPoolUtility.GetTrueshotPool(this);
			int tokenNumeral = GetPowerNumeral(0, 1);
			int drawNumeral = GetPowerNumeral(1, 1);
			
			// Add 1 token to your trueshot pool. Draw a card.
			if (trueshotPool != null)
			{
				IEnumerator addTokensCR = RedRifleTrueshotPoolUtility.AddTrueshotTokens(this, tokenNumeral);
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(addTokensCR);
				}
				else
				{
					GameController.ExhaustCoroutine(addTokensCR);
				}
			}

			IEnumerator drawCardCR = DrawCards(this.HeroTurnTakerController, drawNumeral);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(drawCardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(drawCardCR);
			}

			yield break;
		}

		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			switch (index)
			{
				case 0:
					// One hero may use a power.
					IEnumerator playCardCR = GameController.SelectHeroToUsePower(DecisionMaker);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(playCardCR);
					}
					else
					{
						GameController.ExhaustCoroutine(playCardCR);
					}
					break;

				case 1:
					// One hero may deal 1 target 1 projectile damage.
					IEnumerator dealDamageCR = GameController.SelectHeroToSelectTargetAndDealDamage(
						DecisionMaker,
						1,
						DamageType.Projectile,
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
					// Up to two hero ongoing cards may be played now.
					IEnumerator playOngoingCR = GameController.SelectTurnTakersAndDoAction(
						DecisionMaker,
						new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsHero && !tt.IsIncapacitatedOrOutOfGame),
						SelectionType.PlayCard,
						PlayOngoingResponse,
						2,
						cardSource: GetCardSource()
					);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(playOngoingCR);
					}
					else
					{
						GameController.ExhaustCoroutine(playOngoingCR);
					}
					break;
			}
			yield break;
		}

		private IEnumerator PlayOngoingResponse(TurnTaker tt)
		{
			IEnumerator playOngoingCR = SelectAndPlayCardFromHand(
				FindHeroTurnTakerController(tt.ToHero()),
				cardCriteria: new LinqCardCriteria((Card c) => c.IsOngoing)
			);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(playOngoingCR);
			}
			else
			{
				GameController.ExhaustCoroutine(playOngoingCR);
			}
			yield break;
		}
	}
}