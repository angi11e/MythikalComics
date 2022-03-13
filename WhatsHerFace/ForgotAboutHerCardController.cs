using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.WhatsHerFace
{
	public class ForgotAboutHerCardController : RecallBaseCardController
	{
		/*
		 * Play this card next to a non-hero target.
		 * {WhatsHerFace} is immune to damage dealt by that target.
		 * If {WhatsHerFace} deals damage to that target,
		 *  or If that target leaves play, destroy this card.
		 */

		public ForgotAboutHerCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			base.SpecialStringMaker.ShowSpecialString(
				() => GetCardThisCardIsNextTo().Title
					+ " has forgotten "
					+ base.TurnTaker.NameRespectingVariant
					+ " and cannot deal damage to her.",
				() => true
			).Condition = () => base.Card.IsInPlayAndHasGameText;
		}

		// Play this card next to a non-hero target.
		protected override LinqCardCriteria CustomCriteria => new LinqCardCriteria(
			(Card c) => !c.IsHero && c.IsTarget && c.IsInPlayAndHasGameText,
			"non-hero target"
		);

		public override void AddTriggers()
		{
			// {WhatsHerFace} is immune to damage dealt by that target.
			AddImmuneToDamageTrigger(
				(DealDamageAction dda) =>
					dda.Target == base.CharacterCard
					&& dda.DamageSource.Card == GetCardThisCardIsNextTo()
			);

			// If {WhatsHerFace} deals damage to that target, destroy this card.
			AddTrigger(
				(DealDamageAction dda) =>
					dda.Target == GetCardThisCardIsNextTo()
					&& dda.DamageSource.Card == base.CharacterCard,
				DestroyThisCardResponse,
				TriggerType.DestroySelf,
				TriggerTiming.After
			);

			// If that target leaves play, destroy this card.
			AddIfTheTargetThatThisCardIsNextToLeavesPlayDestroyThisCardTrigger();

			base.AddTriggers();
		}
	}
}