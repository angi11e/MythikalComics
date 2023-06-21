using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.WhatsHerFace
{
	public class AntiAntimemeticsCardController : RecallBaseCardController
	{
		/*
		 * Play this card next to a hero character card.
		 * Redirect all Damage that would be dealt to Hero Targets to that hero.
		 * At the start of your turn, destroy this card.
		 */

		public AntiAntimemeticsCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		// Play this card next to a hero character card.
		protected override LinqCardCriteria CustomCriteria => new LinqCardCriteria(
			(Card c) => !c.IsIncapacitatedOrOutOfGame && IsHeroCharacterCard(c),
			"hero character"
		);

		public override void AddTriggers()
		{
			// Redirect all Damage that would be dealt to Hero Targets to that hero.
			Card targetHero = GetCardThisCardIsNextTo();
			if (!targetHero.IsHeroCharacterCard)
			{
				targetHero = this.Card.Location.OwnerTurnTaker.CharacterCard;
			}

			AddRedirectDamageTrigger(
				(DealDamageAction dd) =>
					IsHero(dd.Target)
					&& dd.Target != targetHero, 
				() => targetHero
			);

			// At the start of your turn, destroy this card.
			AddStartOfTurnTrigger(
				(TurnTaker tt) => tt == this.TurnTaker,
				(PhaseChangeAction p) => GameController.DestroyCard(
					DecisionMaker,
					this.Card,
					cardSource: GetCardSource()
				),
				TriggerType.DestroySelf
			);

			base.AddTriggers();
		}
	}
}