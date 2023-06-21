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
	public class DokuMoguraCardController : CardController
	{
		private const string FirstDamageToThis = "FirstDamageToThis";

		public DokuMoguraCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
			if (IsHeroActiveInThisGame("WhatsHerFaceCharacter"))
			{
				SpecialStringMaker.ShowHasBeenUsedThisTurn(
					FirstDamageToThis,
					"{0} has already dodged damage this turn.",
					"{0} has not yet dodged damage this turn."
				);
			}
		}

		public override void AddTriggers()
		{
			// At the end of {CadaverTeam}'s turn...
			AddEndOfTurnTrigger(
				(TurnTaker tt) => tt == this.TurnTaker,
				EndOfTurnResponse,
				TriggerType.DealDamage
			);

			// If {Angille.WhatsHerFace} is active in this game...
			// ...the first time each turn this card would be dealt damage...
			// ...redirect it to the villain target with the lowest HP outside of {CadaverTeam}'s play area.
			AddFirstTimePerTurnRedirectTrigger(
				(DealDamageAction dd) =>
					dd.Target == this.Card
					&& IsHeroActiveInThisGame("WhatsHerFaceCharacter")
					&& dd.DamageSource.IsTarget,
				FirstDamageToThis,
				TargetType.LowestHP,
				(Card c) =>
					IsVillainTarget(c)
					&& GameController.IsCardVisibleToCardSource(c, GetCardSource())
					&& !c.IsAtLocationRecursive(this.TurnTaker.PlayArea)
			);

			AddAfterLeavesPlayAction(
				(GameAction ga) => ResetFlagAfterLeavesPlay(FirstDamageToThis),
				TriggerType.Hidden
			);

			base.AddTriggers();
		}

		private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
		{
			// ...this card deals each hero target 1 toxic and 1 melee damage.
			List<DealDamageAction> list = new List<DealDamageAction>
			{
				new DealDamageAction(
					GetCardSource(),
					new DamageSource(GameController, this.Card),
					null,
					1,
					DamageType.Toxic
				),
				new DealDamageAction(
					GetCardSource(),
					new DamageSource(GameController, this.Card),
					null,
					1,
					DamageType.Melee
				)
			};

			IEnumerator dealDamageCR = DealMultipleInstancesOfDamage(list, (Card c) => IsHero(c));

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
