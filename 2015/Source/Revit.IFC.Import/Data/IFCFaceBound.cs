﻿//
// Revit IFC Import library: this library works with Autodesk(R) Revit(R) to import IFC files.
// Copyright (C) 2013  Autodesk, Inc.
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.IFC;
using Revit.IFC.Common.Utility;
using Revit.IFC.Common.Enums;
using Revit.IFC.Import.Enums;
using Revit.IFC.Import.Geometry;
using Revit.IFC.Import.Utility;

namespace Revit.IFC.Import.Data
{
    public class IFCFaceBound : IFCRepresentationItem
    {
        IFCLoop m_Bound = null;

        bool m_Orientation = true;

        bool m_IsOuter = false;

        /// <summary>
        /// Return the defining loop of the face boundary.
        /// </summary>
        public IFCLoop Bound
        {
            get { return m_Bound; }
            protected set { m_Bound = value; }
        }

        /// <summary>
        /// Return the orientation of the defining loop of the face boundary.
        /// </summary>
        public bool Orientation
        {
            get { return m_Orientation; }
            protected set { m_Orientation = value; }
        }

        /// <summary>
        /// Returns whether this is an outer boundary (TRUE) or an inner boundary (FALSE).
        /// </summary>
        public bool IsOuter
        {
            get { return m_IsOuter; }
            protected set { m_IsOuter = value; }
        }

        protected IFCFaceBound()
        {
        }

        override protected void Process(IFCAnyHandle ifcFaceBound)
        {
            base.Process(ifcFaceBound);

            IFCAnyHandle ifcLoop = IFCImportHandleUtil.GetRequiredInstanceAttribute(ifcFaceBound, "Bound", true);

            Bound = IFCLoop.ProcessIFCLoop(ifcLoop);

            IsOuter = (IFCAnyHandleUtil.IsSubTypeOf(ifcFaceBound, IFCEntityType.IfcFaceOuterBound));
        }

        /// <summary>
        /// Create geometry for a particular representation item.
        /// </summary>
        /// <param name="shapeEditScope">The geometry creation scope.</param>
        /// <param name="lcs">Local coordinate system for the geometry, without scale.</param>
        /// <param name="scaledLcs">Local coordinate system for the geometry, including scale, potentially non-uniform.</param>
        /// <param name="forceSolid">True if we require a solid.</param>
        /// <param name="guid">The guid of an element for which represntation is being created.</param>
        protected override void CreateShapeInternal(IFCImportShapeEditScope shapeEditScope, Transform lcs, Transform scaledLcs, bool forceSolid, string guid)
        {
            base.CreateShapeInternal(shapeEditScope, lcs, scaledLcs, forceSolid, guid);

            IList<XYZ> loopVertexes = Bound.LoopVertexes;
            if (loopVertexes == null || loopVertexes.Count == 0)
                throw new InvalidOperationException("#" + Id + ": missing loop vertexes, ignoring.");

            if (!Orientation)
                loopVertexes.Reverse();

            // Apply the transform
            IList<XYZ> transformedVertexes = new List<XYZ>();
            foreach (XYZ vertex in loopVertexes)
            {
              transformedVertexes.Add(lcs.OfPoint(vertex));
            }

            shapeEditScope.AddLoopVertexes(transformedVertexes);
        }

        protected IFCFaceBound(IFCAnyHandle ifcFaceBound)
        {
            Process(ifcFaceBound);
        }

        /// <summary>
        /// Create an IFCFaceBound object from a handle of type IfcFaceBound.
        /// </summary>
        /// <param name="ifcFaceBound">The IFC handle.</param>
        /// <returns>The IFCFaceBound object.</returns>
        public static IFCFaceBound ProcessIFCFaceBound(IFCAnyHandle ifcFaceBound)
        {
            if (IFCAnyHandleUtil.IsNullOrHasNoValue(ifcFaceBound))
            {
                IFCImportFile.TheLog.LogNullError(IFCEntityType.IfcFaceBound);
                return null;
            }

            IFCEntity faceBound;
            if (!IFCImportFile.TheFile.EntityMap.TryGetValue(ifcFaceBound.StepId, out faceBound))
                faceBound = new IFCFaceBound(ifcFaceBound);
            return (faceBound as IFCFaceBound);
        }
    }
}