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
	public class LynneCardController : CardController
	{
		public LynneCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// At the start of {CadaverTeam}'s turn, each target in his play area gains 2 HP.
			AddStartOfTurnTrigger(
				(TurnTaker tt) => tt == this.TurnTaker,
				(PhaseChangeAction p) => GameController.GainHP(
					DecisionMaker,
					(Card c) => c.IsAtLocationRecursive(this.TurnTaker.PlayArea),
					2,
					cardSource: GetCardSource()
				),
				TriggerType.GainHP
			);

			// At the end of {CadaverTeam}'s turn...
			AddEndOfTurnTrigger(
				(TurnTaker tt) => tt == this.TurnTaker,
				// ...reveal the top card of {CadaverTeam}'s deck.
				(PhaseChangeAction p) => RevealCards_PutSomeIntoPlay_DiscardRemaining(
					this.TurnTakerController,
					this.TurnTaker.Deck,
					1,
					// If it is a one-shot, put it into play. Otherwise, discard it.
					new LinqCardCriteria((Card c) => c.IsOneShot)
				),
				TriggerType.RevealCard
			);

			// ...whenever a hero card is played outside that hero's play area...
			AddTrigger(
				(CardEntersPlayAction cepa) =>
					IsHero(cepa.CardEnteringPlay)
					&& !cepa.CardEnteringPlay.IsAtLocationRecursive(cepa.CardEnteringPlay.Owner.PlayArea)
					// If {Angille.Theurgy} is active in this game...
					&& IsHeroActiveInThisGame("TheurgyCharacter"),
				// ...this card deals each hero target 1 lightning damage.
				(CardEntersPlayAction cepa) => DealDamage(
					this.Card,
					(Card c) => IsHero(c),
					1,
					DamageType.Lightning
				),
				TriggerType.DealDamage,
				TriggerTiming.After
			);

			base.AddTriggers();
		}
	}
}
