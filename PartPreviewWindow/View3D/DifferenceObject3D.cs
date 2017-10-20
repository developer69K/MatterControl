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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MatterHackers.DataConverters3D;
using MatterHackers.PolygonMesh;
using MatterHackers.VectorMath;

namespace MatterHackers.MatterControl.PartPreviewWindow.View3D
{
	public class MeshWrapper : Object3D
	{
		public MeshWrapper()
		{
		}

		public MeshWrapper(IObject3D child, string ownerId)
		{
			Children.Add(child);

			this.OwnerID = ownerId;
			this.MaterialIndex = child.MaterialIndex;
			this.ItemType = child.ItemType;
			this.OutputType = child.OutputType;
			this.Color = child.Color;
			this.Mesh = child.Mesh;

			this.Matrix = child.Matrix;
			child.Matrix = Matrix4X4.Identity;
		}
	}

	public class DifferenceItem : MeshWrapper
	{
		public DifferenceItem()
		{
		}

		public DifferenceItem(IObject3D child, string ownerId, bool makeHole)
			: base(child, ownerId)
		{
			if (makeHole)
			{
				OutputType = PrintOutputTypes.Hole;
			}
		}
	}

	public class DifferenceGroup : Object3D
	{
		public DifferenceGroup(SafeList<IObject3D> children)
		{
			Children.Modify((list) =>
			{
				foreach (var child in children)
				{
					list.Add(child);
				}
			});

			bool first = true;
			// now wrap every first decendant that has a mesh
			foreach (var child in this.VisibleMeshes().Where((o) => o.Mesh != null))
			{
				// wrap the child in a DifferenceItem
				child.Parent.Children.Modify((list) =>
				{
					list.Remove(child);
					list.Add(new DifferenceItem(child, this.ID, !first));
					first = false;
				});
			}

			ProcessBooleans();
		}

		async void ProcessBooleans()
		{
			// spin up a task to remove holes from the objects in the group
			await Task.Run(() =>
			{
				var container = this;
				var participants = this.VisibleMeshes().Where((obj) => obj.OwnerID == this.ID);
				var removeObjects = participants.Where((obj) => obj.OutputType == PrintOutputTypes.Hole);
				var keepObjects = participants.Where((obj) => obj.OutputType != PrintOutputTypes.Hole);

				if (removeObjects.Any()
					&& keepObjects.Any())
				{
					foreach (var remove in removeObjects)
					{
						foreach (var keep in keepObjects)
						{
							var transformedRemove = Mesh.Copy(remove.Mesh, CancellationToken.None);
							transformedRemove.Transform(remove.WorldMatrix());

							var transformedKeep = Mesh.Copy(keep.Mesh, CancellationToken.None);
							transformedKeep.Transform(keep.WorldMatrix());

							transformedKeep = PolygonMesh.Csg.CsgOperations.Subtract(transformedKeep, transformedRemove);
							var inverse = keep.WorldMatrix();
							inverse.Invert();
							transformedKeep.Transform(inverse);
							keep.Mesh = transformedKeep;
						}

						remove.Visible = false;
					}
				}
			});
		}
	}

	public class IntersectItem : MeshWrapper
	{
		public IntersectItem()
		{
		}

		public IntersectItem(IObject3D child, string ownerId, bool makeHole)
			: base(child, ownerId)
		{
			if (makeHole)
			{
				OutputType = PrintOutputTypes.Hole;
			}
		}
	}

	public class IntersectGroup : Object3D
	{
		public IntersectGroup(SafeList<IObject3D> children)
		{
			Children.Modify((list) =>
			{
				foreach (var child in children)
				{
					list.Add(child);
				}
			});

			bool first = true;
			// now wrap every first decendant that has a mesh
			foreach (var child in this.VisibleMeshes().Where((o) => o.Mesh != null))
			{
				// wrap the child in a IntersectItem
				child.Parent.Children.Modify((list) =>
				{
					list.Remove(child);
					list.Add(new IntersectItem(child, this.ID, !first));
					first = false;
				});
			}

			ProcessBooleans();
		}

