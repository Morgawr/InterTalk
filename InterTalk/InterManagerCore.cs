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

        ////TODO - Adding a way to handle checking for the instance that fired the event.

        #region Singleton Implementation

        private static readonly InterManagerCore instance = new InterManagerCore();

        private InterManagerCore() 
        {
            conditions = new List<Dictionary<string, List<Delegate>>>();
            conditionParams = new List<Dictionary<string, object[][]>>();
        }

        public static InterManagerCore Instance
        {
            get
            {
                return instance;
            }
        }

        #endregion

        #region Fields


        /// <summary>
        /// List that contains all the different conditions in different layers (for multi-purpose event handling)
        /// </summary>
        private List<Dictionary<String, List<Delegate>>> conditions;

        private List<Dictionary<String,object[][]>> conditionParams;

        #endregion

        #region Methods

        /// <summary>
        /// This method registers a new listener to a certain event with certain parameters as a callback function
        /// </summary>
        /// <param name="depth">Depth of the layer, multiple layers with same condition keys can be registered.</param>
        /// <param name="condition">String name for the conditon/event to listen to.</param>
        /// <param name="handler">Callback method (delegate) when the event is fired.</param>
        /// <param name="args">Array of objects for the delegate method parameters.</param>
        /// <returns>Returns an int representing the ID of the registered listener. (Required to unregister a listener).</returns>
        public int Register(int depth, String condition, Delegate handler, params object[] args)
        {
            while (conditions.Count <= depth)
            {
                conditions.Add(new Dictionary<String, List<Delegate>>());
                conditionParams.Add(new Dictionary<string, object[][]>());
            }

            if (!conditions[depth].ContainsKey(condition))
            {
                conditions[depth][condition] = new List<Delegate>();
                conditionParams[depth][condition] = new object[100][];
            }
            int id;
            if((id = ObtainFirstNullID(depth,condition)) != -1)
            {
                conditions[depth][condition][id] = handler;
            }
            else
            {
                conditions[depth][condition].Add(handler);
                id = conditions[depth][condition].IndexOf(handler);
            }
            
            conditionParams[depth][condition][id] = args;

            return id;
        }


        /// <summary>
        /// Obtains the first null ID in the conditions list.
        /// </summary>
        /// <param name="depth">The layer number for the condition we need.</param>
        /// <param name="condition">The condition we are going to check.</param>
        /// <returns></returns>
        private int ObtainFirstNullID(int depth, string condition)
        {
            for (int i = 0; i < conditions[depth][condition].Count; i++)
            {
                if (conditions[depth][condition][i] == null)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// This method fires the event for all the callbacks registered to a certain event.
        /// </summary>
        /// <param name="depth">Level of depth for the layer for the invoked event.</param>
        /// <param name="condition">Condition for the invoked event.</param>
        public void Invoke(int depth, String condition)
        {
            foreach(Delegate d in conditions[depth][condition])
            {
                if(d != null)
                    d.DynamicInvoke(conditionParams[depth][condition][conditions[depth][condition].IndexOf(d)]);
            }
        }

        /// <summary>
        /// This method unregisters a listener's callback method from a certain condition.
        /// </summary>
        /// <param name="depth">Level of depth for the layer of the condition.</param>
        /// <param name="condition">Condition the listener wants to stop listening.</param>
        /// <param name="ID">ID of the listener.</param>
        public void Unregister(int depth, String condition, int ID)
        {
            conditions[depth][condition][ID] = null;
            conditionParams[depth][condition][ID] = null;
        }

        #endregion
    }
}
