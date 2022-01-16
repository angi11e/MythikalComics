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

		protected Card BitchCharacter => base.FindCard("BitchCharacter");
		protected Card FoilCharacter => base.FindCard("BitchCharacter");
		protected Card GrueCharacter => base.FindCard("GrueCharacter");
		protected Card ImpCharacter => base.FindCard("ImpCharacter");
		protected Card ParianCharacter => base.FindCard("ParianCharacter");
		protected Card RegentCharacter => base.FindCard("RegentCharacter");
		protected Card SkitterCharacter => base.FindCard("SkitterCharacter");
		protected Card TattletaleCharacter => base.FindCard("TattletaleCharacter");
	}
}
