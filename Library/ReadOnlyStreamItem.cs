﻿/*
Copyright (c) 2018, John Lewin
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
using System.Threading.Tasks;

namespace MatterHackers.MatterControl.Library
{
	public class ReadOnlyStreamItem : ILibraryAssetStream
	{
		private Func<Task<StreamAndLength>> streamSource;

		public ReadOnlyStreamItem(Func<Task<StreamAndLength>> streamSource)
		{
			this.streamSource = streamSource;
		}

		public string ContentType { get; set; }

		public string ID { get; set; }

		public string Name { get; set; }

		public string FileName { get; set; }

		public bool IsProtected { get; set; }

		public bool IsVisible { get; set; }

		public DateTime DateCreated { get; } = DateTime.Now;

		public DateTime DateModified { get; } = DateTime.Now;

		public long FileSize => 0;

		public string AssetPath => "";

		public bool LocalContentExists => true;

		public string Category => "General";

		public Task<StreamAndLength> GetStream(Action<double, string> progress)
		{
			return streamSource?.Invoke();
		}
	}
}
