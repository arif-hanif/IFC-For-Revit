﻿//
// BIM IFC library: this library works with Autodesk(R) Revit(R) to export IFC files containing model geometry.
// Copyright (C) 2012  Autodesk, Inc.
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
using Autodesk.Revit.DB.Mechanical;
using BIM.IFC.Utility;
using BIM.IFC.Toolkit;
using BIM.IFC.Exporter.PropertySet;

namespace BIM.IFC.Exporter
{
    /// <summary>
    /// Provides methods to export a Revit element as IfcZone.
    /// </summary>
    class ZoneExporter
    {
        /// <summary>
        /// Exports an element as a zone.
        /// </summary>
        /// <param name="exporterIFC">The ExporterIFC object.</param>
        /// <param name="element">The element.</param>
        /// <param name="productWrapper">The ProductWrapper.</param>
        public static void ExportZone(ExporterIFC exporterIFC, Zone element,
            ProductWrapper productWrapper)
        {
            if (element == null)
                return;

            HashSet<IFCAnyHandle> spaceHnds = new HashSet<IFCAnyHandle>();
            
            SpaceSet spaces = element.Spaces;
            foreach (Space space in spaces)
            {
                if (space == null)
                    continue;

                IFCAnyHandle spaceHnd = ExporterCacheManager.SpatialElementHandleCache.Find(space.Id);
                if (!IFCAnyHandleUtil.IsNullOrHasNoValue(spaceHnd))
                    spaceHnds.Add(spaceHnd);
            }

            if (spaceHnds.Count == 0)
                return;

            IFCFile file = exporterIFC.GetFile();

            using (IFCTransaction tr = new IFCTransaction(file))
            {
                string guid = GUIDUtil.CreateGUID(element);
                IFCAnyHandle ownerHistory = exporterIFC.GetOwnerHistoryHandle();
                string name = NamingUtil.GetNameOverride(element, NamingUtil.GetIFCName(element));
                string description = NamingUtil.GetDescriptionOverride(element, null);
                string objectType = NamingUtil.GetObjectTypeOverride(element, exporterIFC.GetFamilyName());

                IFCAnyHandle zoneHnd = IFCInstanceExporter.CreateZone(file, guid, ownerHistory, name, description, objectType);

                productWrapper.AddElement(element, zoneHnd);

                string relAssignsGuid = GUIDUtil.CreateSubElementGUID(element, (int) IFCZoneSubElements.RelAssignsToGroup);
                IFCInstanceExporter.CreateRelAssignsToGroup(file, relAssignsGuid, ownerHistory, null, null, spaceHnds, null, zoneHnd);

                tr.Commit();
                return;
            }
        }
    }
}
