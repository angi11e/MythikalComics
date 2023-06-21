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

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(MoveImpToHero(targetHero));
			}
			else
			{
				GameController.ExhaustCoroutine(MoveImpToHero(targetHero));
			}

			yield break;
		}

		public override void AddSideTriggers()
		{
			if (!this.Card.IsFlipped)
			{
				// At the start of the environment turn, or when that hero becomes incapacitated, move {Imp} to the play area of the next active hero in turn order.
				AddSideTrigger(AddStartOfTurnTrigger(
					(TurnTaker tt) => tt == FindEnvironment().TurnTaker,
					MoveImpToNextHero,
					TriggerType.MoveCard
				));

				AddSideTrigger(AddTrigger(
					(FlipCardAction fca) =>
						fca.CardToFlip.TurnTaker == this.Card.Location.OwnerTurnTaker
						&& fca.CardToFlip.Card.IsCharacter,
					MoveImpToNextHero,
					TriggerType.MoveCard,
					TriggerTiming.Before
				));

				// At the start of that hero's turn, {Imp} deals them 2 melee damage.
				AddSideTrigger(AddStartOfTurnTrigger(
					(TurnTaker tt) => IsHero(tt) && this.Card.Location.OwnerTurnTaker == tt,
					(PhaseChangeAction p) => GameController.DealDamageToTarget(
						new DamageSource(GameController, this.Card),
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
					(TurnTaker tt) => IsHero(tt) && !Journal.DealDamageEntriesThisTurn().Any(
						(DealDamageJournalEntry ddj) => ddj.SourceCard == tt.CharacterCard
					),
					(PhaseChangeAction p) => GameController.ShuffleTrashIntoDeck(
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
			int currentHeroIndex = heroTurnTakers.IndexOf(this.Card.Location.OwnerTurnTaker.ToHero());
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

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(MoveImpToHero(targetHero));
				}
				else
				{
					GameController.ExhaustCoroutine(MoveImpToHero(targetHero));
				}
			}

			yield break;
		}

		private IEnumerator MoveImpToHero(HeroTurnTaker targetHero)
		{
			IEnumerator moveImpCR = GameController.MoveCard(
				this.TurnTakerController,
				this.Card,
				targetHero.PlayArea,
				playCardIfMovingToPlayArea: false,
				showMessage: true,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(moveImpCR);
			}
			else
			{
				GameController.ExhaustCoroutine(moveImpCR);
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
				this.TurnTakerController,
				this.Card,
				this.TurnTaker.PlayArea,
				playCardIfMovingToPlayArea: false,
				cardSource: theSource
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(returnToBaseCR);
			}
			else
			{
				GameController.ExhaustCoroutine(returnToBaseCR);
			}

			if (!flip.CardToFlip.Card.IsFlipped)
			{
				IEnumerator untargetCR = GameController.RemoveTarget(
					this.Card,
					leavesPlayIfInPlay: true,
					theSource
				);
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(untargetCR);
				}
				else
				{
					GameController.ExhaustCoroutine(untargetCR);
				}
			}
		}
	}
}