		async void ProcessBooleans()
		{
			// spin up a task to remove holes from the objects in the group
			await Task.Run(() =>
			{
				var container = this;
				var participants = this.VisibleMeshes().Where((obj) => obj.OwnerID == this.ID);
				var removeObjects = participants.Where((obj) => obj.OutputType == PrintOutputTypes.Hole);
				var keepObjects = participants.Where((obj) => obj.OutputType != PrintOutputTypes.Hole);

				if (removeObjects.Any()
					&& keepObjects.Any())
				{
					foreach (var remove in removeObjects)
					{
						foreach (var keep in keepObjects)
						{
							var transformedRemove = Mesh.Copy(remove.Mesh, CancellationToken.None);
							transformedRemove.Transform(remove.WorldMatrix());

							var transformedKeep = Mesh.Copy(keep.Mesh, CancellationToken.None);
							transformedKeep.Transform(keep.WorldMatrix());

							transformedKeep = PolygonMesh.Csg.CsgOperations.Intersect(transformedKeep, transformedRemove);
							var inverse = keep.WorldMatrix();
							inverse.Invert();
							transformedKeep.Transform(inverse);
							keep.Mesh = transformedKeep;
						}

						remove.Visible = false;
					}
				}
			});
		}
	}

	public class PaintItem : MeshWrapper
	{
		public PaintItem()
		{
		}

		public PaintItem(IObject3D child, string ownerId, bool makeHole)
			: base(child, ownerId)
		{
			if (makeHole)
			{
				OutputType = PrintOutputTypes.Hole;
			}
		}
	}

	public class PaintMaterialGroup : Object3D
	{
		public PaintMaterialGroup(SafeList<IObject3D> children)
		{
			Children.Modify((list) =>
			{
				foreach (var child in children)
				{
					list.Add(child);
				}
			});

			bool first = true;
			// now wrap every first decendant that has a mesh
			foreach (var child in this.VisibleMeshes().Where((o) => o.Mesh != null))
			{
				// wrap the child in a PaintItem
				child.Parent.Children.Modify((list) =>
				{
					list.Remove(child);
					list.Add(new PaintItem(child, this.ID, !first));
					first = false;
				});
			}

			ProcessBooleans();
		}

		async void ProcessBooleans()
		{
			// spin up a task to remove holes from the objects in the group
			await Task.Run(() =>
			{
				var container = this;
				var participants = this.VisibleMeshes().Where((obj) => obj.OwnerID == this.ID).ToList();
				var removeObjects = participants.Where((obj) => obj.OutputType == PrintOutputTypes.Hole).ToList();
				var keepObjects = participants.Where((obj) => obj.OutputType != PrintOutputTypes.Hole).ToList();

				if (removeObjects.Any()
					&& keepObjects.Any())
				{
					foreach (var remove in removeObjects)
					{
						foreach (var keep in keepObjects)
						{
							var transformedRemove = Mesh.Copy(remove.Mesh, CancellationToken.None);
							transformedRemove.Transform(remove.WorldMatrix());

							var transformedKeep = Mesh.Copy(keep.Mesh, CancellationToken.None);
							transformedKeep.Transform(keep.WorldMatrix());

							// remove the paint from the original
							var transformedKeep2 = PolygonMesh.Csg.CsgOperations.Subtract(transformedKeep, transformedRemove);
							var inverseKeep = keep.WorldMatrix();
							inverseKeep.Invert();
							transformedKeep2.Transform(inverseKeep);
							keep.Mesh = transformedKeep2;

							// intersect the paint with the original
							transformedRemove = PolygonMesh.Csg.CsgOperations.Intersect(transformedKeep, transformedRemove);
							var inverseRemove = remove.WorldMatrix();
							inverseRemove.Invert();
							transformedRemove.Transform(inverseRemove);
							remove.Mesh = transformedRemove;
						}

						// set it to the correct extruder
						remove.MaterialIndex = 1;
						// now set it to the new solid color
						remove.OutputType = PrintOutputTypes.Solid;
					}
				}
			});
		}
	}
}