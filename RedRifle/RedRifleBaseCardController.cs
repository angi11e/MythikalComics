using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.RedRifle
{
	public abstract class RedRifleBaseCardController : CardController
	{
		protected RedRifleBaseCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
		{
		}
	}
}