﻿using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cauldron.Drift;
using Handelabra;

namespace Angille.Drift
{
	public class BlueMythikalDriftCharacterCardController : MythikalDriftCharacterCardController
	{
		protected override bool shouldRunSetUp => false;

		public BlueMythikalDriftCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}
	}
}