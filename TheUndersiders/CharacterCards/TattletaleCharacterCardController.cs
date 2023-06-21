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
	public class TattletaleCharacterCardController : TheUndersidersVillainCardController
	{
		public TattletaleCharacterCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
		}

		public override void AddSideTriggers()
		{
			if (!this.Card.IsFlipped)
			{
				// increase damage dealt by villain character cards by 1.
				AddSideTrigger(AddIncreaseDamageTrigger(
					(DealDamageAction dd) => dd.DamageSource.IsTarget && dd.DamageSource.Card.IsVillainCharacterCard,
					1
				));

				// At the end of the villain turn, destroy {H-2} hero ongoing cards. The villain target with the highest HP deals 2 projectile damage to each hero with any ongoing cards in play.
				AddSideTrigger(AddEndOfTurnTrigger(
					(TurnTaker tt) => tt == this.TurnTaker,
					DashHopesResponse,
					new TriggerType[]
					{
						TriggerType.DestroyCard,
						TriggerType.DealDamage
					}
				));

				// Treat {Tattle} effects as active. (taken care of by the cards)
			}
			else
			{
				// At the start of the villain turn, each player may destroy 1 of their ongoing or equipment cards. If they do not, they discard 1 card.
				AddSideTrigger(AddStartOfTurnTrigger(
					(TurnTaker tt) => tt == this.TurnTaker,
					AttritionResponse,
					new TriggerType[]
					{
						TriggerType.DestroyCard,
						TriggerType.DiscardCard
					}
				));

			}
			base.AddSideTriggers();
		}

		private IEnumerator DashHopesResponse(PhaseChangeAction p)
		{
			IEnumerator dashHopesCR = GameController.SelectAndDestroyCards(
				DecisionMaker,
				new LinqCardCriteria((Card c) =>
					IsHero(c) && IsOngoing(c),
					"hero ongoing"
				),
				H - 2,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(dashHopesCR);
			}
			else
			{
				GameController.ExhaustCoroutine(dashHopesCR);
			}

			IEnumerable<HeroTurnTaker> source = (from c in FindCardsWhere(
				(Card c) => c.IsInPlayAndHasGameText && IsHero(c) && IsHero(c.Owner) && IsOngoing(c)
			) select c.Owner.ToHero()).Distinct();

			IEnumerable<Card> heroCharacterCards = source.SelectMany((HeroTurnTaker h) => h.CharacterCards);

			IEnumerator punishHoardersCR = DealDamage(
				null,
				(Card c) => heroCharacterCards.Contains(c),
				2,
				DamageType.Projectile,
				damageSourceInfo: new TargetInfo(
					HighestLowestHP.HighestHP,
					1,
					1,
					new LinqCardCriteria((Card c) => IsVillainTarget(c), "the villain target with the highest HP")
				)
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(punishHoardersCR);
			}
			else
			{
				GameController.ExhaustCoroutine(punishHoardersCR);
			}

			yield break;
		}

		private IEnumerator AttritionResponse(PhaseChangeAction p)
		{
			IEnumerator eachHeroDoesCR = GameController.SelectTurnTakersAndDoAction(
				DecisionMaker,
				new LinqTurnTakerCriteria((TurnTaker tt) => !tt.IsIncapacitatedOrOutOfGame && IsHero(tt)),
				SelectionType.DestroyCard,
				EachHeroAttrition,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(eachHeroDoesCR);
			}
			else
			{
				GameController.ExhaustCoroutine(eachHeroDoesCR);
			}

			yield break;
		}

		private IEnumerator EachHeroAttrition(TurnTaker tt)
		{
			List<DestroyCardAction> storedResults = new List<DestroyCardAction>();
			IEnumerator destructionCR = GameController.SelectAndDestroyCard(
				FindHeroTurnTakerController(tt.ToHero()),
				new LinqCardCriteria(
					(Card c) => c.IsInPlayAndNotUnderCard && c.Owner == tt && (IsOngoing(c) || IsEquipment(c)),
					"ongoing or equipment"
				),
				optional: true,
				storedResults,
				this.Card,
				GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(destructionCR);
			}
			else
			{
				GameController.ExhaustCoroutine(destructionCR);
			}

			if (storedResults.Count() < 1)
			{
				IEnumerator discardCR = SelectAndDiscardCards(
					FindHeroTurnTakerController(tt.ToHero()),
					1
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(discardCR);
				}
				else
				{
					GameController.ExhaustCoroutine(discardCR);
				}
			}

			yield break;
		}
	}
}
