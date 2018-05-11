﻿/*
Copyright (c) 2017, Lars Brubaker, John Lewin
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

The views and conclusions contained in the software and documentation are those
of the authors and should not be interpreted as representing official policies,
either expressed or implied, of the FreeBSD Project.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using MatterHackers.Agg;
using MatterHackers.Agg.Platform;
using MatterHackers.Agg.UI;
using MatterHackers.Localizations;
using MatterHackers.MatterControl.ConfigurationPage;
using MatterHackers.MatterControl.CustomWidgets;
using MatterHackers.MatterControl.SlicerConfiguration;
using MatterHackers.MeshVisualizer;
using MatterHackers.RenderOpenGl;

namespace MatterHackers.MatterControl.PartPreviewWindow
{
	public class GCodeLayerDetailsView : FlowLayoutWidget
	{
		private TextWidget massTextWidget;
		private TextWidget costTextWidget;
		private BedConfig sceneContext;

		private EventHandler unregisterEvents;

		public GCodeLayerDetailsView(GCodeDetails gcodeDetails, BedConfig sceneContext, int dataPointSize, int headingPointSize)
			: base(FlowDirection.TopToBottom)
		{
			this.sceneContext = sceneContext;
			var margin = new BorderDouble(0, 9, 0, 3);

			// local function to add setting
			TextWidget AddSetting(string title, string value, GuiWidget parentWidget)
			{
				parentWidget.AddChild(
					new TextWidget(title + ":", textColor: ActiveTheme.Instance.PrimaryTextColor, pointSize: headingPointSize)
					{
						HAnchor = HAnchor.Left
					});

				var textWidget = new TextWidget(value, textColor: ActiveTheme.Instance.PrimaryTextColor, pointSize: dataPointSize)
				{
					HAnchor = HAnchor.Left,
					Margin = margin,
					AutoExpandBoundsToText = true
				};

				parentWidget.AddChild(textWidget);

				return textWidget;
			}

			GuiWidget layerTime = AddSetting("Time".Localize(), "", this);
			GuiWidget layerHeight = AddSetting("Height".Localize(), "", this);
			GuiWidget layerWidth = AddSetting("Layer Top".Localize(), "", this);
			GuiWidget layerFanSpeeds = AddSetting("Fan Speed".Localize(), "", this);

			void UpdateLayerDisplay(object sender, EventArgs e)
			{
				layerTime.Text = gcodeDetails.LayerTime(sceneContext.ActiveLayerIndex);
				layerHeight.Text = $"{gcodeDetails.GetLayerHeight(sceneContext.ActiveLayerIndex):0.###}";
				layerWidth.Text = $"{gcodeDetails.GetLayerTop(sceneContext.ActiveLayerIndex):0.###}";
				var fanSpeed = gcodeDetails.GetLayerFanSpeeds(sceneContext.ActiveLayerIndex);
				layerFanSpeeds.Text = string.IsNullOrWhiteSpace(fanSpeed) ? "Unchanged" : fanSpeed;
			}

			sceneContext.ActiveLayerChanged += UpdateLayerDisplay;
			// and do the initial setting
			UpdateLayerDisplay(this, null);
		}

		public override void OnClosed(ClosedEventArgs e)
		{
			unregisterEvents?.Invoke(this, null);
			base.OnClosed(e);
		}
	}
}