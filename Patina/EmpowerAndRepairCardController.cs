﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Patina
{
	public class EmpowerAndRepairCardController : CardController
	{
		// card text here

		public EmpowerAndRepairCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			yield break;
		}

		public override void AddTriggers()
		{
			base.AddTriggers();
		}

		public override IEnumerator UsePower(int index = 0)
		{
			yield break;
		}
	}
}