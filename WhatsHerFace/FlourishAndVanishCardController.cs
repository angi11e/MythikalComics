﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.WhatsHerFace
{
	public class FlourishAndVanishCardController : WhatsHerFaceBaseCardController
	{
		/*
		 * Destroy an ongoing or environment card.
		 * If you destroyed an environment card, {WhatsHerFace} regains 2 hp.
		 * If you destroyed a hero card, you may draw 1 card now.
		 */

		public FlourishAndVanishCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// Destroy an ongoing or environment card.
			List<DestroyCardAction> actions = new List<DestroyCardAction>();
			IEnumerator destroyCR = GameController.SelectAndDestroyCard(
				DecisionMaker,
				new LinqCardCriteria((Card c) => c.IsEnvironment || IsOngoing(c), "ongoing or environment"),
				false,
				actions,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(destroyCR);
			}
			else
			{
				GameController.ExhaustCoroutine(destroyCR);
			}

			if (actions.Any() && actions.FirstOrDefault().WasCardDestroyed)
			{
				Card vanished = actions.FirstOrDefault().CardToDestroy.Card;

				// If you destroyed an environment card, {WhatsHerFace} regains 2 hp.
				if (vanished != null && vanished.IsEnvironment)
				{
					IEnumerator healTargetCR = GameController.GainHP(
						this.CharacterCard,
						2,
						cardSource: GetCardSource()
					);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(healTargetCR);
					}
					else
					{
						GameController.ExhaustCoroutine(healTargetCR);
					}
				}

				// If you destroyed a hero card, you may draw 1 card now.
				if (vanished != null && vanished.IsHero)
				{
					IEnumerator drawCardCR = DrawCard(HeroTurnTaker, true);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(drawCardCR);
					}
					else
					{
						GameController.ExhaustCoroutine(drawCardCR);
					}
				}
			}

			yield break;
		}
	}
}