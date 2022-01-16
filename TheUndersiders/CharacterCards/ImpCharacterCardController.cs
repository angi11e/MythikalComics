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
	public class ImpCharacterCardController : TheUndersidersVillainCardController
	{
		public ImpCharacterCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// When this card enters play, place her in the play area of the first active hero in turn order.
			HeroTurnTaker targetHero = Game.HeroTurnTakers.Where(htt => !htt.IsIncapacitatedOrOutOfGame).FirstOrDefault();

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(MoveImpToHero(targetHero));
			}
			else
			{
				base.GameController.ExhaustCoroutine(MoveImpToHero(targetHero));
			}

			yield break;
		}

		public override void AddSideTriggers()
		{
			if (!base.Card.IsFlipped)
			{
				// At the start of the environment turn, or when that hero becomes incapacitated, move {Imp} to the play area of the next active hero in turn order.
				AddSideTrigger(AddStartOfTurnTrigger(
					(TurnTaker tt) => tt == FindEnvironment().TurnTaker,
					MoveImpToNextHero,
					TriggerType.MoveCard
				));

				AddSideTrigger(AddTrigger(
					(FlipCardAction fca) =>
//						FindCardsWhere((Card c) => c.IsHeroCharacterCard && !c.IsIncapacitatedOrOutOfGame).Count() > 0
//						&& !IsTurnTakerActiveInThisGame(base.Card.Location.OwnerName)
						fca.CardToFlip.TurnTaker == base.Card.Location.OwnerTurnTaker
						&& fca.CardToFlip.Card.IsCharacter,
					MoveImpToNextHero,
					TriggerType.MoveCard,
					TriggerTiming.Before
				));

				// At the start of that hero's turn, {Imp} deals them 2 melee damage.
				AddSideTrigger(AddStartOfTurnTrigger(
					(TurnTaker tt) => tt.IsHero && base.Card.Location.OwnerTurnTaker == tt,
					(PhaseChangeAction p) => base.GameController.DealDamageToTarget(
						new DamageSource(base.GameController, base.Card),
						p.ToPhase.TurnTaker.CharacterCard,
						2,
						DamageType.Melee,
						cardSource: GetCardSource()
					),
					TriggerType.DealDamage
				));

				// {Imp} is immune to damage from sources outside that hero's play area.
				AddSideTrigger(AddImmuneToDamageTrigger(
					dda => dda.Target == Card
						&& dda.DamageSource.Card.Location.OwnerTurnTaker != Card.Location.OwnerTurnTaker
				));

				// Treat {Mask} effects as active. (this is done by the cards)
			}
			else
			{
				// At the end of each hero's turn, if they haven't dealt any damage that turn, they shuffle their trash into their deck.
				AddSideTrigger(AddEndOfTurnTrigger(
					(TurnTaker tt) => tt.IsHero && !Journal.DealDamageEntriesThisTurn().Any(
						(DealDamageJournalEntry ddj) => ddj.SourceCard == tt.CharacterCard
					),
					(PhaseChangeAction p) => base.GameController.ShuffleTrashIntoDeck(
						FindTurnTakerController(p.FromPhase.TurnTaker)
					),
					TriggerType.ShuffleTrashIntoDeck
				));
			}
			base.AddSideTriggers();
		}

		private IEnumerator MoveImpToNextHero(GameAction ga)
		{
			List<HeroTurnTaker> heroTurnTakers = Game.HeroTurnTakers.Where(htt => !htt.IsIncapacitatedOrOutOfGame).ToList();
			int currentHeroIndex = heroTurnTakers.IndexOf(base.Card.Location.OwnerTurnTaker.ToHero());
			HeroTurnTaker targetHero = null;

			if (heroTurnTakers.Count() > 1 && currentHeroIndex > -1)
			{
				if (currentHeroIndex == heroTurnTakers.Count() - 1)
				{
					targetHero = heroTurnTakers[0];
				}
				else
				{
					targetHero = heroTurnTakers[currentHeroIndex + 1];
				}

				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(MoveImpToHero(targetHero));
				}
				else
				{
					base.GameController.ExhaustCoroutine(MoveImpToHero(targetHero));
				}
			}

			yield break;
		}

		private IEnumerator MoveImpToHero(HeroTurnTaker targetHero)
		{
			IEnumerator moveImpCR = GameController.MoveCard(
				base.TurnTakerController,
				base.Card,
				targetHero.PlayArea,
				playCardIfMovingToPlayArea: false,
				cardSource: GetCardSource()
			);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(moveImpCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(moveImpCR);
			}

			yield break;
		}

		public override IEnumerator BeforeFlipCardImmediateResponse(FlipCardAction flip)
		{
			CardSource theSource = flip.CardSource;
			if (theSource == null && flip.ActionSource != null)
			{
				theSource = flip.ActionSource.CardSource;
			}
			if (theSource == null)
			{
				theSource = GetCardSource();
			}

			// When {Imp} flips to this side, move her back to the villain play area.
			IEnumerator returnToBaseCR = GameController.MoveCard(
				base.TurnTakerController,
				base.Card,
				base.TurnTaker.PlayArea,
				playCardIfMovingToPlayArea: false,
				cardSource: theSource
			);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(returnToBaseCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(returnToBaseCR);
			}

			if (!flip.CardToFlip.Card.IsFlipped)
			{
				IEnumerator untargetCR = base.GameController.RemoveTarget(
					base.Card,
					leavesPlayIfInPlay: true,
					theSource
				);
				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(untargetCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(untargetCR);
				}
			}
		}
	}
}
