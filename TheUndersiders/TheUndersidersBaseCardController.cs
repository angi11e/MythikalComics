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
	public abstract class TheUndersidersBaseCardController : CardController
	{
		public TheUndersidersBaseCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
		}

		public bool IsEnabled(string icon)
		{
			string identifier = TheUndersiders.GetIdentifier(TheUndersiders.GetVillainFromIcon(icon));
			if (identifier == null)
			{
				return false;
			}

			Card who = base.FindCard(identifier);
			if (!who.IsFlipped && who.IsInPlayAndNotUnderCard && who.IsVillainTarget)
			{
				return true;
			}

			if (base.GameController.Game.IsChallenge && who.IsFlipped)
			{
				return true;
			}

			return false;
		}

		protected Card BitchCharacter => GameController.FindCardsWhere(
			(Card c) => c.Identifier == "BitchCharacter" && c.IsVillainCharacterCard
		).FirstOrDefault();

		protected Card FoilCharacter => GameController.FindCardsWhere(
			(Card c) => c.Identifier == "FoilCharacter" && c.IsVillainCharacterCard
		).FirstOrDefault();

		protected Card GrueCharacter => GameController.FindCardsWhere(
			(Card c) => c.Identifier == "GrueCharacter" && c.IsVillainCharacterCard
		).FirstOrDefault();

		protected Card ImpCharacter => GameController.FindCardsWhere(
			(Card c) => c.Identifier == "ImpCharacter" && c.IsVillainCharacterCard
		).FirstOrDefault();

		protected Card ParianCharacter => GameController.FindCardsWhere(
			(Card c) => c.Identifier == "ParianCharacter" && c.IsVillainCharacterCard
		).FirstOrDefault();

		protected Card RegentCharacter => GameController.FindCardsWhere(
			(Card c) => c.Identifier == "RegentCharacter" && c.IsVillainCharacterCard
		).FirstOrDefault();

		protected Card SkitterCharacter => GameController.FindCardsWhere(
			(Card c) => c.Identifier == "SkitterCharacter" && c.IsVillainCharacterCard
		).FirstOrDefault();

		protected Card TattletaleCharacter => GameController.FindCardsWhere(
			(Card c) => c.Identifier == "TattletaleCharacter" && c.IsVillainCharacterCard
		).FirstOrDefault();
	}
}
