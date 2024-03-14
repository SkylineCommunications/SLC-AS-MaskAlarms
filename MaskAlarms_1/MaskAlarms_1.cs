/*
****************************************************************************
*  Copyright (c) 2024,  Skyline Communications NV  All Rights Reserved.    *
****************************************************************************

By using this script, you expressly agree with the usage terms and
conditions set out below.
This script and all related materials are protected by copyrights and
other intellectual property rights that exclusively belong
to Skyline Communications.

A user license granted for this script is strictly for personal use only.
This script may not be used in any way by anyone without the prior
written consent of Skyline Communications. Any sublicensing of this
script is forbidden.

Any modifications to this script by the user are only allowed for
personal use and within the intended purpose of the script,
and will remain the sole responsibility of the user.
Skyline Communications will not be responsible for any damages or
malfunctions whatsoever of the script resulting from a modification
or adaptation by the user.

The content of this script is confidential information.
The user hereby agrees to keep this confidential information strictly
secret and confidential and not to disclose or reveal it, in whole
or in part, directly or indirectly to any person, entity, organization
or administration without the prior written consent of
Skyline Communications.

Any inquiries can be addressed to:

    Skyline Communications NV
    Ambachtenstraat 33
    B-8870 Izegem
    Belgium
    Tel.    : +32 51 31 35 69
    Fax.    : +32 51 31 01 29
    E-mail  : info@skyline.be
    Web     : www.skyline.be
    Contact : Ben Vandenberghe

****************************************************************************
Revision History:

DATE        VERSION     AUTHOR          COMMENTS

14/03/2024  1.0.0.1     HAN, Skyline    Initial version
****************************************************************************
*/

namespace MaskAlarms_1
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Core.DataMinerSystem.Automation;
    using Skyline.DataMiner.Core.DataMinerSystem.Common;
    using Skyline.DataMiner.Net.Helper;
    using Skyline.DataMiner.Net.Messages;
    using ElementState = Skyline.DataMiner.Core.DataMinerSystem.Common.ElementState;

    /// <summary>
    /// Represents a DataMiner Automation script.
    /// </summary>
    public class Script
    {
        /// <summary>
        /// The script entry point.
        /// </summary>
        /// <param name="engine">Link with SLAutomation process.</param>
        public void Run(IEngine engine)
        {
            IDms thisDms = engine.GetDms();

            string name = GetActualName(engine.GetScriptParam("Name").Value);
            string type = GetActualName(engine.GetScriptParam("Type").Value);
            string operation = engine.GetScriptParam("Operation").Value;

            if (type.Equals("View"))
            {
                HanldeView(engine, thisDms, name, operation);
            }
            else if (type.Equals("Service"))
            {
                var service = engine.FindService(name);
                HandleServices(engine, thisDms, operation, new Service[] { service });
            }
            else if (type.Equals("Element"))
            {
                var element = engine.FindElement(name);
                HandleElements(engine, thisDms, operation, new Element[] { element });
            }
            else
            {
                engine.GenerateInformation("No name was provided. Please provide either a View, Service or Element name");
            }
        }

        private static string GetActualName(string viewName)
        {
            return viewName.Replace("[", string.Empty).Replace("]", string.Empty).Replace("\"", string.Empty);
        }

        private static void HanldeView(IEngine engine, IDms thisDms, string viewName, string operation)
        {
            Service[] services = engine.FindServicesInView(viewName);

            HandleServices(engine, thisDms, operation, services);

            Element[] elements = engine.FindElementsInView(viewName);

            HandleElements(engine, thisDms, operation, elements);
        }

        private static void HandleServices(IEngine engine, IDms thisDms, string operation, Service[] services)
        {
            if (services.IsNullOrEmpty())
            {
                engine.GenerateInformation($"View does not contain any services");
                return;
            }

            foreach (var service in services)
            {
                foreach (var children in service.RawInfo.Children)
                {
                    var element = engine.FindElement(children.DataMinerID, children.ElementID);
                    var dmsElement = thisDms.GetElement(element.ElementName);
                    var elementState = dmsElement.State;

                    if (elementState == ElementState.Stopped)
                    {
                        engine.GenerateInformation($"Element {element.Name} is stopped");
                        continue;
                    }

                    if (operation.Equals("Mask") && elementState != ElementState.Masked)
                    {
                        element.Mask("Mask");
                    }
                    else if (operation.Equals("Unmask"))
                    {
                        element.Unmask();
                    }
                }
            }
        }

        private static void HandleElements(IEngine engine, IDms thisDms, string operation, Element[] elements)
        {
            if (elements.IsNullOrEmpty())
            {
                engine.GenerateInformation($"View does not contain any elements");
                return;
            }

            foreach (var element in elements)
            {
                var elementState = thisDms.GetElement(element.ElementName).State;

                if (elementState == ElementState.Stopped)
                {
                    engine.GenerateInformation($"Element {element.Name} is stopped");
                    continue;
                }

                if (operation.Equals("Mask") && elementState != ElementState.Masked)
                {
                    element.Mask("Mask");
                }
                else if (operation.Equals("Unmask"))
                {
                    element.Unmask();
                }
            }
        }
    }
}