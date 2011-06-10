using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace InterTalk
{
    /// <summary>
    /// This class is the singleton interface for the event manager core. Every class that wants to subscribe to a certain event has to register to
    /// this manager, every class firing a condition/event has to talk through this class.
    /// </summary>
    public sealed class InterManagerCore
    {

        #region Singleton Implementation

        private static readonly InterManagerCore instance = new InterManagerCore();

        private InterManagerCore() { }

        public static InterManagerCore Instance
        {
            get
            {
                return instance;
            }
        }

        #endregion

        /// <summary>
        /// The delegate structure for all the events in the complex condition layer structure.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="InterTalk.CoreEventArgs"/> instance containing the event data.</param>
        public delegate void CoreEventHandler(object sender, CoreEventArgs args);

        /// <summary>
        /// List that contains all the different conditions in different layers (for multi-purpose event handling)
        /// </summary>
        private List<Dictionary<String, CoreEventHandler>> complexConditionLayers;

        /// <summary>
        /// Dictionary containing primitive immediate calls to events. All the methods that subscribe to these events are simpler to handle
        /// than their complex counterpart and don't require the subscribers to care about the context of the called event.
        /// </summary>
        private Dictionary<String, MethodInfo> primitiveCondition;
        
    }
}
